using System;
using System.Collections.Generic;
using System.IO;
using Aspose.Pdf;
using Aspose.Pdf.Text;
using CMI.Contract.DocumentConverter;
using CMI.Manager.DocumentConverter.Extraction.Interfaces;
using CMI.Manager.DocumentConverter.Properties;
using Serilog;
using License = Aspose.Pdf.License;

namespace CMI.Manager.DocumentConverter.Extraction
{
    public class AsposePdfTextExtractor : TextExtractorBase
    {
        private static readonly string[] extensions = {"pdf"};


        static AsposePdfTextExtractor()
        {
            try
            {
                var licensePdf = new License();
                // Retrieve the license content from application settings
                string licenseContent = DocumentConverterSettings.Default.AsposeLicense;
                if (string.IsNullOrWhiteSpace(licenseContent) || licenseContent.Contains("@@"))
                {
                    throw new Exception("License content is missing or placeholder is still present in application settings.");
                }

                // Convert the license content to a stream
                using (var licenseStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(licenseContent)))
                {

                    licensePdf.SetLicense(licenseStream);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unexpected error while setting Aspose license.");
                throw;
            }
        }

        public override IEnumerable<string> AllowedExtensions => extensions;
        public override int Rank => 3;
        public override bool IsAvailable => true;

        public override ExtractionResult ExtractText(IDoc doc, ITextExtractorSettings settings)
        {
            var result = new ExtractionResult(settings.MaxExtractionSize);

            var textAbsorber = new TextAbsorber();

            using (var pdfDocument = new Document(doc.Stream))
            {
                pdfDocument.Pages.Accept(textAbsorber);
            }

            result.Append(textAbsorber.Text);

            return result;
        }
    }
}