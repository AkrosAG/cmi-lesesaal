using System.Linq;
using System.Xml.Linq;

namespace CMI.Access.Repository.Systems.Rosetta
{
    internal class EntityExportResult
    {
        internal EntityExportResult(string xml)
        {
            xmlDoc = !string.IsNullOrEmpty(xml) ? XDocument.Parse(xml) : null;
        }
        private readonly XDocument xmlDoc;

        public string ProcessUrl => xmlDoc?.Descendants("info")
                .FirstOrDefault(e => e.Attribute("desc")?.Value == "process_instance_id_link")?.Value;

        public string ExportPath => xmlDoc?.Descendants("info")
                .FirstOrDefault(e => e.Attribute("desc")?.Value == "full_export_path")?.Value;

        public string ErrorMessage => xmlDoc?.Descendants("errorsExist")
            .FirstOrDefault(e => e.Attribute("error")?.Value == "errorMessage")?.Value;
    }
}
