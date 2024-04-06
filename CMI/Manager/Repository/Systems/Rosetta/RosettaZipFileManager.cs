using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

using CMI.Access.Repository.Systems.Rosetta;
using CMI.Contract.Common.Gebrauchskopie;
using CMI.Manager.Repository.Properties;
using Serilog;

namespace CMI.Manager.Repository.Systems.Rosetta
{
    public static class RosettaZipFileManager
    {
        public static async Task BuildZipFileAsync(string sourcePath, string archiveRecordId, PaketDIP package)
        {
            var targetFile = Path.Combine(Settings.Default.FileCopyDestinationPath, archiveRecordId + ".zip");
            var zipBaseDir = Path.Combine(Settings.Default.FileCopyDestinationPath, archiveRecordId);

            var contentDir = Path.Combine(zipBaseDir, "content");
            var headerDir = Path.Combine(zipBaseDir, "header");
            Directory.CreateDirectory(headerDir); 

            await Task.Run(() => RosettaDataAccess.CopyDirectory(sourcePath, contentDir));

            var metadataXmlPath = Path.Combine(headerDir, "metadata.xml");
            await Task.Run(() => ((Paket)package).SaveToFile(metadataXmlPath)); 

            if (File.Exists(targetFile))
            {
                File.Delete(targetFile); 
            }

            await Task.Run(() => ZipFile.CreateFromDirectory(zipBaseDir, targetFile));
            Log.Information("Created zip file {0}", targetFile);
            await Task.Run(() => Directory.Delete(zipBaseDir, true)); 
        }
    }
}
