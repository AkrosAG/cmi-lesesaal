using System;
using System.IO;
using System.Linq;
using CMI.Access.Harvest.Properties;
using CMI.Contract.Common;
using Serilog;

namespace CMI.Access.Harvest
{
    public static class MetaDataExtension
    {
        public static bool IsOriginal(this Datei file, string compareWith = "original")
        {
            try
            {
                var value = file.LastVersion.Items[0].Ansicht;
                return string.Equals(value, compareWith, StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        public static string GetCdwsUrl(this Datei file, string cdwsEndpoint)
        {
            var baseUrl = cdwsEndpoint?.ToLower().Replace("index", "files");
            var fileUrl = $"{baseUrl}{file.File.ID}/{file.LastVersion.Nr}/{file.LastVersion.Items[0].Ansicht}";

            return fileUrl;
        }

        public static string GetFullPath(this Datei file, string root)
        {
            return Path.Combine(root,                        // CDWS_ROOT
                                $"{file.OBJ_GUID}",                     // OBJ_GUID
                                file.FileId,                            // ID
                                $"{file.LastVersion.Nr}",               // NR    
                                file.LastVersion.Items[0].Ansicht,      // Ansicht
                                file.Filename);                         // Filename
        }

        public static void AddFileInformation(this ArchiveRecord archiveRecord, Verzeichnungseinheit record)
        {
            var source = record.Dateien.Where(f => f.IsOriginal()).ToList();
            if (source.Any() && archiveRecord.Metadata.Files != null)
            {
                foreach (var file in source)
                {
                    var path = file.GetFullPath(Settings.Default.CdwsRoot);
                    Log.Information($"Check file {path}.");
                    if (File.Exists(path))
                    {
                        if (string.IsNullOrEmpty(file.Publikation))
                        {
                            continue;
                        }
                        switch (file.Publikation.ToLower())
                        {
                            case "keinepublikation":
                            case "nichtdefiniert":
                                break;
                            default:
                                var fileInfo = new FileInfo(path);

                                var metadataFile = new ArchiveRecordMetadataFile
                                {
                                    Title = file.Titel,
                                    FileType = file.FileType,
                                    FileName = file.Filename,
                                    FileExtension = file.FileExtension,
                                    FileSize = fileInfo.Length,
                                    Description = file.Bemerkungen,
                                    Kind = file.Art.Item.Bezeichnung,
                                    Publikation = file.Publikation,
                                    SortOrder = (int)file.LastVersion.Nr,  // Value is a decimal and will be truncated
                                    DownloadUrl = file.GetCdwsUrl(Settings.Default.CdwsEndpoint)
                                };
                                Log.Information($"Add file content to Metadata: {file.FileSize} Bytes. Endpoint: {metadataFile.DownloadUrl}");
                                archiveRecord.Metadata.Files.Add(metadataFile);
                                break;
                        }
                    }
                    else
                    {
                        Log.Warning($"File {path} not found.");
                    }
                }
            }
        }
    }

    public partial class Datei
    {
        public VersionType LastVersion => File.Items.OrderBy(c => c.Nr).Last();
        public string FileId => $"{File.ID.Replace($"{OBJ_GUID}-", string.Empty)}";
        public string Filename => $"cdws.{LastVersion.Items[0].Extension}";
    }
}
