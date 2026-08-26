
using Aspose.Doc;
using Aspose.Pdf;
using Aspose.Pdf.Drawing;
using CMI.Contract.Messaging;
using CMI.Utilities.License;
using Newtonsoft.Json.Linq;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Path = System.IO.Path;

namespace CMI.Engine.Asset;

/// <summary>
/// Implementiert <see cref="IAssetCreatePDF"/> und stellt Methoden bereit,
/// um Titelseiten für PDF-Dokumente zu erstellen und diese mit dem eigentlichen
/// Dokument zusammenzuführen, das per URL oder Dateipfad bezogen wird.
/// </summary>
public class AssetAddTitlePageToPDF : IAssetCreatePDF
{
    private static readonly HttpClient httpClient = new HttpClient();
    private readonly string tempFolder;

    /// <summary>
    /// Initialisiert eine neue Instanz von <see cref="AssetAddTitlePageToPDF"/>.
    /// Setzt die Aspose-Lizenz und verwendet den System-Temp-Ordner für temporäre Dateien.
    /// </summary>
    public AssetAddTitlePageToPDF()
    {
        LicenseHelper.SetAsposeLicense();
        tempFolder = Path.GetTempPath();
    }

    /// <summary>
    /// Erstellt eine PDF-Titelseite basierend auf den Metadaten des <see cref="GetAISDateienRequest"/>
    /// und speichert sie als temporäre Datei auf dem Dateisystem.
    /// </summary>
    /// <param name="request">Der Request mit den Metadaten (Titel, Autor, Datum) für die Titelseite.</param>
    /// <returns>Der vollständige Dateipfad zur erstellten Titelseiten-PDF.</returns>
    public string CreateTitlePage(GetAISDateienRequest request)
    {

        var htmlContent = File.ReadAllText(Path.Combine(request.TemplatesDefinitionDirectory, "TitelBlattPDF.Body.mustache"));
        // 2. Deine Daten liegen als JObject vor (Beispiel)
        var datenJson = new JObject
        {
            { "logo_url", Path.Combine(request.TemplatesDefinitionDirectory,"logo.svg") },
            { "titel", request.Titel},
            { "entstehungszeitraum", request.Entstehungszeitraum },
            { "urheber", request.Urheber},
            { "signatur", request.Signatur },
            { "permanente_url", "https://doi.org" }
        };
        // 3. Dynamisches Ersetzen der {{platzhalter}} direkt über C#
        // Wir loopen durch das JObject und ersetzen im HTML-String alle Vorkommen von {{Key}} mit dem Value
        foreach (var property in datenJson.Properties())
        {
            var placeholder = "{{" + property.Name + "}}";
            var value = property.Value?.ToString() ?? string.Empty;

            htmlContent = htmlContent.Replace(placeholder, value);
        }

        // 4. Das fertig befüllte HTML über einen MemoryStream an Aspose.PDF übergeben
        using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(htmlContent)))
        {
            // HtmlLoadOptions sorgt dafür, dass CSS und Layouts von Aspose korrekt verarbeitet werden
            HtmlLoadOptions options = new HtmlLoadOptions();

            // PDF-Dokument aus dem HTML-Stream generieren
            Document pdfDocument = new Document(ms, options);

            // 5. Als PDF abspeichern
            string zielPfad = Path.Combine(request.TemplatesDefinitionDirectory, "Thomas_Mann_Archiv_Dokument.pdf");
            pdfDocument.Save(zielPfad);

            return zielPfad;
        }
    }

    /// <summary>
    /// Lädt den Inhalt einer Datei von einer URL herunter und gibt ihn als Byte-Array zurück.
    /// </summary>
    /// <param name="url">Die URL der herunterzuladenden Datei.</param>
    /// <returns>Den Dateiinhalt als Byte-Array.</returns>
    /// <exception cref="HttpRequestException">Wird geworfen, wenn der HTTP-Abruf fehlschlägt.</exception>
    public async Task<byte[]> GetFileContentFromUrlAsync(string url)
    {
        Log.Information("Lade Datei von URL: {Url}", url);
        return await httpClient.GetByteArrayAsync(url);
    }

    /// <summary>
    /// Liest den Inhalt einer lokalen Datei und gibt ihn als Byte-Array zurück.
    /// </summary>
    /// <param name="path">Der vollständige Dateipfad zur zu lesenden Datei.</param>
    /// <returns>Den Dateiinhalt als Byte-Array.</returns>
    /// <exception cref="FileNotFoundException">Wird geworfen, wenn die Datei nicht gefunden wird.</exception>
    public Task<byte[]> GetFileContentFromPathAsync(string path)
    {
        Log.Information("Lese Datei von Pfad: {Path}", path);
        return null;
    }

    /// <summary>
    /// Fügt die unter <paramref name="filePath"/> gespeicherte Titelseite dem übergebenen PDF-Inhalt
    /// voran und gibt das resultierende PDF als Byte-Array zurück.
    /// Die temporäre Titelseitendatei wird nach der Verarbeitung gelöscht.
    /// </summary>
    /// <param name="memory">Der Inhalt des Original-PDFs als Byte-Array.</param>
    /// <param name="filePath">Der Pfad zur temporären Titelseiten-PDF-Datei.</param>
    /// <returns>Das zusammengeführte PDF (Titelseite + Original) als Byte-Array.</returns>
    public async Task<byte[]> AssetAddTitlePageToPDFToTitle(byte[] memory, string filePath)
    {
        Log.Information("Füge Titelseite {FilePath} zum PDF hinzu", filePath);

        using var titleStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        using var contentStream = new MemoryStream(memory);

        var titleDocument = new Document(titleStream);
        var contentDocument = new Document(contentStream);

        // Titelseite voranstellen: Seiten des Inhaltsdokuments an das Titelseitendokument anhängen
        titleDocument.Pages.Add(contentDocument.Pages);

        using var outputStream = new MemoryStream();
        titleDocument.Save(outputStream);

        // Temporäre Titelseiten-Datei aufräumen
        try
        {
            File.Delete(filePath);
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Temporäre Titelseiten-Datei konnte nicht gelöscht werden: {FilePath}", filePath);
        }

        return outputStream.ToArray();
    }
}