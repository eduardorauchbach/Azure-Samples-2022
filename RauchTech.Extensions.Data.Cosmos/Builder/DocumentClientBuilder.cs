using Microsoft.Azure.Documents.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RauchTech.Extensions.Data.Cosmos
{
    public static class DocumentClientBuilder
    {
        public static DocumentClient Build(string connectionString)
        {
            DocumentClient documentClient;

            DbConnectionStringBuilder cosmosDbSettings;
            Uri serviceEndpoint;

            string accountEndpoint;
            string authKey;


            cosmosDbSettings = new DbConnectionStringBuilder { ConnectionString = connectionString };

            accountEndpoint = cosmosDbSettings.GetValue("AccountEndpoint");

            serviceEndpoint = new Uri(accountEndpoint);

            authKey = cosmosDbSettings.GetValue("AccountKey");

            documentClient = new DocumentClient(serviceEndpoint, authKey, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Ignore,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });

            documentClient.OpenAsync().Wait();

            return documentClient;
        }

        private static string GetValue(this DbConnectionStringBuilder cosmosDbSettings, string key)
        {
            return !cosmosDbSettings.TryGetValue(key, out var value)
                 ? throw new ArgumentNullException(key, "Bad ConnectionString")
                 : value.ToString();
        }
    }
}
