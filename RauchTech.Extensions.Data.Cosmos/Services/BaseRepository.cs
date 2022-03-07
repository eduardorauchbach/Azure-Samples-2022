using Microsoft.Azure.CosmosDB.BulkExecutor;
using Microsoft.Azure.CosmosDB.BulkExecutor.BulkImport;
using Microsoft.Azure.CosmosDB.BulkExecutor.BulkUpdate;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Newtonsoft.Json;
using RauchTech.Extensions.Data.Cosmos.Models;
using System.Linq.Expressions;
using System.Reflection;

namespace RauchTech.Extensions.Data.Cosmos.Services
{
    public abstract class BaseRepository<TEntity> where TEntity : EntityRoot
    {
        #region Constants
        protected static FeedOptions Options { get; } = new FeedOptions { EnableCrossPartitionQuery = true };
        #endregion

        #region Properties
        protected Uri CollectionUri { get; }
        protected string CollectionDataBaseName { get; set; }
        protected string CollectionName { get; set; }

        protected IDocumentClient BaseDocumentClient { get; }
        private readonly DocumentCollection _documentCollection;
        private readonly string _partitionKeyName;
        private readonly string? _partitionKeyPropertyName;
        #endregion

        #region Constructor
        protected BaseRepository(IDocumentClient documentClient)
        {
            BaseDocumentClient = documentClient;

            CollectionUri = UriFactory.CreateDocumentCollectionUri(CollectionDataBaseName, CollectionName);

            _documentCollection = GetCollectionIfExists(BaseDocumentClient as DocumentClient, CollectionDataBaseName, CollectionName);
            _partitionKeyName = _documentCollection.PartitionKey.Paths[0].Replace("/", "");
            _partitionKeyPropertyName = GetPartitionKeyPropertyName();
        }
        #endregion

        #region Bulk Cliente

        private BulkExecutor GetBulkClient()
        {
            return new BulkExecutor(BaseDocumentClient as DocumentClient, _documentCollection);
        }

        /// <summary>
        /// Get the collection if it exists, null if it doesn't.
        /// </summary>
        /// <returns>The requested collection.</returns>
        private static DocumentCollection GetCollectionIfExists(DocumentClient client, string databaseName, string collectionName)
        {
            DocumentCollection? collection;

            if (GetDatabaseIfExists(client, databaseName) == null)
            {
                throw new Exception($"The database '{databaseName}' does not exist");
            }

            collection = client.CreateDocumentCollectionQuery(UriFactory.CreateDatabaseUri(databaseName))
                               .Where(c => c.Id == collectionName).AsEnumerable().FirstOrDefault();

            if (collection == null)
            {
                throw new Exception($"The collection '{collectionName}' does not exist in database '{databaseName}' ");
            }

            return collection;
        }

        /// <summary>
        /// Get the database if it exists, null if it doesn't.
        /// </summary>
        /// <returns>The requested database.</returns>
        private static Database? GetDatabaseIfExists(DocumentClient client, string databaseName)
        {
            return client.CreateDatabaseQuery().Where(d => d.Id == databaseName).AsEnumerable().FirstOrDefault();
        }
        #endregion

        #region Change Data
        public Task AddAsync(TEntity entities)
        {
            if (entities is null)
            {
                throw new ArgumentNullException(nameof(entities));
            }

            RequestOptions? options = ResolveRequestOptions(entities);
            return BaseDocumentClient.CreateDocumentAsync(CollectionUri, entities, options);
        }

        public Task UpdateAsync(TEntity entities)
        {
            if (entities is null)
            {
                throw new ArgumentNullException(nameof(entities));
            }

            RequestOptions? options = ResolveRequestOptions(entities);
            return BaseDocumentClient.UpsertDocumentAsync(CollectionUri, entities, options);
        }

        public Task DeleteAsync(TEntity entities)
        {
            if (entities is null)
            {
                throw new ArgumentNullException(nameof(entities));
            }

            Uri? documentUri = UriFactory.CreateDocumentUri(CollectionDataBaseName, CollectionName, entities.Id.ToString());
            RequestOptions? options = ResolveRequestOptions(entities);

            return BaseDocumentClient.DeleteDocumentAsync(documentUri, options);
        }
        #endregion

