using Autofac;
using Autofac.Integration.Mef;
using Functions.Limits.Domain.Services.Limit;
using System;

namespace Functions.Limits.Domain.Services.Limits
{
    public class LimitsModule : Module
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
                .As<ILimitsService>()
                .AsImplementedInterfaces()
                .InstancePerLifetimeScope()
                .PropertiesAutowired();

            _ = builder
                .RegisterType<BeneficiariesService>()
                .As<IBeneficiariesService>()
                .AsImplementedInterfaces()
                .InstancePerLifetimeScope()
                .PropertiesAutowired();

            _ = builder
                .RegisterType<CommentsService>()
                .As<ICommentsService>()
                .AsImplementedInterfaces()
                .InstancePerLifetimeScope()
                .PropertiesAutowired();

            _ = builder
                .RegisterType<DocumentsService>()
                .As<IDocumentsService>()
                .AsImplementedInterfaces()
                .InstancePerLifetimeScope()
                .PropertiesAutowired();
        }
    }
}
