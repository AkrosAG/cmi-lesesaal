using System.Reflection;
using Autofac;
using CMI.Contract.Messaging;
using CMI.Contract.Monitoring;
using CMI.Contract.Parameter;
using CMI.Manager.Lesesaal.Infrastructure;
using CMI.Utilities.Bus.Configuration;
using CMI.Utilities.Logging.Configurator;
using MassTransit;
using Serilog;

namespace CMI.Manager.Lesesaal
{
    public class LesesaalService
    {
        private IBusControl bus;

        public void Start()
        {
            LogConfigurator.ConfigureForService();

            Log.Information("Lesesaal service is starting");

            var containerBuilder = ContainerConfigurator.Configure();

            BusConfigurator.ConfigureBus(containerBuilder, MonitoredServices.LesesaalService, (cfg, ctx) =>
            {
                cfg.ReceiveEndpoint(BusConstants.ReadUserInformationQueue,
                    ec => { ec.Consumer(ctx.Resolve<ReadUserInformationConsumer>); }
                );
                cfg.ReceiveEndpoint(BusConstants.ReadStammdatenQueue,
                    ec => { ec.Consumer(ctx.Resolve<ReadStammdatenConsumer>); }
                );
                // CollectionManager Methods
                cfg.ReceiveEndpoint(string.Format(BusConstants.LesesaalManagerRequestBase, nameof(GetAllCollectionsRequest)),
                    ec => { ec.Consumer(ctx.Resolve<IConsumer<GetAllCollectionsRequest>>); });
                cfg.ReceiveEndpoint(string.Format(BusConstants.LesesaalManagerRequestBase, nameof(GetActiveCollectionsRequest)),
                    ec => { ec.Consumer(ctx.Resolve<IConsumer<GetActiveCollectionsRequest>>); });
                cfg.ReceiveEndpoint(string.Format(BusConstants.LesesaalManagerRequestBase, nameof(GetCollectionsHeaderRequest)),
                    ec => { ec.Consumer(ctx.Resolve<IConsumer<GetCollectionsHeaderRequest>>); });
                cfg.ReceiveEndpoint(string.Format(BusConstants.LesesaalManagerRequestBase, nameof(GetCollectionRequest)),
                    ec => { ec.Consumer(ctx.Resolve<IConsumer<GetCollectionRequest>>); });
                cfg.ReceiveEndpoint(string.Format(BusConstants.LesesaalManagerRequestBase, nameof(InsertOrUpdateCollectionRequest)),
                    ec => { ec.Consumer(ctx.Resolve<IConsumer<InsertOrUpdateCollectionRequest>>); });
                cfg.ReceiveEndpoint(string.Format(BusConstants.LesesaalManagerRequestBase, nameof(DeleteCollectionRequest)),
                    ec => { ec.Consumer(ctx.Resolve<IConsumer<DeleteCollectionRequest>>); });
                cfg.ReceiveEndpoint(string.Format(BusConstants.LesesaalManagerRequestBase, nameof(BatchDeleteCollectionRequest)),
                    ec => { ec.Consumer(ctx.Resolve<IConsumer<BatchDeleteCollectionRequest>>); });
                cfg.ReceiveEndpoint(string.Format(BusConstants.LesesaalManagerRequestBase, nameof(GetPossibleParentsRequest)),
                    ec => { ec.Consumer(ctx.Resolve<IConsumer<GetPossibleParentsRequest>>); });
                cfg.ReceiveEndpoint(string.Format(BusConstants.LesesaalManagerRequestBase, nameof(GetImageRequest)),
                    ec => { ec.Consumer(ctx.Resolve<IConsumer<GetImageRequest>>); });
                cfg.ReceiveEndpoint(string.Format(BusConstants.LesesaalManagerRequestBase, nameof(GetCollectionItemResultRequest)),
                    ec => { ec.Consumer(ctx.Resolve<IConsumer<GetCollectionItemResultRequest>>); });

                var helper = new ParameterBusHelper();
                helper.SubscribeAllSettingsInAssembly(Assembly.GetExecutingAssembly(), cfg);
            });

            var container = containerBuilder.Build();
            bus = container.Resolve<IBusControl>();
            bus.Start();

            Log.Information("Lesesaal service started");
        }

        public void Stop()
        {
            Log.Information("Lesesaal service is stopping.");
            bus.Stop();
            Log.Information("Lesesaal service has stopped.");
            Log.CloseAndFlush();
        }
    }
}