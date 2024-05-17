using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace CMI.Contract.Common.Compiler
{
    public class HarvestCustomScript : IDynamicScript
    {
        public void PostProcessArchiveRecord(ArchiveRecord archiveRecord)
        {
            CalculateMetadataAccessTokens(archiveRecord);
            CalculatePrimaryDataAccessTokens(archiveRecord);

            var level = GetDefaultElementValue(archiveRecord.Metadata.DetailData, "verzeichnungsstufe"); 
            var benutzbarkeit = GetDefaultElementValue(archiveRecord.Metadata.DetailData, "benutzbarkeit");

            switch (level.ToLower())
            {
                case "klassifikation":
                    archiveRecord.Display.CanBeOrdered = false;
                    break;
                case "bestand":
                case "serie":
                    switch (benutzbarkeit.ToLower())
                    {
                        case "frei einsehbar":
                        case "gesuchspflichtig":
                            archiveRecord.Display.CanBeOrdered = archiveRecord.Metadata.NodeInfo.ChildCount <= 0;
                            break;
                        default:
                            archiveRecord.Display.CanBeOrdered = false;
                            break;
                    }

                    break;
                case "dossier":
                case "einzelstück":
                case "einzelstueck":
                    switch (benutzbarkeit.ToLower())
                    {
                        case "frei einsehbar":
                        case "gesuchspflichtig":
                            archiveRecord.Display.CanBeOrdered = true;
                            break;
                        default:
                            archiveRecord.Display.CanBeOrdered = false;
                            break;
                    }

                    break;
                default:
                    archiveRecord.Display.CanBeOrdered = false;
                    break;
            }

            var dateRange = GetDateRangeValue(archiveRecord.Metadata.DetailData, "entstehungszeitraum");
            var dateRangeText = GetDefaultElementValue(archiveRecord.Metadata.DetailData, "entstehungszeitraum");
            if (dateRange != null)
            {
                // Offene Zeiträume?
                if (dateRangeText.Trim().StartsWith("-"))
                {
                    dateRange.FromDate = dateRange.ToDate.AddYears(-10);
                    dateRange.SearchFromDate = dateRange.FromDate;
                    dateRange.From = dateRange.FromDate.ToString("+yyyyMMdd");
                }

                if (dateRangeText.Trim().EndsWith("-"))
                {
                    dateRange.ToDate = dateRange.FromDate.AddYears(10);
                    dateRange.SearchToDate = dateRange.ToDate;
                    dateRange.To = dateRange.ToDate.ToString("+yyyyMMdd");
                }
            }

            if (archiveRecord.Metadata.DetailData.Any(d => d.ElementName == "CustomFreeTextField04"))
            {
                var pattern = @"\bdps_pid=(?<ie>IE\w*)\b";
                var digitalesOriginal = archiveRecord.Metadata.DetailData.FirstOrDefault(d => 
                    d.ElementName == "CustomFreeTextField04").ElementValue.FirstOrDefault().TextValues.FirstOrDefault();
                if (digitalesOriginal != null && digitalesOriginal.Value != "")
                {
                    var r = Regex.Match(digitalesOriginal.Value, pattern);
                    if (r.Success)
                    {
                        archiveRecord.Metadata.PrimaryDataLink = r.Groups["ie"].Value;
                    }
                }
            }
        }

        public void PostProcessElasticArchiveRecord(ElasticArchiveRecord elasticArchiveRecord, ArchiveRecord archiveRecord)
        {
        }

        private void CalculateMetadataAccessTokens(ArchiveRecord archiveRecord)
        {
            // Regel 1
            var publikation = GetDefaultElementValue(archiveRecord.Metadata.DetailData, "publikation");
            if (string.IsNullOrEmpty(publikation))
            {
                archiveRecord.Security.MetadataAccessToken = new List<string>();
                return;
            }

            switch (publikation.ToLower())
            {
                // Regel 1
                case "keine publikation":
                case "nicht definiert":
                    archiveRecord.Security.MetadataAccessToken = new List<string>();
                    break;
                
                // Regel 2
                case "sofort":
                    archiveRecord.Security.MetadataAccessToken = new List<string>(new[] { "AMA", "AS", "EMA", "Ö1", "Ö2", "Ö3" });
                    break;

                // Regel 3
                case "nach ablauf schutzfrist":
                    if (archiveRecord.Metadata.Usage.ProtectionEndDate.HasValue &&
                        archiveRecord.Metadata.Usage.ProtectionEndDate.Value > DateTime.Today)
                    {
                        archiveRecord.Security.MetadataAccessToken = new List<string>(new[] { "AMA" });
                    }
                    else
                    {
                        archiveRecord.Security.MetadataAccessToken = new List<string>(new[] { "AMA", "AS", "EMA", "Ö1", "Ö2", "Ö3" });
                    }

                    break;
                // Regel 4
                default:
                    archiveRecord.Security.MetadataAccessToken = new List<string>(new[] { "AMA" });
                    break;
            }
        }

        private void CalculatePrimaryDataAccessTokens(ArchiveRecord archiveRecord)
        {
            // Regel 1a
            var benutzbarkeit = GetDefaultElementValue(archiveRecord.Metadata.DetailData, "benutzbarkeit");
            if (string.IsNullOrEmpty(benutzbarkeit))
            {
                archiveRecord.Security.PrimaryDataDownloadAccessToken = new List<string>();
                archiveRecord.Security.PrimaryDataFulltextAccessToken = new List<string>();
                return;
            }
            switch (benutzbarkeit.ToLower())
            {
                case "frei einsehbar":
                    // Regel 2
                    if (archiveRecord.Metadata.Files == null || archiveRecord.Metadata.Files.Count == 0)
                    {
                        archiveRecord.Security.PrimaryDataDownloadAccessToken = new List<string>(new[] { "AMA", "AS", "EMA", "Ö1", "Ö2", "Ö3" });
                        archiveRecord.Security.PrimaryDataFulltextAccessToken = new List<string>(new[] { "AMA", "AS", "EMA", "Ö1", "Ö2", "Ö3" });
                        return;
                    }

                    // Regel 3
                    if (archiveRecord.Metadata.Files != null && archiveRecord.Metadata.Files.Count > 0 && 
                        archiveRecord.Metadata.Files.All(f => f.Publikation.ToLower() == "sofort"))
                    {
                        archiveRecord.Security.PrimaryDataDownloadAccessToken = new List<string>(new[] { "AMA", "AS", "EMA", "Ö1", "Ö2", "Ö3" });
                        archiveRecord.Security.PrimaryDataFulltextAccessToken = new List<string>(new[] { "AMA", "AS", "EMA", "Ö1", "Ö2", "Ö3" });
                    }
                    // Regel 4
                    else
                    {
                        archiveRecord.Security.PrimaryDataDownloadAccessToken = new List<string>(new[] { "AMA" });
                        archiveRecord.Security.PrimaryDataFulltextAccessToken = new List<string>(new[] { "AMA" });
                    }

                    break;
                default:
                    // Zugriff auf Files ist beschränkt
                    archiveRecord.Security.PrimaryDataDownloadAccessToken = new List<string>(new[] { "AMA" });
                    archiveRecord.Security.PrimaryDataFulltextAccessToken = new List<string>(new[] { "AMA" });
                    break;
            }
        }
        
        private string GetDefaultElementValue(List<DataElement> detailData, string fieldName)
        {
            var element = detailData.FirstOrDefault(d => d.ElementName.Equals(fieldName, StringComparison.InvariantCultureIgnoreCase));
            if (element != null)
            {
                var elementValues = element.ElementValue.FirstOrDefault(e => e.Sequence == 0);
                if (elementValues != null)
                {
                    var value = elementValues.TextValues.FirstOrDefault(t => t.IsDefaultLang).Value;
                    if (!string.IsNullOrEmpty(value))
                    {
                        return value;
                    }
                }
            }

            return string.Empty;
        }

        private DateRange GetDateRangeValue(List<DataElement> detailData, string fieldName)
        {
            var element = detailData.FirstOrDefault(d => d.ElementName.Equals(fieldName, StringComparison.InvariantCultureIgnoreCase));
            if (element != null)
            {
                var elementValues = element.ElementValue.FirstOrDefault(e => e.Sequence == 0);
                if (elementValues != null)
                {
                    var value = elementValues.DateRange;
                    return value;
                }
            }

            return null;
        }

    }
}
