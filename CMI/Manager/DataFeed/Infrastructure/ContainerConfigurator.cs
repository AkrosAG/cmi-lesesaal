using Autofac;
using CMI.Access.Harvest;
using CMI.Access.Harvest.CMIAIS;
using CMI.Access.Sql.Lesesaal.EF;
using CMI.Contract.Harvest;
using CMI.Manager.DataFeed.Properties;
using CMI.Manager.DataFeed.SyncLog;
using MassTransit;
using System.Reflection;
using System.Runtime.Caching;

namespace CMI.Manager.DataFeed.Infrastructure
{
    internal class ContainerConfigurator
    {
        public static ContainerBuilder Configure()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<CMIAISDataAccess>().As<IDbMutationQueueAccess>();
            builder.RegisterType<DbSyncLogAccess>().As<IDbSyncLogAccess>();
            builder.RegisterType<CheckMutationQueueJob>().AsSelf();
            builder.RegisterType<RequeueMutationJob>().AsSelf();
            builder.RegisterType<DeleteOldSyncActionItemsJob>().AsSelf();
            builder.RegisterType<DataFeedManager>().As<IDataFeedManager>();

            builder.RegisterInstance(MemoryCache.Default)
                .As<MemoryCache>()
                .SingleInstance();
            var connectionString = Settings.Default.ConnectionStringEF;
            
            builder.RegisterType<LesesaalDb>().AsSelf().WithParameter(nameof(connectionString), connectionString);

            builder.RegisterType<CMIAISDataProvider>().As<IAISDataProvider>();

            builder.RegisterType<JobCancelToken>().As<ICancelToken>().SingleInstance().ExternallyOwned();

            builder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
                .AssignableTo<IConsumer>()
                .AsSelf();

            return builder;
        }
    }
}