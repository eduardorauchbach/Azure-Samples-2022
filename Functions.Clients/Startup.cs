using Functions.Common;
using Infra.Data.Repositories.CosmosDb;
using Infra.Data.Repositories.Interfaces;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using Autofac.Extensions.DependencyInjection.AzureFunctions;
using Autofac;
using Functions.Limits.Domain.Services;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.OpenApi.Models;
using AzureFunctions.Extensions.Swashbuckle;
using Microsoft.OpenApi;
using AzureFunctions.Extensions.Swashbuckle.Settings;
using Swashbuckle.AspNetCore.SwaggerGen;
using Functions.Limits.Domain.Functions.Helper;
using System.Diagnostics;
using Functions.Limits.Domain.Services.Limits;

[assembly: CLSCompliant(true)]
[assembly: FunctionsStartup(typeof(Functions.Limits.Startup))]

namespace Functions.Limits
{
    public sealed class Startup : FunctionsStartup
    {
        #region Constants

        private const string Name = "Limits";

        private const string CosmosStorage = "CNH_CosmosDB";
        private const string JobsStorage = "AzureWebJobsStorage";

        //private const string ProductionEnvironment = "Prod";

        private const string SwaggerTitle = "Limits";
        private const string SwaggerDescription = "Swagger";
        private const string SwaggerVersion = "v1.0";

        #endregion

        public override void Configure(IFunctionsHostBuilder builder)
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            _ = builder.Services.RegisterCommons(Name)
                                .AddSingleton<IHttpContextAccessor, HttpContextAccessor>()
                                .AddSingleton<IFunctionFilter, ActionFilters>()
                                .RegisterAzureRepositories(JobsStorage, CosmosStorage);

            DebugSwagger(builder);

            _ = builder.UseAutofacServiceProviderFactory(ConfigureContainer);
        }

        [Conditional("DEBUG")]
        private void DebugSwagger(IFunctionsHostBuilder builder)
        {
            _ = builder.AddSwashBuckle(Assembly.GetExecutingAssembly(), opts =>
            {
                opts.Title = SwaggerTitle;
                opts.SpecVersion = OpenApiSpecVersion.OpenApi2_0;
                opts.AddCodeParameter = true;
                opts.PrependOperationWithRoutePrefix = true;
                opts.Documents = new[]
                {
                    new SwaggerDocument
                    {
                        Title = SwaggerTitle,
                        Description = SwaggerDescription,
                        Version = SwaggerVersion
                    }
                };
                opts.ConfigureSwaggerGen = (x =>
                {
                    x.OrderActionsBy((o) => { return o.GroupName + o.RelativePath; });
                    x.CustomOperationIds(apiDesc =>
                    {
                        return apiDesc.TryGetMethodInfo(out MethodInfo methodInfo)
                            ? methodInfo.Name
                            : new Guid().ToString();
                    });
                });
            });

            //Todo: quando possível incluir Newtonsoft para que o Swagger funcione direito (ainda não suportado para functions)
            //_ = builder.Services.AddSwaggerGenNewtonsoftSupport();
        }

        private static void ConfigureContainer(ContainerBuilder builder)
        {
            _ = builder.RegisterModule<LimitsModule>();

            _ = builder.RegisterAssemblyTypes(typeof(Startup).Assembly)
                       .InNamespace("Functions.Limits")
                       .AsSelf()
                       .InstancePerTriggerRequest();
        }
    }
}
