using System.Collections.Generic;
using System.Linq;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

namespace CMI.Contract.Common.Compiler
{
    public class IndexCustomScript : IDynamicScript
    {
        public void PostProcessArchiveRecord(ArchiveRecord archiveRecord)
        {
        }

        public void PostProcessElasticArchiveRecord(ElasticArchiveRecord elasticArchiveRecord, ArchiveRecord archiveRecord)
        {
            // Facetten
            // elasticArchiveRecord.Facetten.Text01 = elasticArchiveRecord.Descriptors.GroupBy(d => d.Name)
            if (elasticArchiveRecord.Descriptors.Count > 0 && elasticArchiveRecord.Descriptors.Any(d => d.Thesaurus == "Personenregister"))
            {
                var descriptors = elasticArchiveRecord.Descriptors.Where(d => d.Thesaurus == "Personenregister");
                elasticArchiveRecord.Facetten.Text01 = new List<string>();
                foreach (var descriptor in descriptors)
                {
                    var text = new StringBuilder();
                    text.Append(descriptor.Name);
                    if (descriptor.DateOfBirth != null && (descriptor.DateOfDeath > 0)
                    {
                        text.Append(" (" + descriptor.DateOfBirth.Year + "-" + descriptor.DateOfDeath.Year + ")");
                    }

                    if (!string.IsNullOrEmpty(descriptor.Function))
                    {
                        text.Append(": " + descriptor.Function);
                    }
                    elasticArchiveRecord.Facetten.Text01.Add(text.ToString());
                }
            }
            var sprachen = elasticArchiveRecord.DetailData.FirstOrDefault(d => d.ElementName.Equals("Sprache"));
            if (sprachen != null)
            {
                elasticArchiveRecord.Facetten.Text02 = new List<string>();
                foreach (var sprache in sprachen.TextValues)
                {
                    elasticArchiveRecord.Facetten.Text02.Add(sprache);
                }
            }

            var archivalienart = elasticArchiveRecord.DetailData.FirstOrDefault(d => d.ElementName.Equals("Archivalienart"));
            if (archivalienart != null)
            {
                elasticArchiveRecord.Facetten.Text03 = new List<string>();
                foreach (var archivalie in archivalienart.TextValues)
                {
                    elasticArchiveRecord.Facetten.Text03.Add(archivalie);
                }

                elasticArchiveRecord.Facetten.Text03 = text.ToString();
            }

            if (elasticArchiveRecord.Descriptors.Count > 0 && elasticArchiveRecord.Descriptors.Any(d => d.Thesaurus == "Körperschaftsregister" || d.Thesaurus == "Koerperschaftsregister"))
            {
                var descriptors = elasticArchiveRecord.Descriptors.Where(d => d.Thesaurus == "Körperschaftsregister" || d.Thesaurus == "Koerperschaftsregister");
                elasticArchiveRecord.Facetten.Text04 = new List<string>();
                foreach (var descriptor in descriptors)
                {
                    var text = new StringBuilder();
                    text.Append(descriptor.Name);

                    if (!string.IsNullOrEmpty(descriptor.Function))
                    {
                        text.Append(" : " + descriptor.Function);
                    }
                    elasticArchiveRecord.Facetten.Text04.Add(text.ToString());
                }

            }
        }

    }
}
