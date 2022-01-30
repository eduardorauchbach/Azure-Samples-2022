using Autofac.Extensions.DependencyInjection.AzureFunctions;
using Autofac;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;
using Functions.Limits.Domain.Services;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Serilog.Core;
using Serilog;
using Serilog.Exceptions;

namespace RauchTech.Extensions.Common
{
    public static class Initializer
    {
        #region Constants
        private const string AppInsightsPropName = "{0}:AppInsights";
        private const string LogAnalyticsAuthId = "{0}:LogAnalyticsAuthId";
        private const string LogAnalyticsWorkspaceId = "{0}:LogAnalyticsWorkspaceId";
        #endregion

        private static string? _functionPrefix;
        private static string? _authId;
        private static string? _workspaceId;
        private static string? _instrumentationKey;

        public static IServiceCollection RegisterCommons(this IFunctionsHostBuilder builder, string configPrefix)
        {
            if (string.IsNullOrWhiteSpace(configPrefix))
            {
                throw new ArgumentNullException(nameof(configPrefix));
            }

            _functionPrefix = $"Function{configPrefix}";

            _authId = GetConfiguration(LogAnalyticsAuthId, configPrefix);
            _workspaceId = GetConfiguration(LogAnalyticsWorkspaceId, configPrefix);
            _instrumentationKey = GetConfiguration(AppInsightsPropName, configPrefix);

            return builder.LoadLogs();
            //.RegisterTelemetryClient(configPrefix, $"Functions{configPrefix}")
        }

        #region Logging

        private static IServiceCollection LoadLogs(this IFunctionsHostBuilder builder)
        {
            if (_instrumentationKey != null && _workspaceId != null && _authId != null)
            {
                builder.Services
                        .ConfigureApplicationInsights()
                        .ConfigureSerilog();

                _ = builder.UseAutofacServiceProviderFactory(LoadLogsModule);
            }

            return builder.Services;
        }

        private static IServiceCollection ConfigureApplicationInsights(this IServiceCollection services)
        {
            if (_instrumentationKey != null)
            {
                _ = services.AddScoped(_ =>
                {
                    TelemetryClient client = new TelemetryClient(TelemetryConfiguration.CreateDefault())
                    {
                        InstrumentationKey = _instrumentationKey
                    };

                    client.Context.Cloud.RoleName = _functionPrefix;
                    return client;
                });
            }

            return services;
        }

        private static IServiceCollection ConfigureSerilog(this IServiceCollection services)
        {
            return services.AddLogging(builder =>
            {
                Logger logger;

                try
                {
                    logger = new LoggerConfiguration()
                                   .MinimumLevel.Verbose()
                                   .Enrich.FromLogContext()
                                   .Enrich.WithExceptionDetails()
                                   .WriteTo.AzureAnalytics(_workspaceId, _authId, logName: _functionPrefix)
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

        private static void LoadLogsModule(ContainerBuilder builder)
        {
            _ = builder.RegisterModule<CustomLogModule>();
        }

        #endregion

        //public static IServiceCollection RegisterAzureRepositories(this IServiceCollection services, string jobsConnection, string cosmosConnection)
        //{
        //    if (string.IsNullOrWhiteSpace(jobsConnection))
        //    {
        //        throw new ArgumentNullException(nameof(jobsConnection));
        //    }

        //    if (string.IsNullOrWhiteSpace(cosmosConnection))
        //    {
        //        throw new ArgumentNullException(nameof(cosmosConnection));
        //    }

        //    if (services is null)
        //    {
        //        throw new ArgumentNullException(nameof(services));
        //    }

        //    string blobConnection = Environment.GetEnvironmentVariable(jobsConnection);
        //    string documentConnection = Environment.GetEnvironmentVariable(cosmosConnection);

        //    RepositoryDomain.RegisterBlob(blobConnection, services);
        //    RepositoryDomain.RegisterDocument(documentConnection, services);

        //    return services;
        //}

        //public static IServiceCollection RegisterTelemetryClient(this IServiceCollection services, string configPrefix, string roleName)
        //{
        //    if (string.IsNullOrWhiteSpace(configPrefix))
        //    {
        //        throw new ArgumentNullException(nameof(configPrefix));
        //    }

        //    if (string.IsNullOrWhiteSpace(roleName))
        //    {
        //        throw new ArgumentNullException(nameof(roleName));
        //    }

        //    if (services is null)
        //    {
        //        throw new ArgumentNullException(nameof(services));
        //    }

        //    string instrumentationKey = Environment.GetEnvironmentVariable(configPrefix);

        //    if (!string.IsNullOrEmpty(instrumentationKey))
        //    {
        //        _ = services.AddScoped(_ =>
        //        {
        //            TelemetryClient client = new TelemetryClient(TelemetryConfiguration.CreateDefault())
        //            {
        //                InstrumentationKey = instrumentationKey
        //            };

        //            client.Context.Cloud.RoleName = roleName;
        //            return client;
        //        });
        //    }

        //    return services;
        //}

        private static string? GetConfiguration(string propName, string prefix)
        {
            string configKey = string.Format(CultureInfo.InvariantCulture, propName, prefix);
            return Environment.GetEnvironmentVariable(configKey);
        }
    }
}