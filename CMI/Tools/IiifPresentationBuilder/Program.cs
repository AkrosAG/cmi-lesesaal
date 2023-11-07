using System;
using System.IO;
using CMI.Contract.Common.Gebrauchskopie;
using CMI.Engine.Asset.PostProcess;

namespace CMI.Tools.IiifPresentationBuilder
{
    internal class Program
    {
        static void Main(string[] args)
        {

            var root = @"C:\Temp\30409375";
            var archiveRecordId = "30409300";


            var metadataFile = new FileInfo(Path.Combine(root, "header", "metadata.xml"));
            var paket = (PaketDIP) Paket.LoadFromFile(metadataFile.FullName);

            var location = new ViewerFileLocationSettings
            {
                ManifestOutputSaveDirectory = "C:\\Temp\\ManifestTest\\manifests",
                ContentOutputSaveDirectory = "",
                OcrOutputSaveDirectory = ""
            };

            // Create Manifest
            var helper = new PostProcessManifestCreator(new IiifManifestSettings()
                {
                    ApiServerUri = new Uri("https://recherche.library.ethz.ch/clientdev/iiif/"),
                    ImageServerUri = new Uri("https://recherche.library.ethz.ch/image/"),
                    PublicManifestWebUri = new Uri("https://recherche.library.ethz.ch/clientdev/files/manifests/"),
                    PublicContentWebUri = new Uri("https://recherche.library.ethz.ch/clientdev/files/content/"),
                    PublicOcrWebUri = new Uri("https://recherche.library.ethz.ch/clientdev/files/ocr/"),
                    PublicDetailRecordUri = new Uri("https://recherche.library.ethz.ch/clientdev/")
                },
                location);


            helper.CreateManifest(archiveRecordId, paket, root);

            Console.WriteLine("Erzeugt");
            Console.ReadLine();

        }
    }
}
