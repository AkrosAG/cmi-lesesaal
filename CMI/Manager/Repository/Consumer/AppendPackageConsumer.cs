using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using CMI.Contract.Common;
using CMI.Contract.Messaging;
using CMI.Manager.Repository.Properties;
using MassTransit;
using Serilog;
using Serilog.Context;
using Serilog.Core.Enrichers;

namespace CMI.Manager.Repository.Consumer
{
    public class AppendPackageConsumer : IConsumer<IArchiveRecordAppendPackage>
    {
        private readonly IRepositoryManager repositoryManager;

        /// <summary>
        ///     Initializes a new instance of the <see cref="DownloadPackageConsumer" /> class.
        /// </summary>
        /// <param name="repositoryManager">The repository manager.</param>
        public AppendPackageConsumer(IRepositoryManager repositoryManager)
        {
            this.repositoryManager = repositoryManager;
        }

        public async Task Consume(ConsumeContext<IArchiveRecordAppendPackage> context)
        {
            var conversationEnricher = new PropertyEnricher(nameof(context.ConversationId), context.ConversationId);
            var archiveRecordIdEnricher = new PropertyEnricher(nameof(context.Message.ArchiveRecord.ArchiveRecordId),
                context.Message.ArchiveRecord?.ArchiveRecordId);

            using (LogContext.Push(conversationEnricher, archiveRecordIdEnricher))
            {
                Log.Information("Received {CommandName} command with conversationId {ConversationId} from the bus",
                    nameof(IArchiveRecordAppendPackage), context.ConversationId);
                var vaidPackage = false;
                if (Settings.Default.RepositoryManager == "rosetta")
                {
                    // We already have the data, just set the order status and zip the file
                    vaidPackage = context.Message?.ArchiveRecord?.PrimaryData?.Count > 0;
                }
                else
                {
                    // Need to clear the existing primary data that just contain the metadata. 
                    // Required because the append Package is adding the same information again.
                    context.Message.ArchiveRecord?.PrimaryData.Clear();
                    // Get the package from the repository
                    var result = await repositoryManager.AppendPackageToArchiveRecord(context.Message.ArchiveRecord, context.Message.MutationId,
                        context.Message.PrimaerdatenAuftragId);

                    // Inform the world about the created package
                    if (result != null && result.Success && result.Valid)
                    {
                        vaidPackage = true;
                    }
                }

                // Inform the world about the created package
                if (vaidPackage)
                {
                    Log.Information("Package creation was successful for packageId {packageId}", context.Message.ElasticRecord.PrimaryDataLink);
                  //  Debug.Assert(context.Message.ElasticRecord.PrimaryData.First()..PackageFileName != null);
                    var endpoint = await context.GetSendEndpoint(new Uri(context.SourceAddress,
                        BusConstants.AssetManagerPrepareForRecognition));

                    await endpoint.Send<PrepareForRecognitionMessage>(new
                    {
                        context.Message.MutationId,
                        context.Message.ArchiveRecord,
                        context.Message.PrimaerdatenAuftragId
                    });
                }
                else
                {
                    // If package creation was not successful, stop syncing here and return failure.
                    Log.Error(
                        "Failed to extract primary data from repository for archiveRecord with conversationId {ConversationId} with RepositoryManager {RepositoryManager}",
                        context.ConversationId, Settings.Default.RepositoryManager);
                    await context.Publish<IArchiveRecordUpdated>(new
                    {
                        context.Message.MutationId,
                        context.Message.ArchiveRecord?.ArchiveRecordId,
                        context.Message.PrimaerdatenAuftragId,
                        ActionSuccessful = false,
                      
                    });
                }
            }
        }
    }
}