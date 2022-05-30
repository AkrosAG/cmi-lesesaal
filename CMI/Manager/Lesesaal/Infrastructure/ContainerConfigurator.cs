using System.Reflection;
using Autofac;
using CMI.Access.Sql.Lesesaal.EF;
using CMI.Contract.Common;
using CMI.Contract.Messaging;
using CMI.Contract.Monitoring;
using CMI.Contract.Parameter;
using CMI.Manager.Lesesaal.Properties;
using CMI.Utilities.Bus.Configuration;
using MassTransit;

namespace CMI.Manager.Lesesaal.Infrastructure
{
    /// <summary>
    ///     Helper class for configuring the IoC container.
    /// </summary>
    internal class ContainerConfigurator
    {
        public static ContainerBuilder Configure()
        {
            var builder = new ContainerBuilder();
            RegisterBus(builder);

            var connectionString = DbConnectionSetting.Default.ConnectionStringEF;
            builder.RegisterType<LesesaalDb>().AsSelf().WithParameter(nameof(connectionString), connectionString); ;
            builder.RegisterType<ParameterHelper>().As<IParameterHelper>();
            builder.RegisterType<CollectionAccess>().As<ICollectionAccess>();
            builder.RegisterType<CollectionManager>().As<ICollectionManager>();

            // SimpleConsumers
            builder.RegisterType(typeof(SimpleConsumer<GetAllCollectionsRequest, GetAllCollectionsResponse, ICollectionManager>)).As(typeof(IConsumer<GetAllCollectionsRequest>));
            builder.RegisterType(typeof(SimpleConsumer<GetActiveCollectionsRequest, GetActiveCollectionsResponse, ICollectionManager>)).As(typeof(IConsumer<GetActiveCollectionsRequest>));
            builder.RegisterType(typeof(SimpleConsumer<GetCollectionsHeaderRequest, GetCollectionsHeaderResponse, ICollectionManager>)).As(typeof(IConsumer<GetCollectionsHeaderRequest>));
            builder.RegisterType(typeof(SimpleConsumer<GetCollectionRequest, GetCollectionResponse, ICollectionManager>)).As(typeof(IConsumer<GetCollectionRequest>));
            builder.RegisterType(typeof(SimpleConsumer<InsertOrUpdateCollectionRequest, InsertOrUpdateCollectionResponse, ICollectionManager>)).As(typeof(IConsumer<InsertOrUpdateCollectionRequest>));
            builder.RegisterType(typeof(SimpleConsumer<DeleteCollectionRequest, DeleteCollectionResponse, ICollectionManager>)).As(typeof(IConsumer<DeleteCollectionRequest>));
            builder.RegisterType(typeof(SimpleConsumer<BatchDeleteCollectionRequest, BatchDeleteCollectionResponse, ICollectionManager>)).As(typeof(IConsumer<BatchDeleteCollectionRequest>));
            builder.RegisterType(typeof(SimpleConsumer<GetPossibleParentsRequest, GetPossibleParentsResponse, ICollectionManager>)).As(typeof(IConsumer<GetPossibleParentsRequest>));
            builder.RegisterType(typeof(SimpleConsumer<GetImageRequest, GetImageResponse, ICollectionManager>)).As(typeof(IConsumer<GetImageRequest>));
            builder.RegisterType(typeof(SimpleConsumer<GetCollectionItemResultRequest, GetCollectionItemResultResponse, ICollectionManager>)).As(typeof(IConsumer<GetCollectionItemResultRequest>));


            builder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
                .AssignableTo<IConsumer>()
                .AsSelf();

            return builder;
        }


        private static void RegisterBus(ContainerBuilder builder)
        {
            var helper = new ParameterBusHelper();
            BusConfigurator.ConfigureBus(builder, MonitoredServices.LesesaalService,
                (cfg, ctx) => { helper.SubscribeAllSettingsInAssembly(Assembly.GetExecutingAssembly(), cfg); });
        }
    }
}