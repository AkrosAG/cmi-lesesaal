using System;
using System.Threading.Tasks;
using CMI.Contract.Asset;
using CMI.Contract.Common;
using CMI.Contract.Messaging;
using CMI.Manager.Repository.Properties;
using MassTransit;
using Serilog;
using Serilog.Context;
using Serilog.Core.Enrichers;

namespace CMI.Manager.Repository.Consumer
{
    public class DownloadPackageConsumer : IConsumer<IDownloadPackage>
    {
        private readonly IBus bus;
        private readonly IRepositoryManager repositoryManager;

        /// <summary>
        ///     Initializes a new instance of the <see cref="DownloadPackageConsumer" /> class.
        /// </summary>
        /// <param name="repositoryManager">The repository manager.</param>
        /// <param name="bus">The bus.</param>
        public DownloadPackageConsumer(IRepositoryManager repositoryManager, IBus bus)
        {
            this.repositoryManager = repositoryManager;
            this.bus = bus;
        }

        public async Task Consume(ConsumeContext<IDownloadPackage> context)
        {
            var conversationEnricher = new PropertyEnricher(nameof(context.ConversationId), context.ConversationId);
            var archiveRecordId = context.Message.ElasticArchiveRecord.ArchiveRecordId;
            var archiveRecordIdEnricher = new PropertyEnricher(nameof(archiveRecordId), archiveRecordId);
            var packageIdEnricher = new PropertyEnricher(nameof(context.Message.PackageId), context.Message.PackageId);

            using (LogContext.Push(conversationEnricher, archiveRecordIdEnricher, packageIdEnricher))
            {
                Log.Information("Received {CommandName} command with conversationId {ConversationId} from the bus", nameof(IDownloadPackage),
                    context.ConversationId);

                RepositoryPackage repositoryPackage = null;
                var succes = false;
                var errorMessage = string.Empty;

                if (Settings.Default.RepositoryManager == "rosetta")
                {
                    var result = await repositoryManager.ReadPackageMetadata(context.Message.ElasticArchiveRecord);
                    succes = result.Success && result.Valid;
                    repositoryPackage = result.PackageDetails;
                    errorMessage = result.ErrorMessage;
                }
                else
                {
                    // Get the package from the repository
                    // We are not waiting for it to end, because we want to free the consumer as early as possible
                    var packageId = context.Message.PackageId;
                    var result = await repositoryManager.GetPackage(packageId, archiveRecordId, context.Message.PrimaerdatenAuftragId);
                    succes = result.Success && result.Valid; 
                    repositoryPackage = result.PackageDetails;
                    errorMessage = result.ErrorMessage;
                }

                // Do we have a valid package?
                if (succes)
                {
                    // Forward the downloaded package to the asset manager for transformation
                    var endpoint = await context.GetSendEndpoint(new Uri(bus.Address, BusConstants.AssetManagerPrepareForTransformation));

                    await endpoint.Send(new PrepareForTransformationMessage
                    {
                        AssetType = AssetType.Gebrauchskopie,
                        CallerId = context.Message.CallerId,
                        RetentionCategory = context.Message.RetentionCategory,
                        Recipient = context.Message.Recipient,
                        Language = context.Message.Language,
                        ProtectWithPassword = context.Message.RetentionCategory != CacheRetentionCategory.UsageCopyPublic,
                        PrimaerdatenAuftragId = context.Message.PrimaerdatenAuftragId,
                        RepositoryPackage = repositoryPackage
                    });

                    // also publish the event, that the package is downloaded
                    await context.Publish<IPackageDownloaded>(
                        new
                        {
                            PackageInfo = repositoryPackage
                        });
                }
                else
                {
                    // Publish the download asset failure event
                    await context.Publish<IAssetReady>(new AssetReady
                    {
                        Valid = false,
                        ErrorMessage = errorMessage,
                        AssetType = AssetType.Gebrauchskopie,
                        CallerId = context.Message.CallerId,
                        ArchiveRecordId = archiveRecordId,
                        RetentionCategory = context.Message.RetentionCategory,
                        Recipient = context.Message.Recipient,
                        PrimaerdatenAuftragId = context.Message.PrimaerdatenAuftragId
                    });
                }
            }
        }
    }
}