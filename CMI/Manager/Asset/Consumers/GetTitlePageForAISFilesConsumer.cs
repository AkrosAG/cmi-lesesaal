using CMI.Contract.Messaging;
using CMI.Engine.Asset;
using MassTransit;
using Serilog.Context;
using System.Threading.Tasks;

namespace CMI.Manager.Asset.Consumers;

public class GetTitlePageForAISFilesConsumer : IConsumer<GetTitlePageForAISFilesRequest>
{
    private readonly IAssetGetTitlePageForPDF assetGetTitlePageForPdf;

    public GetTitlePageForAISFilesConsumer(IAssetGetTitlePageForPDF assetGetTitlePageForPdf)
    {
        this.assetGetTitlePageForPdf = assetGetTitlePageForPdf;
    }

    public async Task Consume(ConsumeContext<GetTitlePageForAISFilesRequest> context)
    {
        using (LogContext.PushProperty(nameof(context.ConversationId), context.ConversationId))
        {
            var titlePageBytes = assetGetTitlePageForPdf.CreateTitlePage(context.Message.Metadaten);
            await context.RespondAsync(new GetTitlePageForAISFilesResult { TitlePagePdfBytes = titlePageBytes });
        }
    }
}