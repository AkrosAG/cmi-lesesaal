using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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

        public static async Task AddFileContentAsync(this ArchiveRecord archiveRecord, Verzeichnungseinheit record)
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
                        var bytes = await ReadAllBytesAsync(path);
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
                            Publikation= file.Publikation,
                            SortOrder = ((int)file.LastVersion.Nr)  // Value is a decimal and will be truncated
                        };
                        archiveRecord.Metadata.Files.Add(metadataFile);
                        Log.Information($"Added file content to Metadata: {bytes.LongLength} Bytes.");
                    }
                }
            }
        }

        private static async Task<byte[]> ReadAllBytesAsync(string path)
        {
            using var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 4096, useAsync: true);
            var buffer = new byte[fileStream.Length];
            await fileStream.ReadAsync(buffer, 0, buffer.Length);
            return buffer;
        }
    }

    public partial class Datei
    {
        public VersionType LastVersion => File.Items.OrderBy(c => c.Nr).Last();
        public string FileId => $"{File.ID.Replace($"{OBJ_GUID}-", string.Empty)}";
        public string Filename => $"cdws.{LastVersion.Items[0].Extension}";
    }
}
