using Autofac;
using Autofac.Integration.Mef;
using System;

namespace Functions.SampleCosmos.Domain.Services.Code.Builder
{
    public class SampleModule : Module
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
                .RegisterType<SampleService>()
                .As<ISampleService>()
                .AsImplementedInterfaces()
                .InstancePerLifetimeScope()
                .PropertiesAutowired();
        }
    }
}
