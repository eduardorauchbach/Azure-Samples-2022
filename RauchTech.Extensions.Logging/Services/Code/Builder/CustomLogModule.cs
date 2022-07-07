using Microsoft.Extensions.DependencyInjection;
using RauchTech.Extensions.Logging;
using RauchTech.Extensions.Logging.Services.Code;

namespace Functions.Limits.Domain.Services.Code.Builder
{
    public static class CustomLogModule
    {
        public static IServiceCollection AddCustomLogModule(this IServiceCollection services)
        {
            services.AddScoped<ICustomLog, CustomLog>();

            return services;
        }
    }
}
