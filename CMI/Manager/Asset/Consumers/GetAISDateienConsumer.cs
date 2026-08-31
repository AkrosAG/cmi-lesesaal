using CMI.Contract.Messaging;
using CMI.Engine.Asset;
using MassTransit;
using Serilog.Context;
using System.Threading.Tasks;

namespace CMI.Manager.Asset.Consumers;

public class GetAISDateienConsumer : IConsumer<GetAISDateienRequest>
{
    private readonly IAssetGetTitlePageForPDF assetGetTitlePageForPdf;

    public GetAISDateienConsumer(IAssetGetTitlePageForPDF assetGetTitlePageForPdf)
    {
        this.assetGetTitlePageForPdf = assetGetTitlePageForPdf;
    }

    public async Task Consume(ConsumeContext<GetAISDateienRequest> context)
    {
        using (LogContext.PushProperty(nameof(context.ConversationId), context.ConversationId))
        {
            var titlePageBytes = assetGetTitlePageForPdf.CreateTitlePage(context.Message.Metadaten);
            await context.RespondAsync(new GetAISDateienResult { TitlePagePdfBytes = titlePageBytes });
        }
    }
}