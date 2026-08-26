using System.Collections.Generic;
using System.Threading.Tasks;

namespace CMI.Engine.Asset;

public interface IAssetCreatePDF
{
    byte[] CreateTitlePage(Dictionary<string, string> metadaten);
    Task<byte[]> GetFileContentFromUrlAsync(string url);
    byte[] MergeTitlePageWithContent(byte[] titlePageBytes, byte[] contentBytes);
}