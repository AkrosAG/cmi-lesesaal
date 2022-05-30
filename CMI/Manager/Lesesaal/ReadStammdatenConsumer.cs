using System.Threading.Tasks;
using CMI.Access.Sql.Lesesaal;
using CMI.Contract.Messaging;
using CMI.Manager.Lesesaal.Properties;
using MassTransit;
using Serilog;
using Serilog.Context;

namespace CMI.Manager.Lesesaal
{
    public class ReadStammdatenConsumer : IConsumer<GetStammdatenRequest>
    {
        private StammdatenDataAccess dataAccess;


        public async Task Consume(ConsumeContext<GetStammdatenRequest> context)
        {
            if (dataAccess == null)
            {
                dataAccess = new StammdatenDataAccess(DbConnectionSetting.Default.ConnectionString);
            }

            using (LogContext.PushProperty(nameof(context.ConversationId), context.ConversationId))
            {
                Log.Information("Received {CommandName} command with conversationId {ConversationId} from the bus", nameof(GetStammdatenRequest),
                    context.ConversationId);

                await context.RespondAsync(new GetStammdatenResponse
                {
                    NamesAndIds = dataAccess.GetNamesAndIds(context.Message.BezeichnungDerStammdaten, context.Message.Language)
                });

                Log.Information("Response sent.");
            }
        }
    }
}