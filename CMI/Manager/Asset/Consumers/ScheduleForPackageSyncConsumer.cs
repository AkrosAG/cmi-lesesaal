using System.Threading.Tasks;
using CMI.Contract.Common;
using CMI.Contract.Messaging;
using MassTransit;
using Serilog;
using Serilog.Context;
using Serilog.Core.Enrichers;

namespace CMI.Manager.Asset.Consumers
{
    public class ScheduleForPackageSyncConsumer : IConsumer<IScheduleForPackageSync>
    {
        private readonly IAssetManager assetManager;
        private readonly IBus bus;

        public ScheduleForPackageSyncConsumer(IAssetManager assetManager, IBus bus)
        {
            this.assetManager = assetManager;
            this.bus = bus;
        }

        public async Task Consume(ConsumeContext<IScheduleForPackageSync> context)
        {
            var conversationEnricher = new PropertyEnricher(nameof(context.ConversationId), context.ConversationId);
            var archiveRecordIdEnricher = new PropertyEnricher(nameof(context.Message.Workload.ArchiveRecord.ArchiveRecordId),
                context.Message.Workload.ArchiveRecord.ArchiveRecordId);

            using (LogContext.Push(conversationEnricher, archiveRecordIdEnricher))
            {
                Log.Information("Received {CommandName} command.", nameof(IScheduleForPackageSync));

                var archiveRecord = context.Message.Workload.ArchiveRecord;
                var elasticArchiveRecord = context.Message.Workload.ElasticRecord;
                await assetManager.RegisterJobInPreparationQueue(archiveRecord.ArchiveRecordId, elasticArchiveRecord.PrimaryDataLink,
                    AufbereitungsArtEnum.Sync, AufbereitungsServices.AssetService,
                    elasticArchiveRecord.PrimaryData, context.Message.Workload);
            }
        }
    }
}