        #region Change Data (Bulk)

        public async Task BulkInsertAsync(IEnumerable<TEntity> entities)
        {
            if (entities?.Any() != true)
            {
                return;
            }

            BulkExecutor bulkExecutor = GetBulkClient();
            await bulkExecutor.InitializeAsync().ConfigureAwait(false);

            BulkImportResponse bulkInsertResponse = await bulkExecutor.BulkImportAsync(entities).ConfigureAwait(false);
        }

        public async Task BulkUpdateAsync(IEnumerable<TEntity> entities)
        {
            if (entities?.Any() != true)
            {
                return;
            }

            List<UpdateItem> updateItems = new List<UpdateItem>();
            List<UpdateOperation> updateOperations;
            (string, object?) valuePair;

            BulkExecutor bulkExecutor;

            foreach (TEntity? e in entities)
            {
                updateOperations = new List<UpdateOperation>();

                foreach (PropertyInfo? p in e.GetType().GetProperties())
                {
                    valuePair = GetField(e, p);

                    if (valuePair.Item2 == null)
                    {
                        continue;
                    }
                    else if (p.PropertyType == typeof(string))
                    {
                        updateOperations.Add(new SetUpdateOperation<string>
                        (
                            valuePair.Item1,
                            (string)valuePair.Item2
                        ));
                    }
                    else if (p.PropertyType == typeof(char) || p.PropertyType == typeof(char?))
                    {
                        updateOperations.Add(new SetUpdateOperation<char?>
                        (
                            valuePair.Item1,
                            (char?)valuePair.Item2
                        ));
                    }
                    else if (p.PropertyType == typeof(Guid) || p.PropertyType == typeof(Guid?))
                    {
                        updateOperations.Add(new SetUpdateOperation<Guid?>
                        (
                            valuePair.Item1,
                            (Guid?)valuePair.Item2
                        ));
                    }
                    else if (p.PropertyType == typeof(bool) || p.PropertyType == typeof(bool?))
                    {
                        updateOperations.Add(new SetUpdateOperation<bool?>
                        (
                            valuePair.Item1,
                            (bool?)valuePair.Item2
                        ));
                    }
                    else if (long.TryParse(Convert.ToString(valuePair.Item2), out long numberInt))
                    {
                        updateOperations.Add(new SetUpdateOperation<long?>
                        (
                            valuePair.Item1,
                            numberInt
                        ));
                    }
                    else if (double.TryParse(Convert.ToString(valuePair.Item2), out double numberDec))
                    {
                        updateOperations.Add(new SetUpdateOperation<double?>
                        (
                            valuePair.Item1,
                            numberDec
                        ));
                    }
                    else
                    {
                        updateOperations.Add(new SetUpdateOperation<object>
                        (
                            valuePair.Item1,
                            valuePair.Item2
                        ));
                    }
                }

                updateItems.Add(new UpdateItem
                (
                    e.Id.ToString(),
                    e.GetType().GetProperty(_partitionKeyPropertyName).GetValue(e).ToString(),
                    updateOperations
                ));
            }

            bulkExecutor = GetBulkClient();
            await bulkExecutor.InitializeAsync().ConfigureAwait(false);

            _ = await bulkExecutor.BulkUpdateAsync(updateItems).ConfigureAwait(false);

            #region Local

            static (string, object?) GetField(TEntity entity, PropertyInfo prop)
            {
                string name = string.Empty;
                object? value = null;

                foreach (object attr in prop.GetCustomAttributes(true))
                {
                    name += (attr as JsonPropertyAttribute).PropertyName;
                }

                value = prop.GetValue(entity);

                return (name, value);
            }

            #endregion
        }

