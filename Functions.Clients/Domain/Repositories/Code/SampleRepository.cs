using Functions.Clients.Domain.Models;
using RauchTech.Extensions.Data.Cosmos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;

namespace Functions.Clients.Domain.Repositories.Code
{
    public sealed class SampleRepository : BaseRepository<SampleModel>, ISampleRepository
    {
        #region Constructors
        public SampleRepository(IDocumentClient client)
            : base(client)
        {
            CollectionDataBaseName = "SampleDataBase";
            CollectionName = "Sample";
        }
        #endregion

        public async Task<DateTime?> GetLastSincDate() //Just an example
        {
            IQueryable<SampleModel> query = BaseDocumentClient.CreateDocumentQuery<SampleModel>(CollectionUri, Options)
                                           .Where(x => x.LastUpdate != null)
                                           .OrderBy(x => x.LastUpdate).Take(1);

            //Serve para pegar a menor data de atualização, caso algum problema ocorra no meio da sincronia, ele retoma tudo, mas no geral vai diminuir bastante o volume de sincronia
            SampleModel sample = (await CreateEntities(query)).FirstOrDefault();

            return sample?.LastUpdate;
        }
    }
}
