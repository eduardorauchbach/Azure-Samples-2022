using Microsoft.Extensions.DependencyInjection;

namespace Functions.SampleCosmos.Domain.Services.Code.Builder
{
    public static class SampleModule
    {
        public static IServiceCollection AddSampleModule(this IServiceCollection services)
        {
            services.AddScoped<ISampleService, SampleService>();

            return services;
        }
    }
}
