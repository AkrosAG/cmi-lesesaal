using System.Linq;
using System.Xml.Linq;

namespace CMI.Access.Repository.Systems.Rosetta
{
    internal class EntityExportResult(string xml)
    {
        private readonly XDocument xmlDoc = !string.IsNullOrEmpty(xml) ? XDocument.Parse(xml) : null;

        public string ProcessUrl => xmlDoc?.Descendants("info")
                .FirstOrDefault(e => e.Attribute("desc")?.Value == "process_instance_id_link")?.Value;

        public string ExportPath => xmlDoc?.Descendants("info")
                .FirstOrDefault(e => e.Attribute("desc")?.Value == "full_export_path")?.Value;
    }
}
