using Autofac;
using Autofac.Integration.Mef;
using RauchTech.Extensions.Logging;
using RauchTech.Extensions.Logging.Services.Code;

namespace Functions.Limits.Domain.Services.Code
{
    public class CustomLogsModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            base.Load(builder);
            builder.RegisterMetadataRegistrationSources();

            _ = builder
                .RegisterType<CustomLog>()
                .As<ICustomLog>()
                .AsImplementedInterfaces()
                .InstancePerLifetimeScope()
                .PropertiesAutowired();
        }
    }
}
