using CMI.Access.Sql.Lesesaal.EF;
using CMI.Contract.Common;
using CMI.Contract.Messaging;
using CMI.Contract.Monitoring;
using CMI.Contract.Parameter;
using CMI.Manager.Lesesaal.Consumer;
using CMI.Manager.Lesesaal.Infrastructure;
using CMI.Utilities.Bus.Configuration;
using CMI.Utilities.Logging.Configurator;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System.Reflection;

namespace CMI.Manager.Lesesaal
{
    public class LesesaalService
    {
        private IBusControl bus;

        public void Start()
        {
            LogConfigurator.ConfigureForService();

            Log.Information("Lesesaal service is starting");

            var services = ContainerConfigurator.Configure();

            BusConfigurator.ConfigureBusModern(services, MonitoredServices.LesesaalService, AddConsumers, (context, cfg) =>
            {
                cfg.ReceiveEndpoint(BusConstants.ReadUserInformationQueue,
                    e => { e.ConfigureConsumer<ReadUserInformationConsumer>(context); });
                cfg.ReceiveEndpoint(BusConstants.ReadStammdatenQueue,
                    e => { e.ConfigureConsumer<ReadStammdatenConsumer>(context); });


                // CollectionManager Methods
                cfg.ReceiveEndpoint(string.Format(BusConstants.LesesaalManagerRequestBase, nameof(GetAllCollectionsRequest)),
                    e => e.ConfigureConsumer<SimpleConsumer<GetAllCollectionsRequest, GetAllCollectionsResponse, ICollectionManager>>(context));
                cfg.ReceiveEndpoint(string.Format(BusConstants.LesesaalManagerRequestBase, nameof(GetActiveCollectionsRequest)),
                    e => e.ConfigureConsumer<SimpleConsumer<GetActiveCollectionsRequest, GetActiveCollectionsResponse, ICollectionManager>>(
                        context));
                cfg.ReceiveEndpoint(string.Format(BusConstants.LesesaalManagerRequestBase, nameof(GetCollectionsHeaderRequest)),
                    e => e.ConfigureConsumer<SimpleConsumer<GetCollectionsHeaderRequest, GetCollectionsHeaderResponse, ICollectionManager>>(
                        context));
                cfg.ReceiveEndpoint(string.Format(BusConstants.LesesaalManagerRequestBase, nameof(GetCollectionRequest)),
                    e => e.ConfigureConsumer<SimpleConsumer<GetCollectionRequest, GetCollectionResponse, ICollectionManager>>(context));
                cfg.ReceiveEndpoint(string.Format(BusConstants.LesesaalManagerRequestBase, nameof(InsertOrUpdateCollectionRequest)),
                    e => e
                        .ConfigureConsumer<SimpleConsumer<InsertOrUpdateCollectionRequest, InsertOrUpdateCollectionResponse, ICollectionManager>>(
                            context));
                cfg.ReceiveEndpoint(string.Format(BusConstants.LesesaalManagerRequestBase, nameof(DeleteCollectionRequest)),
                    e => e.ConfigureConsumer<SimpleConsumer<DeleteCollectionRequest, DeleteCollectionResponse, ICollectionManager>>(context));
                cfg.ReceiveEndpoint(string.Format(BusConstants.LesesaalManagerRequestBase, nameof(BatchDeleteCollectionRequest)),
                    e => e.ConfigureConsumer<SimpleConsumer<BatchDeleteCollectionRequest, BatchDeleteCollectionResponse, ICollectionManager>>(
                        context));
                cfg.ReceiveEndpoint(string.Format(BusConstants.LesesaalManagerRequestBase, nameof(GetPossibleParentsRequest)),
                    e => e.ConfigureConsumer<SimpleConsumer<GetPossibleParentsRequest, GetPossibleParentsResponse, ICollectionManager>>(context));
                cfg.ReceiveEndpoint(string.Format(BusConstants.LesesaalManagerRequestBase, nameof(GetImageRequest)),
                    e => e.ConfigureConsumer<SimpleConsumer<GetImageRequest, GetImageResponse, ICollectionManager>>(context));
                cfg.ReceiveEndpoint(string.Format(BusConstants.LesesaalManagerRequestBase, nameof(GetCollectionItemResultRequest)),
                    e => e.ConfigureConsumer<SimpleConsumer<GetCollectionItemResultRequest, GetCollectionItemResultResponse, ICollectionManager>>(
                        context));

                // Synchronisation
                cfg.ReceiveEndpoint(string.Format(BusConstants.LesesaalManagerRequestBase, nameof(GetLogDataRequest)),
                    e => e.ConfigureConsumer<SimpleConsumer<GetLogDataRequest, GetLogDataResponse, ISynchronisationManager>>(context));
                cfg.ReceiveEndpoint(string.Format(BusConstants.LesesaalManagerRequestBase, nameof(GetSyncDataRequest)),
                    e => e.ConfigureConsumer<SimpleConsumer<GetSyncDataRequest, GetSyncDataResponse, ISynchronisationManager>>(context));
                cfg.ReceiveEndpoint(string.Format(BusConstants.LesesaalManagerRequestBase, nameof(BatchAddSyncActionsRequest)),
                    e => e.ConfigureConsumer<SimpleConsumer<BatchAddSyncActionsRequest, BatchAddSyncActionsResponse, ISynchronisationManager>>(
                        context));
                cfg.ReceiveEndpoint(string.Format(BusConstants.LesesaalManagerRequestBase, nameof(GetSyncNumberPerHourRequest)),
                    e => e.ConfigureConsumer<SimpleConsumer<GetSyncNumberPerHourRequest, GetSyncNumberPerHourResponse, ISynchronisationManager>>(
                        context));

                // Sync Action Methods
                cfg.ReceiveEndpoint(string.Format(BusConstants.LesesaalManagerRequestBase, nameof(GetPendingMutationsRequest)),
                    e => e.ConfigureConsumer<SimpleConsumer<GetPendingMutationsRequest, GetPendingMutationsResponse, ILesesaalDataProvider>>(
                        context));
                cfg.ReceiveEndpoint(string.Format(BusConstants.LesesaalManagerRequestBase, nameof(UpdateMutationStatusRequest)),
                    e => e.ConfigureConsumer<SimpleConsumer<UpdateMutationStatusRequest, UpdateMutationStatusResponse, ILesesaalDataProvider>>(
                        context));
                cfg.ReceiveEndpoint(string.Format(BusConstants.LesesaalManagerRequestBase, nameof(BulkUpdateMutationStatusRequest)),
                    e => e
                        .ConfigureConsumer<
                            SimpleConsumer<BulkUpdateMutationStatusRequest, BulkUpdateMutationStatusResponse, ILesesaalDataProvider>>(context));
                cfg.ReceiveEndpoint(string.Format(BusConstants.LesesaalManagerRequestBase, nameof(ResetFailedSyncOperationsRequest)),
                    e => e
                        .ConfigureConsumer<
                            SimpleConsumer<ResetFailedSyncOperationsRequest, ResetFailedSyncOperationsResponse, ILesesaalDataProvider>>(context));
                cfg.ReceiveEndpoint(string.Format(BusConstants.LesesaalManagerRequestBase, nameof(InsertSyncActionRequest)),
                    e => e.ConfigureConsumer<InsertSyncActionRequestConsumer>(context));

                cfg.ReceiveEndpoint(string.Format(BusConstants.LesesaalManagerRequestBase, nameof(DeleteOldSyncActionRequest)),
                    e => e.ConfigureConsumer<DeleteOldSyncActionRequestConsumer>(context));

                var helper = new ParameterBusHelper();
                helper.SubscribeAllSettingsInAssembly(Assembly.GetExecutingAssembly(), cfg);
            });

            var provider = services.BuildServiceProvider();
            bus = provider.GetRequiredService<IBusControl>();

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

        private void AddConsumers(IBusRegistrationConfigurator x)
        {
            // registers all IConsumer implementations in this assembly
            x.AddConsumers(Assembly.GetExecutingAssembly());

            // register all the generic implementations
            // CollectionManager
            x.AddConsumer<SimpleConsumer<GetAllCollectionsRequest, GetAllCollectionsResponse, ICollectionManager>>();
            x.AddConsumer<SimpleConsumer<GetActiveCollectionsRequest, GetActiveCollectionsResponse, ICollectionManager>>();
            x.AddConsumer<SimpleConsumer<GetCollectionsHeaderRequest, GetCollectionsHeaderResponse, ICollectionManager>>();
            x.AddConsumer<SimpleConsumer<GetCollectionRequest, GetCollectionResponse, ICollectionManager>>();
            x.AddConsumer<SimpleConsumer<InsertOrUpdateCollectionRequest, InsertOrUpdateCollectionResponse, ICollectionManager>>();
            x.AddConsumer<SimpleConsumer<DeleteCollectionRequest, DeleteCollectionResponse, ICollectionManager>>();
            x.AddConsumer<SimpleConsumer<BatchDeleteCollectionRequest, BatchDeleteCollectionResponse, ICollectionManager>>();
            x.AddConsumer<SimpleConsumer<GetPossibleParentsRequest, GetPossibleParentsResponse, ICollectionManager>>();
            x.AddConsumer<SimpleConsumer<GetImageRequest, GetImageResponse, ICollectionManager>>();
            x.AddConsumer<SimpleConsumer<GetCollectionItemResultRequest, GetCollectionItemResultResponse, ICollectionManager>>();
            
            // Sync Action
            x.AddConsumer<SimpleConsumer<GetLogDataRequest, GetLogDataResponse, ISynchronisationManager>>();
            x.AddConsumer<SimpleConsumer<GetSyncDataRequest, GetSyncDataResponse, ISynchronisationManager>>();
            x.AddConsumer<SimpleConsumer<BatchAddSyncActionsRequest, BatchAddSyncActionsResponse, ISynchronisationManager>>();
            x.AddConsumer<SimpleConsumer<GetSyncNumberPerHourRequest, GetSyncNumberPerHourResponse, ISynchronisationManager>>();
            x.AddConsumer<SimpleConsumer<GetPendingMutationsRequest, GetPendingMutationsResponse, ILesesaalDataProvider>>();
            x.AddConsumer<SimpleConsumer<UpdateMutationStatusRequest, UpdateMutationStatusResponse, ILesesaalDataProvider>>();
            x.AddConsumer<SimpleConsumer<BulkUpdateMutationStatusRequest, BulkUpdateMutationStatusResponse, ILesesaalDataProvider>>();
            x.AddConsumer<SimpleConsumer<ResetFailedSyncOperationsRequest, ResetFailedSyncOperationsResponse, ILesesaalDataProvider>>();
        }
    }
}