using System.Threading.Tasks;
using CMI.Access.Repository.Systems.Dir;
using CMI.Contract.Monitoring;
using MassTransit;
using Serilog;
using Serilog.Context;

namespace CMI.Manager.Repository.Consumer
{
    public class CheckRepositoryConsumer : IConsumer<RepositoryCheckRequest>
    {
        private readonly IRepositoryCheck repositoryCheck;


        public CheckRepositoryConsumer(IRepositoryCheck repositoryCheck)
        {
            this.repositoryCheck = repositoryCheck;
        }


        public async Task Consume(ConsumeContext<RepositoryCheckRequest> context)
        {
            using (LogContext.PushProperty(nameof(context.ConversationId), context.ConversationId))
            {
                Log.Information("Received {CommandName} command with conversationId {ConversationId} from the bus", nameof(RepositoryCheckRequest),
                    context.ConversationId);

                var response = repositoryCheck.GetRepositoryResponse();

                await context.RespondAsync(response);
            }
        }
    }
}