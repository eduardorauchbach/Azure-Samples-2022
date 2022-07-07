using Functions.Common;
using Functions.SampleCosmos.Domain.Functions.Helper;
using Functions.SampleCosmos.Domain.Repositories.Helper;
using Functions.SampleCosmos.Domain.Services.Code.Builder;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.DependencyInjection;
using System;

[assembly: CLSCompliant(true)]
[assembly: FunctionsStartup(typeof(Functions.SampleCosmos.Startup))]

namespace Functions.SampleCosmos
{
    public sealed class Startup : FunctionsStartup
    {
        #region Constants

        private const string Name = "Sample";

        private const string CosmosStorage = "CosmosStorage";
        private const string JobsStorage = "BlobStorage";

        #endregion

        public override void Configure(IFunctionsHostBuilder builder)
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            _ = builder.Services.RegisterCommons(Name)
                                .AddSingleton<IFunctionFilter, ActionFilters>()
                                .RegisterAzureRepositories(JobsStorage, CosmosStorage)

                                .AddSampleModule();
        }
    }
}
