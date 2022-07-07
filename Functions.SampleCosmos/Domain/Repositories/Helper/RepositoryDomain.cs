using Functions.SampleCosmos.Domain.Repositories.Code;
using Microsoft.Azure.Documents;
using Microsoft.Extensions.DependencyInjection;
using RauchTech.Extensions.Data.Cosmos;
using System;

namespace Functions.SampleCosmos.Domain.Repositories.Helper
{
    internal static class RepositoryDomain
    {
        public static void RegisterDocument(string documentConnection, IServiceCollection services)
        {
            _ = services.AddSingleton<IDocumentClient>(_ => DocumentClientBuilder.Build(documentConnection));

            _ = services
                   .AddScoped<ISampleRepository, SampleRepository>();
        }

        public static IServiceCollection RegisterAzureRepositories(this IServiceCollection services, string jobsConnection, string cosmosConnection)
        {
            //if (string.IsNullOrWhiteSpace(jobsConnection))
            //{
            //    throw new ArgumentNullException(nameof(jobsConnection));
            //}

            if (string.IsNullOrWhiteSpace(cosmosConnection))
            {
                throw new ArgumentNullException(nameof(cosmosConnection));
            }

            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            //string blobConnection = Environment.GetEnvironmentVariable(jobsConnection);
            string documentConnection = Environment.GetEnvironmentVariable(cosmosConnection);

            //RepositoryDomain.RegisterBlob(blobConnection, services);
            RegisterDocument(documentConnection, services);

            return services;
        }
    }
}
