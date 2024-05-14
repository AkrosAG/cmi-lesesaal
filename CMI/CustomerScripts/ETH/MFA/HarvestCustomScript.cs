using System;
using System.Collections.Generic;
using System.Linq;

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
        }

        public void PostProcessElasticArchiveRecord(ElasticArchiveRecord elasticArchiveRecord, ArchiveRecord archiveRecord)
        {
        }

        private void CalculateMetadataAccessTokens(ArchiveRecord archiveRecord)
        {
            // Metadata Tokens werden bestimmt durch  Benutzbarkeit
            var publikation = GetDefaultElementValue(archiveRecord.Metadata.DetailData, "publikation");
            if (string.IsNullOrEmpty(publikation))
            {
                archiveRecord.Security.MetadataAccessToken = new List<string>();
                return;
            }

            switch (publikation.ToLower())
            {
                case "keine publikation":
                case "nicht definiert":
                    archiveRecord.Security.MetadataAccessToken = new List<string>();
                    break;

                case "sofort":
                    archiveRecord.Security.MetadataAccessToken = new List<string>(new[] { "AMA", "AS", "EMA", "Ö1", "Ö2", "Ö3" });
                    break;

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
                default:
                    archiveRecord.Security.MetadataAccessToken = new List<string>(new[] { "AMA" });
                    break;
            }
        }

        private void CalculatePrimaryDataAccessTokens(ArchiveRecord archiveRecord)
        {
            // PrimaryData Tokens werden bestimmt durch  Benutzbarkeit
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
                    if (archiveRecord.Metadata.Files != null && archiveRecord.Metadata.Files.Count > 0 && archiveRecord.Metadata.Files.Any(f => f.Publikation.ToLower() != "sofort"))
                    {
                        // Wenn es mehrere Files gibt, und eines davon nicht sofort publiziert ist, dann ist der Zugriff auf die Files beschränkt
                        archiveRecord.Security.PrimaryDataDownloadAccessToken = new List<string>(new[] { "AMA" });
                        archiveRecord.Security.PrimaryDataFulltextAccessToken = new List<string>(new[] { "AMA" });
                    }
                    else
                    {
                        archiveRecord.Security.PrimaryDataDownloadAccessToken = new List<string>(new[] { "AMA", "AS", "EMA", "Ö1", "Ö2", "Ö3" });
                        archiveRecord.Security.PrimaryDataFulltextAccessToken = new List<string>(new[] { "AMA", "AS", "EMA", "Ö1", "Ö2", "Ö3" });
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
