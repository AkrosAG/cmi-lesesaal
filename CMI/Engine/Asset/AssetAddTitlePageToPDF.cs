using Aspose.Pdf;
using CMI.Contract.Messaging;
using CMI.Utilities.License;
using Newtonsoft.Json.Linq;
using Serilog;
using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Path = System.IO.Path;

namespace CMI.Engine.Asset;

/// <summary>
/// Implementiert <see cref="IAssetCreatePDF"/> und stellt Methoden bereit,
/// um Titelseiten für PDF-Dokumente zu erstellen und diese mit dem eigentlichen
/// Dokument zusammenzuführen.
/// </summary>
public class AssetAddTitlePageToPDF : IAssetCreatePDF
{
    private static readonly HttpClient httpClient = new HttpClient();

    public AssetAddTitlePageToPDF()
    {
        LicenseHelper.SetAsposeLicense();
    }

    /// <summary>
    /// Erstellt eine PDF-Titelseite basierend auf den Metadaten des <see cref="GetAISDateienRequest"/>
    /// und gibt sie als Byte-Array zurück.
    /// </summary>
    public byte[] CreateTitlePage(GetAISDateienRequest request)
    {
        var templatePath = Path.Combine(request.TemplatesDefinitionDirectory, "TitelBlattPDF.Body.mustache");
        var htmlContent = File.ReadAllText(templatePath);

        var placeholders = new JObject
        {
            { "logo_url", Path.Combine(request.TemplatesDefinitionDirectory, "logo.svg") },
            { "titel", request.Titel },
            { "entstehungszeitraum", request.Entstehungszeitraum },
            { "urheber", request.Urheber },
            { "signatur", request.Signatur },
            { "permanente_url", request.URLVerzeichniseinheit }
        };

        foreach (var property in placeholders.Properties())
        {
            htmlContent = htmlContent.Replace("{{" + property.Name + "}}", property.Value?.ToString() ?? string.Empty);
        }

        using var ms = new MemoryStream(Encoding.UTF8.GetBytes(htmlContent));
        var pdfDocument = new Document(ms, new HtmlLoadOptions());
        using var outputStream = new MemoryStream();
        pdfDocument.Save(outputStream);
        return outputStream.ToArray();
    }

    /// <summary>
    /// Lädt den Inhalt einer Datei von einer URL herunter und gibt ihn als Byte-Array zurück.
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