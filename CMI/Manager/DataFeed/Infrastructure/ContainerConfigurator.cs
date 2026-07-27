using Autofac;
using CMI.Manager.DataFeed.SyncLog;
using MassTransit;
using System.Reflection;

namespace CMI.Manager.DataFeed.Infrastructure
{
    internal class ContainerConfigurator
    {
        public static ContainerBuilder Configure()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<DbSyncLogAccess>().As<IDbSyncLogAccess>();
            builder.RegisterType<CheckMutationQueueJob>().AsSelf();
            builder.RegisterType<RequeueMutationJob>().AsSelf();
            builder.RegisterType<DeleteOldSyncActionItemsJob>().AsSelf();

            builder.RegisterType<JobCancelToken>().As<ICancelToken>().SingleInstance().ExternallyOwned();

            builder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
                .AssignableTo<IConsumer>()
                .AsSelf();

            return builder;
        }
    }
}