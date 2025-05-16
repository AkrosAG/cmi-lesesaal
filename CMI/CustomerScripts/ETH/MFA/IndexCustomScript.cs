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
            var sprachen = elasticArchiveRecord.DetailData.FirstOrDefault(d => d.ElementName.Equals("Sprachen"));
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

            var digitalvorhanden = elasticArchiveRecord.DetailData.FirstOrDefault(d => d.ElementName.Equals("LinkAufDigitalesOriginal"));

            elasticArchiveRecord.Facetten.Boolean01 = new List<bool>();
            if (elasticArchiveRecord.Files != null && elasticArchiveRecord.Files.Count > 0)
            {
                elasticArchiveRecord.Facetten.Boolean01.Add(true);
            }
            else
            {
                elasticArchiveRecord.Facetten.Boolean01.Add(false);
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
                        text.Append(descriptor.Name);
                        if (!string.IsNullOrEmpty(descriptor.Function))
                        {
                            text.Append(": " + descriptor.Function);
                        }

                        elasticArchiveRecord.Facetten.Text04.Add(text.ToString());
                    }

                }
                if (elasticArchiveRecord.Descriptors.Any(d => d.Thesaurus == "Ortsregister"))
                {
                    var descriptors = elasticArchiveRecord.Descriptors.Where(d => d.Thesaurus == "Ortsregister");
                    elasticArchiveRecord.Facetten.Text05 = new List<string>();
                    foreach (var descriptor in descriptors)
                    {
                        var text = new StringBuilder();
                        text.Append(descriptor.Name);

                        if (!string.IsNullOrEmpty(descriptor.Function))
                        {
                            text.Append(": " + descriptor.Function);
                        }

                        elasticArchiveRecord.Facetten.Text05.Add(text.ToString());
                    }
                }
                if (elasticArchiveRecord.Descriptors.Any(d => d.Thesaurus == "Werkregister"))
                {
                    var descriptors = elasticArchiveRecord.Descriptors.Where(d => d.Thesaurus == "Werkregister");
                    elasticArchiveRecord.Facetten.Text06 = new List<string>();
                    foreach (var descriptor in descriptors)
                    {
                        var text = new StringBuilder();
                        text.Append(descriptor.Name);

                        if (descriptor.DateOfBirth != null && descriptor.DateOfDeath != null)
                        {
                            text.Append(" (" + descriptor.DateOfBirth.Year + "-" + descriptor.DateOfDeath.Year + ")");
                        }
                        else if (descriptor.DateOfBirth != null)
                        {
                            text.Append(" (" + descriptor.DateOfBirth.Year + ")");
                        }

                        elasticArchiveRecord.Facetten.Text06.Add(text.ToString());
                    }
                }
                if (elasticArchiveRecord.Descriptors.Any(d => d.Thesaurus == "Sachregister"))
                {
                    var descriptors = elasticArchiveRecord.Descriptors.Where(d => d.Thesaurus == "Sachregister");
                    elasticArchiveRecord.Facetten.Text07 = new List<string>();
                    foreach (var descriptor in descriptors)
                    {

                        elasticArchiveRecord.Facetten.Text07.Add(descriptor.Name);
                    }
                }

                // ID Name erzeugen für Deskriptoren
                int counter = 0;
                CreateThesaurusDetail(elasticArchiveRecord, ref counter, "personenregister", true);
                CreateThesaurusDetailWithUmlauts(elasticArchiveRecord, ref counter, "körperschaftsregister", "koerperschaftsregister");
                CreateThesaurusDetail(elasticArchiveRecord, ref counter, "ortsregister");
                CreateThesaurusDetail(elasticArchiveRecord, ref counter, "werkregister", true);
                CreateThesaurusDetail(elasticArchiveRecord, ref counter, "sachregister");
            }

            if (elasticArchiveRecord.DetailData.Any(d => d.ElementName.StartsWith("Link") || d.ElementName == "URL"))
            {
                var links = elasticArchiveRecord.DetailData.Where(d => d.ElementName.StartsWith("Link") || d.ElementName == "URL");
                foreach (var link in links)
                {
                    var textLink = link.TextValues.First();
                    if (string.IsNullOrEmpty(textLink))
                    {
                        continue;
                    }

                    if (!(textLink.StartsWith("https://") || textLink.StartsWith("http://")))
                    {
                        link.TextValues = new List<string> { string.Format("<a href =\"//{0}\" target=\"_blank\">{0}</a>", textLink) };
                    }
                    else
                    {
                        link.TextValues = new List<string> { string.Format("<a href =\"{0}\" target=\"_blank\">{0}</a>", textLink) };
                    }
                }
            }


            if (elasticArchiveRecord.ReferenceCode == "\u200A")
            {
                switch (elasticArchiveRecord.Level.ToLower())
                {
                    case "bestand":
                    case "serie":
                    case "dossier":
                    case "einzelstück":
                    case "einzelstueck":
                        elasticArchiveRecord.ReferenceCode = "[ohne Signatur]";
                        break;
                }
            }
        }

        private static void CreateThesaurusDetailWithUmlauts(ElasticArchiveRecord elasticArchiveRecord, ref int counter, string typeName, string typeNameWithoutUmlauts)
        {
            var descriptors = elasticArchiveRecord.Descriptors.Where(d => d.Thesaurus.ToLower().Equals(typeName)).OrderBy(d => d.Name).ToList();
            if (!string.IsNullOrEmpty(typeNameWithoutUmlauts))
            {
                var descriptorsWithUmlauts = elasticArchiveRecord.Descriptors.Where(d => d.Thesaurus.ToLower().Equals(typeNameWithoutUmlauts))
                    .OrderBy(d => d.Name).ToList();

                descriptors.AddRange(descriptorsWithUmlauts);
            }
            foreach (var descriptor in descriptors)
            {
                descriptor.SortingNumber = counter++;
                if (descriptor.DateOfBirth != null)
                {
                    string yearOfDeath = descriptor.DateOfDeath != null ? descriptor.DateOfDeath.Year.ToString() : "?";
                    descriptor.IdName = descriptor.Function != string.Empty ? string.Format("{0} ({1}-{2}), {3}", descriptor.Name,
                        descriptor.DateOfBirth.Year.ToString(), yearOfDeath, descriptor.Function) : string.Format("{0} ({1}-{2})", descriptor.Name,
                        descriptor.DateOfBirth.Year.ToString(), yearOfDeath);
                }
                else
                {
                    descriptor.IdName = descriptor.Function != string.Empty ? string.Format("{0}, {1}", descriptor.Name, descriptor.Function) : descriptor.Name;
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
                        descriptor.DateOfBirth.Year.ToString(), yearOfDeath); 
                }
                else
                {
                    descriptor.IdName = descriptor.Function != string.Empty ? string.Format("{0}, {1}", descriptor.Name, descriptor.Function) : descriptor.Name;
                }
            }
        }
    }
}
