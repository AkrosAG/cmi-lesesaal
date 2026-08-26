using CMI.Contract.Messaging;
using CMI.Engine.Asset;
using MassTransit;
using Serilog.Context;
using System.Threading.Tasks;

namespace CMI.Manager.Asset.Consumers;

public class GetAISDateienConsumer : IConsumer<GetAISDateienRequest>
{
    private readonly IAssetCreatePDF assetCreatePdf;

    public GetAISDateienConsumer(IAssetCreatePDF assetCreatePdf)
    {
        this.assetCreatePdf = assetCreatePdf;
    }

    public async Task Consume(ConsumeContext<GetAISDateienRequest> context)
    {
        using (LogContext.PushProperty(nameof(context.ConversationId), context.ConversationId))
        {
            var titlePageBytes = assetCreatePdf.CreateTitlePage(context.Message.Metadaten);
            await context.RespondAsync(new GetAISDateienResult { TitlePagePdfBytes = titlePageBytes });
        }
    }
}