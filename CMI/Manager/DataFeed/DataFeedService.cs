using CMI.Contract.Monitoring;
using CMI.Manager.DataFeed.Infrastructure;
using CMI.Utilities.Bus.Configuration;
using CMI.Utilities.Logging.Configurator;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Serilog;
using System.Reflection;
using System.Threading.Tasks;

namespace CMI.Manager.DataFeed
{
    public class DataFeedService
    {
        private readonly IServiceCollection services;
        private IBusControl bus;
        private IScheduler scheduler;
        private ServiceProvider provider;

        /// <summary>
        ///     The data feed service uses a timer to poll the mutation queue for any pending changes.
        ///     Pending changes are then put on the bus for further processing.
        /// </summary>
        public DataFeedService()
        {
            // Configure IoC Container
            services = ContainerConfigurator.Configure();
            LogConfigurator.ConfigureForService();
        }

        /// <summary>
        ///     Starts the DataFeed Service.
        ///     Called by the service host when the service is started.
        /// </summary>
        public async Task Start()
        {
            Log.Information("DataFeed service is starting");

            // Configure Bus

            BusConfigurator.ConfigureBusModern(services, MonitoredServices.DataFeedService, AddConsumers, (context, cfg) => { });

            Log.Verbose("Starting scheduler");
            // Start the timer
            provider = services.BuildServiceProvider();
            scheduler = await SchedulerConfigurator.Configure(provider);
            await scheduler.Start();
            bus = provider.GetRequiredService<IBusControl>();
            Log.Information("DataFeed service started");
            bus.Start();
        }

        /// <summary>
        ///     Stops the DataFeed Service.
        ///     Called by the service host when the service is stopped.
        /// </summary>
        public void Stop()
        {
            Log.Information("DataFeed service is stopping");

            // Get the singleton JobCancelToken and cancel any running job
            var token = provider.GetRequiredService<ICancelToken>();

            // Job abbrechen
            token.Cancel();

            // Stop the scheduler and wait until any running jobs have completed
            scheduler.Shutdown(true);

            bus.Stop();

            Log.Information("DataFeed service stopped");
            Log.CloseAndFlush();
        }

        private void AddConsumers(IBusRegistrationConfigurator x)
        {
            // registers all IConsumer implementations in this assembly
            x.AddConsumers(Assembly.GetExecutingAssembly());
        }
    }
}