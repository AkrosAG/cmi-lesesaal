using CMI.Contract.Common;
using MassTransit;
using Serilog;
using System;
using System.Threading.Tasks;
using LogContext = Serilog.Context.LogContext;

namespace CMI.Manager.DataFeed.Consumers
{
    /// <summary>
    ///     Consumer for ActaProSyncRecord messages.
    ///     Stores the received records in SyncAction table for further processing.
    /// </summary>
    public class SyncRecordConsumer : IConsumer<MutationRecord>
    {
        private readonly IDataFeedManager dataFeedManager;

        /// <summary>
        ///     Initializes a new instance of the <see cref="SyncRecordConsumer"/> class.
        /// </summary>
        /// <param name="dataFeedManager">Manager to process the sync action.</param>
        public SyncRecordConsumer(IDataFeedManager _dataFeedManager)
        {
            dataFeedManager = _dataFeedManager;
        }

        /// <summary>
        ///     Consumes the ActaProSyncRecorActaProSyncRecordd message and stores it for syncing.
        /// </summary>
        /// <param name="context">Message context.</param>
        public async Task Consume(ConsumeContext<MutationRecord> context)
        {
            using (LogContext.PushProperty(nameof(context.ConversationId), context.ConversationId))
            {
                try
                {
                    var message = context.Message;
                    Log.Information("Received {CommandName} message with ArchiveRecordId: {ArchiveRecordId}, Action: {Action}, ConversationId: {ConversationId}",
                        nameof(MutationRecord), message.ArchiveRecordId, message.Action, context.ConversationId);

                    await dataFeedManager.HandleSyncRecordAsync(message);

                    Log.Information("SyncAction inserted for ArchiveRecordId: {ArchiveRecordId}", message.ArchiveRecordId);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Unexpected error while handling MutationRecord message.");
                    throw;
                }
            }
        }
    }
}
