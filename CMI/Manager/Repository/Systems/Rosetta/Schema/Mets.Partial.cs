using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using Autofac.Features.Metadata;
using MassTransit;
using Serilog;

namespace CMI.Manager.Repository.Systems.Rosetta.Schema
{
    public partial class Mets
    {
        private MdSecTypeMdWrapXmlData Wrapper => DmdSec[0].MdWrap.Item as MdSecTypeMdWrapXmlData;

        public IDictionary<string, string> GetMetaData()
        {
            return Wrapper?.Any[0].ChildNodes.OfType<XmlElement>()
                .ToDictionary(n => Regex.Replace(n.Name, @"^.+:", string.Empty), n => n.InnerText);
        }

        public DivType GetTableOfContent()
        {
            return GetMasterNode().Div.First();
        }

        public DivType GetMasterNode()
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
            var entryStruct = StructMap.Where(s => s.TYPE.Equals("LOGICAL", StringComparison.InvariantCultureIgnoreCase)).ToList();
            
            // Gibt es keine logische Struct Maps (müsste es neu immer geben), kehren wir zur physischen zurück
            if (entryStruct != null && entryStruct.Any())
            {
                // Jetzt holen wir den Modified oder wenn nicht vorhanden den Preservation Master
                master = entryStruct.Find(modifiedMaster)?.Div ?? entryStruct.Find(preservationMaster)?.Div;
            }
            else
            {
                entryStruct = StructMap.Where(s => s.TYPE.Equals("PHYSICAL", StringComparison.InvariantCultureIgnoreCase)).ToList();
                // Jetzt holen wir den Modified oder wenn nicht vorhanden den Preservation Master
                master = entryStruct.Find(modifiedMaster)?.Div ?? entryStruct.Find(preservationMaster)?.Div;
            }

            return master ?? throw new KeyNotFoundException("Master node could not be found");
        }

        public string GetImportFolderName()
        {
            var pattern = @"^[^-]+";
            var structMap = StructMap.FirstOrDefault(s => s.TYPE.Equals("LOGICAL", StringComparison.InvariantCultureIgnoreCase)) ?? 
                            StructMap.FirstOrDefault(s => s.TYPE.Equals("PHYSICAL", StringComparison.InvariantCultureIgnoreCase));
            
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
