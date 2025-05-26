using Aspose.Pdf;
using Aspose.Pdf.Text;
using CMI.Contract.DocumentConverter;
using CMI.Manager.DocumentConverter.Extraction.Interfaces;
using CMI.Utilities.License;
using System.Collections.Generic;

namespace CMI.Manager.DocumentConverter.Extraction
{
    public class AsposePdfTextExtractor : TextExtractorBase
    {
        private static readonly string[] extensions = {"pdf"};


        static AsposePdfTextExtractor()
        {
            LicenseHelper.SetAsposeLicense();
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