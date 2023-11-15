using System;
using System.Threading.Tasks;
using CMI.Contract.Common;
using CMI.Contract.Messaging;
using MassTransit;
using Serilog;
using Serilog.Context;

namespace CMI.Manager.Index.Consumer
{
    /// <summary>
    ///     Class UpdateArchiveRecordConsumer.
    /// </summary>
    /// <seealso cref="IUpdateArchiveRecord" />
    public class UpdateArchiveRecordConsumer : IConsumer<IUpdateArchiveRecord>
    {
        private readonly IIndexManager indexManager;

        /// <summary>
        ///     Initializes a new instance of the <see cref="UpdateArchiveRecordConsumer" /> class.
        /// </summary>
        /// <param name="indexManager">The index manager that is responsible for updating.</param>
        public UpdateArchiveRecordConsumer(IIndexManager indexManager)
        {
            this.indexManager = indexManager;
        }

        /// <summary>
        ///     Consumes the specified message from the bus.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>Task.</returns>
        public async Task Consume(ConsumeContext<IUpdateArchiveRecord> context)
        {
            var currentStatus = AufbereitungsStatusEnum.OCRAbgeschlossen;
            using (LogContext.PushProperty(nameof(context.ConversationId), context.ConversationId))
            {
                Log.Information("Received {CommandName} command with conversationId {ConversationId} from the bus", nameof(IUpdateArchiveRecord),
                    context.ConversationId);

                try
                {
                    indexManager.UpdateArchiveRecord(context);
                    Log.Information($"Updated archive record {context.Message.ArchiveRecord.ArchiveRecordId} in elastic index.");
                    if (!context.Message.DoNotReportCompletion)
                    {
                        currentStatus = AufbereitungsStatusEnum.IndizierungAbgeschlossen;
                        await UpdatePrimaerdatenAuftragStatus(context, AufbereitungsServices.IndexService, currentStatus);

                        await context.Publish<IArchiveRecordUpdated>(new
                        {
                            context.Message.MutationId,
                            context.Message.ArchiveRecord.ArchiveRecordId,
                            ActionSuccessful = true,
                            context.Message.PrimaerdatenAuftragId
                        });
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Failed to update archiveRecord with conversationId {ConversationId} in Elastic or SQL", context.ConversationId);
                    if (!context.Message.DoNotReportCompletion)
                    {
                        await context.Publish<IArchiveRecordUpdated>(new
                        {
                            context.Message.MutationId,
                            context.Message.ArchiveRecord.ArchiveRecordId,
                            ActionSuccessful = false,
                            context.Message.PrimaerdatenAuftragId,
                            ErrorMessage = ex.Message,
                            ex.StackTrace
                        });
                        await UpdatePrimaerdatenAuftragStatus(context, AufbereitungsServices.IndexService, currentStatus, ex.Message);
                    }
                }
            }
        }

        private async Task UpdatePrimaerdatenAuftragStatus(ConsumeContext<IUpdateArchiveRecord> context, AufbereitungsServices service,
            AufbereitungsStatusEnum newStatus, string errorText = null)
        {
            if (context.Message.PrimaerdatenAuftragId > 0)
            {
                Log.Information("Auftrag mit Id {PrimaerdatenAuftragId} wurde im {service}-Service auf Status {Status} gesetzt.",
                    context.Message.PrimaerdatenAuftragId, service.ToString(), newStatus.ToString());

                var ep = await context.GetSendEndpoint(new Uri(context.SourceAddress,
                    BusConstants.AssetManagerUpdatePrimaerdatenAuftragStatusMessageQueue));
                await ep.Send<IUpdatePrimaerdatenAuftragStatus>(new UpdatePrimaerdatenAuftragStatus
                {
                    PrimaerdatenAuftragId = context.Message.PrimaerdatenAuftragId,
                    Service = service,
                    Status = newStatus,
                    ErrorText = errorText
                });
            }
        }
    }
}