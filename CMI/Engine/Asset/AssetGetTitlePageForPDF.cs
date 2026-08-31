using Aspose.Pdf;
using CMI.Contract.Parameter;
using CMI.Engine.Asset.ParameterSettings;
using CMI.Utilities.License;
using Serilog;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CMI.Engine.Asset;

/// <summary>
/// Erstellt Titelseiten für AIS-PDF-Downloads und führt sie mit dem Originaldokument zusammen.
/// Das HTML-Template und das Logo werden aus <see cref="TitelblattSettings"/> gelesen
/// und können über die Management-Oberfläche pro Kunde überschrieben werden.
/// </summary>
public class AssetGetTitlePageForPDF : IAssetGetTitlePageForPDF
{
    private readonly IParameterHelper parameterHelper;

    public AssetGetTitlePageForPDF(IParameterHelper parameterHelper)
    {
        LicenseHelper.SetAsposeLicense();
        this.parameterHelper = parameterHelper;
    }

    /// <summary>
    /// Erstellt eine PDF-Titelseite aus dem konfigurierten Template und den übergebenen Metadaten.
    /// Gibt <c>null</c> zurück, wenn kein Template konfiguriert ist oder das Template fehlerhaft ist.
    /// </summary>
    /// <param name="metadaten">
    /// Key-Value-Paare der Template-Platzhalter. Schlüssel entsprechen direkt den
    /// {{placeholder}}-Namen im Mustache-Template (z.B. "titel", "signatur", "permanente_url").
    /// </param>
    public byte[] CreateTitlePage(Dictionary<string, string> metadaten)
    {
        var settings = parameterHelper.GetSetting<TitelblattSettings>();

        if (string.IsNullOrWhiteSpace(settings?.HtmlTemplate))
        {
            Log.Information("Kein Titelblatt-Template konfiguriert – Titelseite wird übersprungen.");
            return null;
        }

        try
        {
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
        catch (System.Exception ex)
        {
            Log.Warning(ex, "Titelblatt konnte nicht erstellt werden – Titelseite wird übersprungen.");
            return null;
        }
    }
}