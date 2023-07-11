using System.Collections.Generic;
using System.Linq;
using System.Text;

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
                    if (descriptor.DateOfBirth != null && descriptor.DateOfDeath != null)
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
            }

            if (elasticArchiveRecord.Descriptors.Count > 0)
            {
                if (elasticArchiveRecord.Descriptors.Any(d => d.Thesaurus == "Körperschaftsregister" || d.Thesaurus == "Koerperschaftsregister"))
                {
                    var descriptors = elasticArchiveRecord.Descriptors.Where(d =>
                        d.Thesaurus == "Körperschaftsregister" || d.Thesaurus == "Koerperschaftsregister");
                    elasticArchiveRecord.Facetten.Text04 = new List<string>();
                    foreach (var descriptor in descriptors)
                    {
                        var text = new StringBuilder();
                        text.Append(descriptor.Description);
                        if (!string.IsNullOrEmpty(descriptor.Function))
                        {
                            text.Append(" : " + descriptor.Function);
                        }

                        elasticArchiveRecord.Facetten.Text04.Add(text.ToString());
                    }

                }

                // ID Name erzeugen für Deskriptoren
                var personDescriptors = elasticArchiveRecord.Descriptors.Where(d => d.Thesaurus.ToLower().Equals("personenregister")).OrderBy(d => d.Name);
                int counter = 0;
                foreach (var descriptor in personDescriptors)
                {
                    if (descriptor.DateOfBirth != null)
                    {
                        descriptor.SortingNumber = counter++;
                        string yearOfDeath = descriptor.DateOfDeath != null ? descriptor.DateOfDeath.Year.ToString() : "?";
                        descriptor.IdName = descriptor.Function != string.Empty ? string.Format("{0} ({1}-{2}), {3}", descriptor.Name,
                            descriptor.DateOfBirth.Year.ToString(), yearOfDeath, descriptor.Function) : string.Format("{0} ({1}-{2})", descriptor.Name,
                            descriptor.DateOfBirth.Year.ToString(), yearOfDeath);
                    }
                    else
                    {
                        descriptor.IdName = descriptor.Function != string.Empty ? string.Format("{0}, {1}", descriptor.Name, descriptor.Function) : descriptor.Name;
                    }
                    if (!string.IsNullOrEmpty(descriptor.Source))
                    {
                        descriptor.Source = string.Format("<a href =\"https://d-nb.info/gnd/{0}\" target=\"_blank\"> GND-ID: {1}</a>", descriptor.Source, descriptor.Source);
                    }
                }

                CreateThesaurusDetail(elasticArchiveRecord, ref counter, "koerperschaftsregister");
            }

            if (elasticArchiveRecord.DetailData.Any(d => d.ElementName.StartsWith("Link") || d.ElementName == "URL"))
            {
                var links = elasticArchiveRecord.DetailData.Where(d => d.ElementName.StartsWith("Link") || d.ElementName == "URL");
                foreach (var link in links)
                {
                    var textLink = link.TextValues.First();
                    if (!string.IsNullOrEmpty(textLink))
                    {
                        link.TextValues = new List<string> { string.Format("<a href =\"{0}\" target=\"_blank\">{0}</a>", textLink) };
                    }
                }
            }

        }

        private static void CreateThesaurusDetail(ElasticArchiveRecord elasticArchiveRecord, ref int counter, string typeName, bool withDate = false)
        {
            var descriptors = elasticArchiveRecord.Descriptors.Where(d => d.Thesaurus.ToLower().Equals(typeName)).OrderBy(d => d.Name);

            foreach (var descriptor in descriptors)
            {
                descriptor.SortingNumber = counter++;
                if (withDate && descriptor.DateOfBirth != null)
                {
                    string yearOfDeath = descriptor.DateOfDeath != null ? descriptor.DateOfDeath.Year.ToString() : "?";
                    descriptor.IdName = descriptor.Function != string.Empty ? string.Format("{0} ({1}-{2}), {3}", descriptor.Name,
                        descriptor.DateOfBirth.Year.ToString(), yearOfDeath, descriptor.Function) : string.Format("{0} ({1}-{2})", descriptor.Name,
                        descriptor.DateOfDeath.Year.ToString(), yearOfDeath); descriptor.IdName = descriptor.Function != string.Empty ? string.Format("{0}, {1}", descriptor.Name, descriptor.Function) : descriptor.Name;
                }
                else
                {
                    descriptor.IdName = descriptor.Function != string.Empty ? string.Format("{0}, {1}", descriptor.Name, descriptor.Function) : descriptor.Name;
                }

                descriptor.Source = "";
            }
        }
    }
}
