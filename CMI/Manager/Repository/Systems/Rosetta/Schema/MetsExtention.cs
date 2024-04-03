using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;

namespace CMI.Manager.Repository.Systems.Rosetta.Schema
{
    public static class MetsExtention
    {
        public static MdSecTypeMdWrapXmlData Wrapper(this Mets mets)
        {
          return mets.DmdSec[0].MdWrap.Item as MdSecTypeMdWrapXmlData;
        }

        public static IDictionary<string, string> GetMetaData(this Mets mets)
        {
            var wrapper = mets.DmdSec[0].MdWrap.Item as MdSecTypeMdWrapXmlData;
            return wrapper?.Any[0].ChildNodes.OfType<XmlElement>()
                .ToDictionary(n => Regex.Replace(n.Name, @"^.+:", string.Empty), n => n.InnerText);
        }

        public static DivType GetTableOfContent(this Mets mets)
        {
            return mets.GetMasterNode().Div.First();
        }

        public static DivType GetMasterNode(this Mets mets)
        {
            DivType master = null;
            Predicate<StructMapType> modifiedMaster = (StructMapType m) =>
            {
                return Regex.IsMatch(m.Div.LABEL.ToUpper(), "MODIFIED[\\s-_]+MASTER");
            };
            Predicate<StructMapType> preservationMaster = (StructMapType m) =>
            {
                return Regex.IsMatch(m.Div.LABEL.ToUpper(), "PRESERVATION[\\s-_]+MASTER");
            };
            // --------------------------------------------------------------------------------------------------
            // Für jede Repräsentation gibt es eine Struct Map. 
            // Repräsentationen können z.B. sein
            // - Preservation Master
            // - Modified Master
            // - Derivative Copy
            // Und für jede Repräsentation kann es eine Logische und eine Physische Struct Map geben.
            // Uns interessiert die Struktur und Dateien des Modified Masters. Ist dieser nicht vorhanden,
            // dann verwenden wir den Preservation Master, der immer vorhanden ist.
            // Ebenso suchen wir zuerst nach den "LOGICAL" StructMaps und erst danach nach den "LOGICAL"
            // --------------------------------------------------------------------------------------------------

            // Gibt es logische Struct Maps?
            var entryStruct = mets.StructMap.Where(s => s.TYPE.Equals("LOGICAL", StringComparison.InvariantCultureIgnoreCase)).ToList();
            
            // Gibt es keine logische Struct Maps (müsste es neu immer geben), kehren wir zur physischen zurück
            if (entryStruct != null && entryStruct.Any())
            {
                // Jetzt holen wir den Modified oder wenn nicht vorhanden den Preservation Master
                master = entryStruct.Find(modifiedMaster)?.Div ?? entryStruct.Find(preservationMaster)?.Div;
            }
            else
            {
                entryStruct = mets.StructMap.Where(s => s.TYPE.Equals("PHYSICAL", StringComparison.InvariantCultureIgnoreCase)).ToList();
                // Jetzt holen wir den Modified oder wenn nicht vorhanden den Preservation Master
                master = entryStruct.Find(modifiedMaster)?.Div ?? entryStruct.Find(preservationMaster)?.Div;
            }

            return master ?? throw new KeyNotFoundException("Master node could not be found");
        }

        public static string GetImportFolderName(this Mets mets)
        {
            var pattern = @"^[^-]+";
            var structMap = mets.StructMap.FirstOrDefault(s => s.TYPE.Equals("LOGICAL", StringComparison.InvariantCultureIgnoreCase)) ??
                            mets.StructMap.FirstOrDefault(s => s.TYPE.Equals("PHYSICAL", StringComparison.InvariantCultureIgnoreCase));
            
            var regex = new Regex(pattern);
            var match = regex.Match(structMap.ID);

            if (match.Success)
            {
                return match.Value;
            }

            Log.Warning("Folder name could not be extracted from METS file");
            return null;
        }
    }
}
