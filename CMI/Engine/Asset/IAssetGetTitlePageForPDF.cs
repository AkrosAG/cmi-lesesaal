using System.Collections.Generic;

namespace CMI.Engine.Asset;

public interface IAssetGetTitlePageForPDF
{
    byte[] CreateTitlePage(Dictionary<string, string> metadaten);
}