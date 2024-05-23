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

        private void CalculateCanBeOrderWitnRequest(ArchiveRecord archiveRecord)
        {
            var level = GetDefaultElementValue(archiveRecord.Metadata.DetailData, "verzeichnungsstufe");
            var benutzbarkeit = GetDefaultElementValue(archiveRecord.Metadata.DetailData, "benutzbarkeit");

            switch (benutzbarkeit.ToLower())
            {
                case "frei einsehbar":
                    switch (level.ToLower())
                    {

                        case "bestand":
                        case "dossier":
                        case "einzelstück":
                        case "einzelstueck":
                            archiveRecord.Display.CanBeOrdered = archiveRecord.Metadata.NodeInfo.ChildCount <= 0;
                            archiveRecord.Display.NeedsOrderRequest = false;
                            break;
                        case "serie":
                            archiveRecord.Display.CanBeOrdered = archiveRecord.Metadata.NodeInfo.ChildCount <= 0;
                            archiveRecord.Display.NeedsOrderRequest = true;
                            break;
                        default:
                            archiveRecord.Display.CanBeOrdered = false;
                            archiveRecord.Display.NeedsOrderRequest = true;
                            break;
                    }

                    break;

                case "gesuchspflichtig":
                    switch (level.ToLower())
                    {
                        case "dossier":
                        case "einzelstück":
                        case "einzelstueck":
                        case "bestand":
                        case "serie":
                            archiveRecord.Display.CanBeOrdered = archiveRecord.Metadata.NodeInfo.ChildCount <= 0;
                            archiveRecord.Display.NeedsOrderRequest = true;
                            break;
                        default:
                            archiveRecord.Display.CanBeOrdered = false;
                            archiveRecord.Display.NeedsOrderRequest = true;
                            break;
                    }
                    break;
                default:
                    archiveRecord.Display.CanBeOrdered = false;
                    archiveRecord.Display.NeedsOrderRequest = true;
                    break;
            }
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
