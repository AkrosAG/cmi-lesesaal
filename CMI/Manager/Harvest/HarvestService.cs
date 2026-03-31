using CMI.Contract.Messaging;
using CMI.Contract.Monitoring;
using CMI.Contract.Parameter;
using CMI.Manager.Harvest.Consumers;
using CMI.Manager.Harvest.Infrastructure;
using CMI.Utilities.Bus.Configuration;
using CMI.Utilities.Logging.Configurator;
using GreenPipes;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Reflection;

namespace CMI.Manager.Harvest
{
    /// <summary>
    ///     The HarvestService is configuring the bus and IoC container.
    /// </summary>
    public class HarvestService
    {
        private readonly IServiceCollection services;
        private IBusControl bus;

        public HarvestService()
        {
            // Configure IoC Container
            services = ContainerConfigurator.Configure();
            LogConfigurator.ConfigureForService();
        }

        /// <summary>
        ///     Starts the Harvest Service.
        ///     Called by the service host when the service is started.
        /// </summary>
        public void Start()
        {
            Log.Information("Harvest service is starting");

            // Configure Bus
            BusConfigurator.ConfigureBusModern(services, MonitoredServices.HarvestService, AddConsumersAndRequestClients, (context, cfg) =>
            {
                cfg.ReceiveEndpoint(BusConstants.HarvestManagerSyncArchiveRecordMessageQueue,
                    ec =>
                    {
                        ec.ConfigureConsumer<SyncArchiveRecordConsumer>(context);

                        ec.UseRetry(retryPolicy =>
                            retryPolicy.Exponential(10, TimeSpan.FromSeconds(1), TimeSpan.FromMinutes(5), TimeSpan.FromSeconds(5)));
                        BusConfigurator.SetPrefetchCountForEndpoint(ec);
                    });

                cfg.ReceiveEndpoint(BusConstants.HarvestManagerArchiveRecordUpdatedEventQueue, ec =>
                {
                    ec.ConfigureConsumer<ArchiveRecordUpdatedConsumer>(context);
                    ec.UseRetry(retryPolicy =>
                        retryPolicy.Exponential(10, TimeSpan.FromSeconds(1), TimeSpan.FromMinutes(5), TimeSpan.FromSeconds(5)));
                });
                cfg.ReceiveEndpoint(BusConstants.HarvestManagerArchiveRecordRemovedEventQueue, ec =>
                {
                    ec.ConfigureConsumer<ArchiveRecordRemovedConsumer>(context);
                    ec.UseRetry(retryPolicy =>
                        retryPolicy.Exponential(10, TimeSpan.FromSeconds(1), TimeSpan.FromMinutes(5), TimeSpan.FromSeconds(5)));
                });
                cfg.ReceiveEndpoint(BusConstants.HarvestManagerResyncArchiveDatabaseMessageQueue, ec =>
                {
                    ec.ConfigureConsumer<ArchiveDatabaseResyncConsumer>(context);
                    ec.UseRetry(retryPolicy =>
                        retryPolicy.Exponential(10, TimeSpan.FromSeconds(1), TimeSpan.FromMinutes(5), TimeSpan.FromSeconds(5)));
                });

                cfg.ReceiveEndpoint(BusConstants.MonitoringAisDbCheckQueue, ec =>
                {
                    ec.ConfigureConsumer<CheckAisDbConsumer>(context);
                });
                cfg.ReceiveEndpoint(BusConstants.ManagementApiGetHarvestLogInfoRequestQueue, ec =>
                {
                    ec.ConfigureConsumer<HarvestLogInfoConsumer>(context);
                    ec.UseRetry(retryPolicy =>
                        retryPolicy.Exponential(10, TimeSpan.FromSeconds(1), TimeSpan.FromMinutes(5), TimeSpan.FromSeconds(5)));
                });

                // Wire up the parameter manager
                var helper = new ParameterBusHelper();
                helper.SubscribeAllSettingsInAssembly(Assembly.GetExecutingAssembly(), cfg);
            });

            var provider = services.BuildServiceProvider();
            bus = provider.GetRequiredService<IBusControl>();
            bus.Start();

            Log.Information("Harvest service started");
        }

        private void AddConsumersAndRequestClients(IBusRegistrationConfigurator x)
        {
            // registers all IConsumer implementations in this assembly
            x.AddConsumers(Assembly.GetExecutingAssembly());

            // RequestClients registrieren
            x.AddRequestClient<FindArchiveRecordRequest>(
                new Uri(new Uri(BusConfigurator.Uri), BusConstants.IndexManagerFindArchiveRecordMessageQueue),
                TimeSpan.FromMinutes(1));

            x.AddRequestClient<ConvertArchiveRecordRequest>(
                new Uri(new Uri(BusConfigurator.Uri), BusConstants.IndexManagerConvertArchiveRecordMessageQueue),
                TimeSpan.FromMinutes(1));
        }


        /// <summary>
        ///     Stops the Harvest Service.
        ///     Called by the service host when the service is stopped.
        /// </summary>
        public void Stop()
        {
            Log.Information("Harvest service is stopping");
            bus.Stop();
            Log.Information("Harvest service stopped");
            Log.CloseAndFlush();
        }
    }
}