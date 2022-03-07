using Functions.Clients.Domain.Repositories.Interfaces;
using Functions.Clients.Domain.Repositories.Sample;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using RauchTech.Extensions.Data.Cosmos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Functions.Clients.Domain.Repositories
{
    internal static class RepositoryDomain
    {
        public static void RegisterDocument(string documentConnection, IServiceCollection services)
        {
            _ = services.AddSingleton<IDocumentClient>(_ => DocumentClientBuilder.Build(documentConnection));

            _ = services
                   .AddScoped<ISampleRepository, SampleRepository>();
        }

        public static IFunctionsHostBuilder RegisterAzureRepositories(this IFunctionsHostBuilder builder, string jobsConnection, string cosmosConnection)
        {
            //if (string.IsNullOrWhiteSpace(jobsConnection))
            //{
            //    throw new ArgumentNullException(nameof(jobsConnection));
            //}

            if (string.IsNullOrWhiteSpace(cosmosConnection))
            {
                throw new ArgumentNullException(nameof(cosmosConnection));
            }

            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            //string blobConnection = Environment.GetEnvironmentVariable(jobsConnection);
            string documentConnection = Environment.GetEnvironmentVariable(cosmosConnection);

            //RepositoryDomain.RegisterBlob(blobConnection, services);
            RegisterDocument(documentConnection, builder.Services);

            return builder;
        }
    }
}
