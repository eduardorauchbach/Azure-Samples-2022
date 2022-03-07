using Functions.Common;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.DependencyInjection;
using System;
using Autofac.Extensions.DependencyInjection.AzureFunctions;
using Autofac;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.OpenApi.Models;
using AzureFunctions.Extensions.Swashbuckle;
using Microsoft.OpenApi;
using AzureFunctions.Extensions.Swashbuckle.Settings;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Diagnostics;
using Functions.Clients.Domain.Functions.Helper;
using Functions.Clients.Domain.Repositories;

[assembly: CLSCompliant(true)]
[assembly: FunctionsStartup(typeof(Functions.Limits.Startup))]

namespace Functions.Limits
{
    public sealed class Startup : FunctionsStartup
    {
        #region Constants

        private const string Name = "Clients";

        private const string CosmosStorage = "CosmosStorage";
        private const string JobsStorage = "BlobStorage";

        private const string SwaggerTitle = "Clients";
        private const string SwaggerDescription = "Swagger";
        private const string SwaggerVersion = "v1.0";

        #endregion

        public override void Configure(IFunctionsHostBuilder builder)
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            _ = builder.RegisterCommons(Name)
                       .RegisterSwagger(SwaggerTitle, SwaggerDescription, SwaggerVersion)
                       .RegisterAzureRepositories(JobsStorage, CosmosStorage);

            _ = builder.Services.AddSingleton<IFunctionFilter, ActionFilters>();
            _ = builder.UseAutofacServiceProviderFactory(ConfigureContainer);
        }

        private static void ConfigureContainer(ContainerBuilder builder)
        {
            //_ = builder.RegisterModule<LimitsModule>();

            _ = builder.RegisterAssemblyTypes(typeof(Startup).Assembly)
                       .InNamespace($"Functions{Name}")
                       .AsSelf()
                       .InstancePerTriggerRequest();
        }
    }
}
