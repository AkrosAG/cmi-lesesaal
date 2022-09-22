using System.Configuration;
using System.Reflection;
using System.Runtime.Caching;
using Autofac;
using CMI.Access.Harvest;
using CMI.Access.Harvest.CMIAIS;
using CMI.Access.Harvest.ScopeArchiv;
using CMI.Access.Sql.Lesesaal.EF;
using CMI.Contract.Common.Compiler;
using CMI.Contract.Harvest;
using CMI.Manager.DataFeed.Properties;
using MassTransit;
using Microsoft.CSharp;

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
            builder.RegisterInstance<MemoryCache>(MemoryCache.Default).SingleInstance();

            builder.RegisterType<CMIAISArchiveRecordProcessHandler>().As<IArchiveRecordProcessHandler>();
            builder.RegisterType<AISDataAccess>().As<IDbMutationQueueAccess>();
            builder.RegisterType<CheckMutationQueueJob>().AsSelf();
            builder.RegisterType<RequeueMutationJob>().AsSelf();
            var connectionString = ConfigurationManager.ConnectionStrings[nameof(LesesaalDb)].ConnectionString;
            builder.RegisterType<LesesaalDb>().AsSelf().WithParameter(nameof(connectionString), connectionString);
            builder.RegisterType<AISDataProviderFactory>().As<IAISDataProviderFactory>();
            builder.RegisterType<ArchiveRecordBuilderFactory>().As<IArchiveRecordBuilderFactory>();
            builder.RegisterType<CSharpCodeProvider>().AsSelf().SingleInstance();
            builder.RegisterType<DynamicScriptProvider>().As<IDynamicScriptProvider>();
            builder.Register(ctx =>
            {
                return new EmptyScriptLocator();
            })
            .AsImplementedInterfaces()
            .SingleInstance();

            builder.Register(ctx =>
            {
                var dataProviderFactory = ctx.Resolve<IAISDataProviderFactory>();
                return dataProviderFactory.Create();
            }).AsImplementedInterfaces().AsSelf();

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