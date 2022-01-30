using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;

namespace RauchTech.Extensions.Common
{
    public static class Initializer
    {
        #region Constants
        private const string AppInsightsPropName = "{0}:AppInsights";

        private const string LogAnalyticsAuthId = "{0}:LogAnalyticsAuthId";

        private const string LogAnalyticsWorkspaceId = "{0}:LogAnalyticsWorkspaceId";
        #endregion

        public static IServiceCollection RegisterCommons(this IFunctionsHostBuilder builder, string configPrefix)
        {
            if (string.IsNullOrWhiteSpace(configPrefix))
            {
                throw new ArgumentNullException(nameof(configPrefix));
            }

            return builder.Services.AddScoped<ICustomLogFactory, CustomLogFactory>()
                            .ConfigureApplicationInsights(configPrefix)
                            .RegisterTelemetryClient(configPrefix, $"Functions{configPrefix}")
                            .ConfigureSerilog(configPrefix);
        }

        private static void LoadLogs(this IFunctionsHostBuilder builder)
        {
            services.AddScoped<ICustomLogFactory, CustomLogFactory>()
        }

        public static IServiceCollection RegisterAzureRepositories(this IServiceCollection services, string jobsConnection, string cosmosConnection)
        {
            if (string.IsNullOrWhiteSpace(jobsConnection))
            {
                throw new ArgumentNullException(nameof(jobsConnection));
            }

            if (string.IsNullOrWhiteSpace(cosmosConnection))
            {
                throw new ArgumentNullException(nameof(cosmosConnection));
            }

            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            string blobConnection = Environment.GetEnvironmentVariable(jobsConnection);
            string documentConnection = Environment.GetEnvironmentVariable(cosmosConnection);

            RepositoryDomain.RegisterBlob(blobConnection, services);
            RepositoryDomain.RegisterDocument(documentConnection, services);

            return services;
        }

        public static IServiceCollection RegisterTelemetryClient(this IServiceCollection services, string configPrefix, string roleName)
        {
            if (string.IsNullOrWhiteSpace(configPrefix))
            {
                throw new ArgumentNullException(nameof(configPrefix));
            }

            if (string.IsNullOrWhiteSpace(roleName))
            {
                throw new ArgumentNullException(nameof(roleName));
            }

            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            string instrumentationKey = Environment.GetEnvironmentVariable(configPrefix);

            if (!string.IsNullOrEmpty(instrumentationKey))
            {
                _ = services.AddScoped(_ =>
                {
                    TelemetryClient client = new TelemetryClient(TelemetryConfiguration.CreateDefault())
                    {
                        InstrumentationKey = instrumentationKey
                    };

                    client.Context.Cloud.RoleName = roleName;
                    return client;
                });
            }

            return services;
        }

        private static IServiceCollection ConfigureApplicationInsights(this IServiceCollection services, string configPrefix)
        {
            string instrumentationKey = Environment.GetEnvironmentVariable("ApplicationInsights:InstrumentationKey");

            if (!string.IsNullOrEmpty(instrumentationKey))
            {
                _ = services.AddScoped(_ =>
                {
                    TelemetryClient client = new TelemetryClient(TelemetryConfiguration.CreateDefault())
                    {
                        InstrumentationKey = instrumentationKey
                    };

                    client.Context.Cloud.RoleName = $"Functions{configPrefix}";
                    return client;
                });
            }

            return services;
        }

        private static IServiceCollection ConfigureSerilog(this IServiceCollection services, string configPrefix)
        {
            return services.AddLogging(builder =>
            {
                string authId;
                string workspaceId;
                string instrumentationKey;

                Logger logger;

                try
                {
                    authId = GetConfiguration(LogAnalyticsAuthId, configPrefix);
                    workspaceId = GetConfiguration(LogAnalyticsWorkspaceId, configPrefix);
                    instrumentationKey = GetConfiguration(AppInsightsPropName, configPrefix);

                    if (string.IsNullOrEmpty(instrumentationKey)
                        || string.IsNullOrEmpty(workspaceId)
                        || string.IsNullOrEmpty(authId))
                    {
                        throw new ArgumentNullException(nameof(configPrefix));
                    }

                    logger = new LoggerConfiguration()
                                   .MinimumLevel.Verbose()
                                   .Enrich.FromLogContext()
                                   .Enrich.WithExceptionDetails()
                                   .WriteTo.AzureAnalytics(workspaceId, authId, logName: $"Functions{configPrefix}")
                                   .CreateLogger();

                    _ = builder.AddSerilog(logger);

                    //_ = builder.AddApplicationInsights(instrumentationKey)
                    //           .AddSerilog(logger, dispose: true);
                }
                catch
                {
                    throw;
                }
            });
        }

        private static string GetConfiguration(string propName, string prefix)
        {
            string configKey = string.Format(CultureInfo.InvariantCulture, propName, prefix);
            return Environment.GetEnvironmentVariable(configKey);
        }
    }
}
}