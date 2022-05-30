using System.Reflection;
using Autofac;
using CMI.Access.Harvest;
using CMI.Access.Harvest.ScopeArchiv;
using CMI.Contract.Harvest;
using MassTransit;

namespace CMI.Manager.DataFeed.Infrastructure
{
    internal class ContainerConfigurator
    {
        public static ContainerBuilder Configure()
        {
            var builder = new ContainerBuilder();

            // register the different consumers and classes
            builder.RegisterType<LanguageSettings>().AsSelf();
            builder.RegisterType<ApplicationSettings>().AsSelf();
            builder.RegisterType<CachedLookupData>().AsSelf();
            builder.RegisterType<SipDateBuilder>().AsSelf();
            builder.RegisterType<DigitizationOrderBuilder>().AsSelf();
            
            builder.RegisterType<AISDataAccess>().As<IDbMutationQueueAccess>();
            builder.RegisterType<CheckMutationQueueJob>().AsSelf();
            builder.RegisterType<RequeueMutationJob>().AsSelf();

            builder.RegisterType<AISDataProviderFactory>().As<IAISDataProviderFactory>();
            builder.RegisterType<ArchiveRecordBuilderFactory>().As<IArchiveRecordBuilderFactory>();

            builder.Register(ctx =>
            {
                var dataProviderFactory = ctx.Resolve<IAISDataProviderFactory>();
                return dataProviderFactory.Create();
            }).As<IAISDataProvider>().AsSelf();

            builder.Register(ctx =>
            {
                var builderFactory = ctx.Resolve<IArchiveRecordBuilderFactory>();
                return builderFactory.Create();
            }).As<IArchiveRecordBuilder>().AsSelf();

            builder.RegisterType<JobCancelToken>().As<ICancelToken>().SingleInstance().ExternallyOwned();

            builder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
                .AssignableTo<IConsumer>()
                .AsSelf();

            return builder;
        }
    }
}