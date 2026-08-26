using Aspose.Pdf;
using CMI.Contract.Parameter;
using CMI.Engine.Asset.ParameterSettings;
using CMI.Utilities.License;
using Serilog;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace CMI.Engine.Asset;

/// <summary>
/// Erstellt Titelseiten für AIS-PDF-Downloads und führt sie mit dem Originaldokument zusammen.
/// Das HTML-Template und das Logo werden aus <see cref="TitelblattSettings"/> gelesen
/// und können über die Management-Oberfläche pro Kunde überschrieben werden.
/// </summary>
public class AssetAddTitlePageToPDF : IAssetCreatePDF
{
    private static readonly HttpClient httpClient = new HttpClient();
    private readonly IParameterHelper parameterHelper;

    public AssetAddTitlePageToPDF(IParameterHelper parameterHelper)
    {
        LicenseHelper.SetAsposeLicense();
        this.parameterHelper = parameterHelper;
    }

    /// <summary>
    /// Erstellt eine PDF-Titelseite aus dem konfigurierten Template und den übergebenen Metadaten.
    /// </summary>
    /// <param name="metadaten">
    /// Key-Value-Paare der Template-Platzhalter. Schlüssel entsprechen direkt den
    /// {{placeholder}}-Namen im Mustache-Template (z.B. "titel", "signatur", "permanente_url").
    /// </param>
    public byte[] CreateTitlePage(Dictionary<string, string> metadaten)
    {
        var settings = parameterHelper.GetSetting<TitelblattSettings>();

        var html = settings.HtmlTemplate;
        html = html.Replace("{{logo_base64}}", settings.LogoBase64 ?? string.Empty);

        foreach (var kv in metadaten)
        {
            html = html.Replace("{{" + kv.Key + "}}", kv.Value ?? string.Empty);
        }

        using var ms = new MemoryStream(Encoding.UTF8.GetBytes(html));
        var pdfDocument = new Document(ms, new HtmlLoadOptions());
        using var outputStream = new MemoryStream();
        pdfDocument.Save(outputStream);
        return outputStream.ToArray();
    }

    /// <summary>
    /// Lädt den Inhalt einer Datei von einer URL herunter.
    /// </summary>
    public async Task<byte[]> GetFileContentFromUrlAsync(string url)
    {
        Log.Information("Lade Datei von URL: {Url}", url);
        return await httpClient.GetByteArrayAsync(url);
    }

    /// <summary>
    /// Fügt die Titelseite dem PDF-Inhalt voran und gibt das zusammengeführte PDF zurück.
    /// </summary>
    public byte[] MergeTitlePageWithContent(byte[] titlePageBytes, byte[] contentBytes)
    {
        Log.Information("Führe Titelseite mit PDF-Inhalt zusammen");

        using var titleStream = new MemoryStream(titlePageBytes);
        using var contentStream = new MemoryStream(contentBytes);

        var titleDocument = new Document(titleStream);
        var contentDocument = new Document(contentStream);

        titleDocument.Pages.Add(contentDocument.Pages);

        using var outputStream = new MemoryStream();
        titleDocument.Save(outputStream);
        return outputStream.ToArray();
    }
}