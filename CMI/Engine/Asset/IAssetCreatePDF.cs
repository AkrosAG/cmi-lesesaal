using CMI.Contract.Messaging;
using System.Threading.Tasks;

namespace CMI.Engine.Asset;

public interface IAssetCreatePDF
{
    byte[] CreateTitlePage(GetAISDateienRequest request);
    Task<byte[]> GetFileContentFromUrlAsync(string url);
    byte[] MergeTitlePageWithContent(byte[] titlePageBytes, byte[] contentBytes);
}