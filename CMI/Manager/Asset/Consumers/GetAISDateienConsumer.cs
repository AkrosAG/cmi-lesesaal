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
            var filePath = assetCreatePdf.CreateTitlePage(context.Message);

            var stream = await assetCreatePdf.GetFileContentFromUrlAsync(context.Message.URLDatei);

            await assetCreatePdf.AssetAddTitlePageToPDFToTitle(stream, filePath);
        }
    }
}