
using System;
using System.Reflection;
using Autofac;
using CMI.Access.Repository.Systems.Rosetta;
using CMI.Contract.Messaging;
using CMI.Contract.Monitoring;
using CMI.Contract.Repository;
using CMI.Engine.PackageMetadata.Systems.Rosetta;
using CMI.Manager.Repository.Systems;
using CMI.Manager.Repository.Systems.Rosetta;
using CMI.Utilities.Bus.Configuration;
using MassTransit;

namespace CMI.Utilities.DigitalRepository.PrimaryDataHarvester
{
    internal class ContainerConfigurator
    {
        public static ContainerBuilder Configure()
        {
            var builder = new ContainerBuilder();
            BusConfigurator.ConfigureBus(builder, MonitoredServices.NotMonitored, (cfg, ctx) =>
            {
                cfg.ReceiveEndpoint( MonitoredServices.NotMonitored.ToString(),
                    ec => { ec.Consumer(() => new HeartbeatConsumer(MonitoredServices.NotMonitored.ToString())); });
            });

            builder.RegisterType<RosettaRepositoryProvider>().As<IRepositoryProvider>();
            builder.RegisterType<RosettaPackageHandler>().As<IPackageHandler>();
            builder.RegisterType<RosettaDataAccess>().As<IRosettaDataAccess>();
            builder.RegisterType<RosettaRepositoryCheck>().As<IRepositoryCheck>();


            // register all the consumers
            builder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
                .AssignableTo<IConsumer>()
                .AsSelf();


            builder.Register(CreateFindArchiveRecordRequestClient);
            builder.Register(GetArchiveRecordsForPackageRequestClientCallback);
            builder.RegisterType<PrimaryDataHarvester>().AsSelf();

            return builder;
        }

        private static IRequestClient<FindArchiveRecordRequest> CreateFindArchiveRecordRequestClient(IComponentContext context)
        {
            var requestTimeout = TimeSpan.FromMinutes(10);
            var bus = context.Resolve<IBusControl>();
            return bus.CreateRequestClient<FindArchiveRecordRequest>(new Uri(new Uri(BusConfigurator.Uri), BusConstants.IndexManagerFindArchiveRecordMessageQueue), requestTimeout);
        }

        private static IRequestClient<GetArchiveRecordsForPackageRequest>
            GetArchiveRecordsForPackageRequestClientCallback(IComponentContext context)
        {
            var serviceUrl = string.Format(BusConstants.IndexManagagerRequestBase, nameof(GetArchiveRecordsForPackageRequest));
            var requestTimeout = TimeSpan.FromMinutes(10);
            var bus = context.Resolve<IBusControl>();

            return bus.CreateRequestClient<GetArchiveRecordsForPackageRequest>(new Uri(bus.Address, serviceUrl), requestTimeout);
        }
    }
}
