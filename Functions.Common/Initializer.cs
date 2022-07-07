using Functions.Limits.Domain.Services.Code.Builder;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Core;
using Serilog.Exceptions;
using System.Globalization;

namespace Functions.Common
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

        public static IServiceCollection RegisterCommons(this IServiceCollection services, string configPrefix)
        {
            if (string.IsNullOrWhiteSpace(configPrefix))
            {
                throw new ArgumentNullException(nameof(configPrefix));
            }

            _functionPrefix = $"Function{configPrefix}";

            _authId = GetConfiguration(LogAnalyticsAuthId, configPrefix);
            _workspaceId = GetConfiguration(LogAnalyticsWorkspaceId, configPrefix);
            _instrumentationKey = GetConfiguration(AppInsightsPropName, configPrefix);

            services.RegisterUtilities();
            services.RegisterLogs();

            return services;
        }

        #region Utilities

        private static IServiceCollection RegisterUtilities(this IServiceCollection services)
        {
            _ = services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            return services;
        }

        #endregion

        #region Logging

        private static IServiceCollection RegisterLogs(this IServiceCollection services)
        {
            if (_instrumentationKey != null && _workspaceId != null && _authId != null)
            {
                services.ConfigureApplicationInsights()
                        .ConfigureSerilog();

                _ = services.AddCustomLogModule();
            }

            return services;
        }

        private static IServiceCollection ConfigureApplicationInsights(this IServiceCollection services)
        {
            if (_instrumentationKey != null)
            {
                _ = services.AddScoped(_ =>
                {
                    TelemetryClient client = new(TelemetryConfiguration.CreateDefault())
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

        #endregion

        private static string? GetConfiguration(string propName, string prefix)
        {
            string configKey = string.Format(CultureInfo.InvariantCulture, propName, prefix);
            return Environment.GetEnvironmentVariable(configKey);
        }
    }
}