using System;
using System.Collections.Generic;
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

        public static string GetFullPath(this Datei file, string root)
        {
            return Path.Combine(root,                                   // CDWS_ROOT
                                $"{file.OBJ_GUID}",                     // OBJ_GUID
                                file.FileId,                            // ID
                                $"{file.LastVersion.Nr}",               // NR    
                                file.LastVersion.Items[0].Ansicht,      // Ansicht
                                file.Filename);                         // Filename
        }

        public static void AddFileContent(this List<ArchiveRecordMetadataFile> list, Verzeichnungseinheit record)
        {
            var source = record.Dateien.Where(f => f.IsOriginal()).ToList();
            if (source.Any())
            {
                foreach (var file in source)
                {
                    var path = file.GetFullPath(Settings.Default.CdwsRoot);
                    Log.Information($"Check file {path}.");
                    if (File.Exists(path))
                    {
                        var bytes = File.ReadAllBytes(path);
                        var metadataFile = new ArchiveRecordMetadataFile
                        {
                            Title = file.Titel,
                            FileType = file.FileType,
                            FileName = file.Filename,
                            FileExtension = file.FileExtension,
                            FileSize = bytes.LongLength,
                            Description = file.Bemerkungen,
                            ContentText = Convert.ToBase64String(bytes),
                            Kind = file.Art.Item.Bezeichnung,
                            SortOrder = ((int)file.LastVersion.Nr)  // Value is a decimal and will be truncated
                        };
                        list.Add(metadataFile);
                        Log.Information($"Added file content to Metadata: {bytes.LongLength} Bytes.");
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
