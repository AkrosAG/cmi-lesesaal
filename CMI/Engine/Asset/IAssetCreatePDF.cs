using CMI.Contract.Messaging;
using System.Threading.Tasks;

namespace CMI.Engine.Asset;

public interface IAssetCreatePDF
{
    string CreateTitlePage(GetAISDateienRequest request);
    Task<byte[]> GetFileContentFromUrlAsync(string url);

    Task<byte[]> GetFileContentFromPathAsync(string path);

    Task<byte[]> AssetAddTitlePageToPDFToTitle(byte[] memory, string filePath);
}