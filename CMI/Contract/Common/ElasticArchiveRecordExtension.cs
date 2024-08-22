using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Resources;
using CMI.Contract.Common.Properties;
using Newtonsoft.Json;
using Serilog;

namespace CMI.Contract.Common
{
    public static class ElasticArchiveRecordExtension
    {
        private static ResourceManager resourceManager;
        private static ResourceManager ResourceManager
        {
            get { return resourceManager ?? (resourceManager = new ResourceManager(typeof(Resources))); }
        }

        public static string GetAuszuhebendeArchiveRecordId(this ElasticArchiveRecord elasticArchiveRecord)
        {
            // Original standard way
            //var dossierId = elasticArchiveRecord.ArchiveplanContext.FirstOrDefault(apc => apc.Level == "Dossier")?.ArchiveRecordId;
            //return string.IsNullOrEmpty(dossierId) ? elasticArchiveRecord.ArchiveRecordId : dossierId;

            // new for ETH: not all 'Einzelstücke' have a dossier
            return elasticArchiveRecord.ArchiveRecordId;
        }
        
        /// <summary>
        /// Translates the field Levels and the customer field "Accessibility according to BGA".
        /// Was made for the task PVW-789
        /// </summary>
        /// <param name="record">the to translate record</param>
        /// <param name="language">language abbreviation e.g. "en"</param>
        public static void Translate(this TreeRecord record, string language)
        {
            try
            {
                var cultureInfo = new CultureInfo(language);

                var level = ResourceManager.GetString(record.Level ?? "", cultureInfo);
                if (!string.IsNullOrEmpty(level))
                {
                    record.Level = level;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unexpected error while translating the record");
                throw;
            }
        }

        /// <summary>
        /// Translates the field Levels and the customer field "Accessibility according to BGA".
        /// Was made for the task PVW-789
        /// </summary>
        /// <param name="record">the to translate record</param>
        /// <param name="language">language abbreviation e.g. "en"</param>
        public static void Translate(this SearchRecord record, string language)
        {
            try
            {
                var cultureInfo = new CultureInfo(language);

                var level = ResourceManager.GetString(record.Level ?? "", cultureInfo);
                if (!string.IsNullOrEmpty(level))
                {
                    record.Level = level;
                }
                record.Permission = ResourceManager.GetString(record.Permission ?? "", cultureInfo);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unexpected error while translating the record");
                throw;
            }
        }

        public static string GetSchutzfristenVerzeichnung(this ElasticArchiveRecord entity)
        {
            return
                $"SF-Kat: {entity.ProtectionCategory} / SF-Dauer: {entity.ProtectionDuration} / SF-Ende: {entity.ProtectionEndDate?.Date.ToString("dd.MM.yyy") ?? "-"}"
                    .Trim();
        }

    }
}