        public async Task BulkDeleteAsync(IEnumerable<TEntity> entities)
        {
            if (entities?.Any() != true)
            {
                return;
            }
            BulkExecutor bulkExecutor = GetBulkClient();
            await bulkExecutor.InitializeAsync().ConfigureAwait(false);

            List<Tuple<string, string>>? pkIdTuplesToDelete = GeneratePartitionKeyDocumentIdTuplesToBulkDelete();
            _ = await bulkExecutor.BulkDeleteAsync(pkIdTuplesToDelete).ConfigureAwait(false);

            #region Local

            List<Tuple<string, string>> GeneratePartitionKeyDocumentIdTuplesToBulkDelete()
            {
                List<Tuple<string, string>>? pkIdTuplesToDelete = new List<Tuple<string, string>>();

                foreach (TEntity? item in entities)
                {
                    string? keyValue = item.GetType().GetProperty(_partitionKeyPropertyName).GetValue(item).ToString();
                    pkIdTuplesToDelete.Add(new Tuple<string, string>(keyValue, item.Id.ToString()));
                }

                return pkIdTuplesToDelete;
            }

            #endregion
        }

        #endregion

        #region Get All
        public Task<ICollection<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>>? predicate = null)
        {
            predicate ??= (x => true);

            IQueryable<TEntity>? query = BaseDocumentClient.CreateDocumentQuery<TEntity>(CollectionUri, Options)
                                           .Where(predicate);

            return CreateEntities(query);
        }

        public Task<ICollection<TReturn>> GetAllAsync<TReturn>(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, TReturn>> selector)
        {
            IQueryable<TReturn>? query = BaseDocumentClient.CreateDocumentQuery<TEntity>(CollectionUri, Options)
                                           .Where(predicate)
                                           .Select(selector);

            return CreateEntities(query);
        }

        #endregion

        #region Get Filters
        public Task<TEntity?> GetAsync(TEntity entity)
        {
            return entity is null
                 ? throw new ArgumentNullException(nameof(entity))
                 : GetByIdAsync(entity.Id);
        }

        public async Task<TEntity?> GetAsync(Expression<Func<TEntity, bool>> predicate)
        {
            ICollection<TEntity>? query = await GetAllAsync(predicate).ConfigureAwait(false);
            return query.FirstOrDefault();
        }

        public Task<ICollection<TEntity>> GetAllAsync()
        {
            return GetAllAsync(_ => true);
        }

        public async Task<bool> Any(Expression<Func<TEntity, bool>> predicate)
        {
            ICollection<TEntity>? query = await GetAllAsync(predicate).ConfigureAwait(false);

            return query.Any();
        }

        public Task<TEntity?> GetByIdAsync(Guid id)
        {
            return Guid.Empty.Equals(id)
                 ? throw new ArgumentNullException(nameof(id))
                 : GetAsync(entity => entity.Id == id);
        }
        #endregion

        #region Utils
        protected async Task<ICollection<TReturn>> CreateEntities<TReturn>(IQueryable<TReturn> query)
        {
            List<TReturn>? entities = new List<TReturn>();
            IDocumentQuery<TReturn>? documentQuery = query.AsDocumentQuery();

            while (documentQuery.HasMoreResults)
            {
                FeedResponse<TReturn>? partialData = await documentQuery.ExecuteNextAsync<TReturn>().ConfigureAwait(false);
                entities.AddRange(partialData);
            }

            return entities;
        }

        protected virtual RequestOptions ResolveRequestOptions(TEntity entity)
        {
            if (_partitionKeyPropertyName != null)
            {
                return new RequestOptions
                {
                    PartitionKey = new PartitionKey(entity.GetType().GetProperty(_partitionKeyPropertyName).GetValue(entity))
                };
            }

            return new RequestOptions
            {
                PartitionKey = PartitionKey.None,
            };
        }

        private string? GetPartitionKeyPropertyName()
        {
            string? propertyName = null;
            JsonPropertyAttribute? jAttr;

            foreach (PropertyInfo? p in typeof(TEntity).GetProperties())
            {
                foreach (object attr in p.GetCustomAttributes(true))
                {
                    jAttr = attr as JsonPropertyAttribute;
                    if (jAttr != null)
                    {
                        if (jAttr.PropertyName == _partitionKeyName)
                        {
                            propertyName = p.Name;
                            break;
                        }
                    }

                }
                if (propertyName != null)
                {
                    break;
                }
            }

            return propertyName;
        }

        #endregion
    }

}
