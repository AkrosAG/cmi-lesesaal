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
            return elasticArchiveRecord.ArchiveplanContext.FirstOrDefault(apc => apc.Level == "Dossier")?.ArchiveRecordId;
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
                if (record is SearchRecord searchRecord)
                {
                    searchRecord.Permission = ResourceManager.GetString(searchRecord.Permission ?? "", cultureInfo);
                }

                var level = ResourceManager.GetString(record.Level ?? "", cultureInfo) ;
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

        private static bool HasProperty(dynamic expandoObject, string propertyName)
        {
            return ((IDictionary<string, object>) expandoObject).ContainsKey(propertyName);
        }
    }
}