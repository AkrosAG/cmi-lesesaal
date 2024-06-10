using CMI.Contract.Messaging;
using MassTransit;
using Serilog;
using System;
using System.Threading.Tasks;
using Serilog.Context;

namespace CMI.Manager.Index.Consumer
{
    internal class ConvertArchiveRecordConsumer : IConsumer<ConvertArchiveRecordRequest>
    {
        private readonly IIndexManager indexManager;

        public ConvertArchiveRecordConsumer(IIndexManager indexManager)
        {
            this.indexManager = indexManager;
        }

        public async Task Consume(ConsumeContext<ConvertArchiveRecordRequest> context)
        {
            using (LogContext.PushProperty(nameof(context.ConversationId), context.ConversationId))
            {
                Log.Information("Received {CommandName} command with conversationId {ConversationId} from the bus", nameof(ConvertArchiveRecordRequest),
                    context.ConversationId);

                try
                {
                    await context.RespondAsync(new ConvertArchiveRecordResponse
                    {
                        ElasticArchiveRecord = indexManager.ConvertArchiveRecord(context.Message.ArchiveRecord)
                    });
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Failed to convert archiveRecord with conversationId {ConversationId}", context.ConversationId);
                    throw;
                }
            }
        }
    }
}