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

        public static string GetSchutzfristenVerzeichnung(this ElasticArchiveRecord entity)
        {
            var anhang3 = entity.HasCustomProperty("Anhang3") && entity.CustomFields.anhang3
                ? "/ Anhang 3"
                : "";

            var katAutomatisierung = entity.HasCustomProperty("KategorieDia") && entity.CustomFields.kategorieDia == 2
                ? "/ Schutzfristverzeichnung validiert"
                : "";

            var optional = $"{anhang3} {katAutomatisierung}".Trim();
            return
                $"SF-Kat: {entity.ProtectionCategory} / SF-Dauer: {entity.ProtectionDuration} / SF-Ende: {entity.ProtectionEndDate?.Date.ToString("dd.MM.yyy") ?? "-"} {optional}"
                    .Trim();
        }

        public static T GetCustomValueOrDefault<T>(this ElasticArchiveRecord entity, string key)
        {
            if (!entity.HasCustomProperty(key))
            {
                return default;
            }

            var kv = ((IDictionary<string, object>) entity.CustomFields).FirstOrDefault(k =>
                k.Key.Equals(key, StringComparison.InvariantCultureIgnoreCase));
            return (T) kv.Value;
        }

        public static bool HasCustomProperty(this ElasticArchiveRecord entity, string key)
        {
            // ignore case, because the customfields are lowercamelcase
            return entity?.CustomFields != null &&
                   ((IDictionary<string, object>) entity.CustomFields).Any(k => k.Key.Equals(key, StringComparison.InvariantCultureIgnoreCase));
        }

        public static string Aktenzeichen(this ElasticArchiveRecord record)
        {
            Log.Verbose("Getting property aktenzeichen.");
            if (record.HasCustomProperty("aktenzeichen"))
            {
               
                if (record.CustomFields.aktenzeichen is string)
                {
                    Log.Verbose("Property aktenzeichen: {aktenzeichen}", record.CustomFields.aktenzeichen);
                    return record.CustomFields.aktenzeichen;
                }

                if (record.CustomFields.aktenzeichen is List<object>)
                {
                    var aktenzeichen = string.Join(", ", record.CustomFields.aktenzeichen);
                    Log.Verbose("Property aktenzeichen: {aktenzeichen}", aktenzeichen);
                    return aktenzeichen;
                }
            }

            return null;
        }

        public static string Zusatzmerkmal(this ElasticArchiveRecord record)
        {
            Log.Verbose("Getting property zusatzkomponenteZac1.");
            if (record.HasCustomProperty("zusatzkomponenteZac1"))
            {
                
                if (record.CustomFields.zusatzkomponenteZac1 is string)
                {
                    Log.Verbose("Property zusatzkomponenteZac1: {zusatzkomponenteZac1}",record.CustomFields.zusatzkomponenteZac1);
                    return record.CustomFields.zusatzkomponenteZac1;
                }

                if (record.CustomFields.zusatzkomponenteZac1 is List<object>)
                {
                    var zusatzkomponenteZac1 = string.Join(", ", record.CustomFields.zusatzkomponenteZac1);
                    Log.Verbose("Property zusatzkomponenteZac1: {zusatzkomponenteZac1}", zusatzkomponenteZac1);
                    return zusatzkomponenteZac1;
                }
            }

            return null;
        }

        public static string Benutzbarkeit(this ElasticArchiveRecord record)
        {
            Log.Verbose("Getting property Benutzbarkeit.");
            if (record.HasCustomProperty("benutzbarkeit"))
            {
                if (record.CustomFields.benutzbarkeit is string)
                {
                    Log.Verbose("Property Benutzbarkeit: {benutzbarkeit}", record.CustomFields.benutzbarkeit);
                    return record.CustomFields.benutzbarkeit;
                }
                if (record.CustomFields.benutzbarkeit is List<object>)
                {
                    var benutzbarkeit= string.Join(", ", record.CustomFields.benutzbarkeit);
                    Log.Verbose("Property Benutzbarkeit: {benutzbarkeit}", benutzbarkeit);
                    return benutzbarkeit;
                }
            }

            return null;
        }

        public static string Form(this ElasticArchiveRecord record)
        {
            Log.Verbose("Getting property form.");
            if (record.HasCustomProperty("form"))
            {
                Log.Verbose("Property form: {form}", JsonConvert.SerializeObject(record.CustomFields.form));
                if (record.CustomFields.form is string)
                {
                    Log.Verbose("Property form: {form}",  record.CustomFields.form);
                    return record.CustomFields.form;
                }

                if (record.CustomFields.form is List<object>)
                {
                    var form = string.Join(", ", record.CustomFields.form);
                    Log.Verbose("Property form: {form}", form);
                    return form;
                }
            }

            return null;
        }

        public static string EntstehungszeitraumAnmerkung(this ElasticArchiveRecord record)
        {
            Log.Verbose("Getting property entstehungszeitraumAnmerkung.");
            if (record.HasCustomProperty("entstehungszeitraumAnmerkung"))
            {
                if (record.CustomFields.entstehungszeitraumAnmerkung is string)
                {
                    Log.Verbose("Property entstehungszeitraumAnmerkung: {entstehungszeitraumAnmerkung}",record.CustomFields.entstehungszeitraumAnmerkung);
                    return record.CustomFields.entstehungszeitraumAnmerkung;
                }

                if (record.CustomFields.entstehungszeitraumAnmerkung is List<object>)
                {
                    var entstehungszeitraumAnmerkung = string.Join(", ", record.CustomFields.entstehungszeitraumAnmerkung);
                    Log.Verbose("Property entstehungszeitraumAnmerkung: {entstehungszeitraumAnmerkung}", entstehungszeitraumAnmerkung);
                    return entstehungszeitraumAnmerkung;
                }
            }

            return null;
        }

        public static string Verwertungsrecht(this ElasticArchiveRecord record)
        {
            Log.Verbose("Getting property Verwertungsrecht.");
            if (record.HasCustomProperty("verwertungsrecht"))
            {
                if (record.CustomFields.verwertungsrecht is string)
                {
                    Log.Verbose("Property verwertungsrecht: {verwertungsrecht}", record.CustomFields.verwertungsrecht);
                    return record.CustomFields.verwertungsrecht;
                }

                if (record.CustomFields.verwertungsrecht is List<object>)
                {
                    var verwertungsrecht = string.Join(", ", record.CustomFields.verwertungsrecht);
                    Log.Verbose("Property verwertungsrecht: {verwertungsrecht}", verwertungsrecht);
                    return verwertungsrecht;
                }
            }

            return null;
        }

        public static string ZuständigeStelle(this ElasticArchiveRecord record)
        {
            Log.Verbose("Getting property zuständigeStelle.");
            if (record.HasCustomProperty("zuständigeStelle"))
            {
                if (record.CustomFields.zuständigeStelle is string)
                {
                    Log.Verbose("Property zuständigeStelle: {zuständigeStelle}", JsonConvert.SerializeObject(record.CustomFields.zuständigeStelle));
                    return record.CustomFields.zuständigeStelle;
                }

                if (record.CustomFields.zuständigeStelle is List<object>)
                {
                    var zuständigeStelle = string.Join(", ", record.CustomFields.zuständigeStelle);
                    Log.Verbose("Property zuständigeStelle: {zuständigeStelle}", zuständigeStelle);
                    return zuständigeStelle;
                }
            }

            return null;
        }

        public static string ZusätzlicheInformationen(this ElasticArchiveRecord record)
        {
            Log.Verbose("Getting property bemerkungZurVe.");
            if (record.HasCustomProperty("bemerkungZurVe"))
            {
                Log.Verbose("Property bemerkungZurVe: {bemerkungZurVe}", record.CustomFields.bemerkungZurVe);
                if (record.CustomFields.bemerkungZurVe is string)
                {
                    return record.CustomFields.bemerkungZurVe;
                }

                if (record.CustomFields.bemerkungZurVe is List<object>)
                {
                    var bemerkungZurVe = string.Join(", ", record.CustomFields.bemerkungZurVe);
                    Log.Verbose("Property bemerkungZurVe: {bemerkungZurVe}", bemerkungZurVe);
                    return bemerkungZurVe;
                }
            }

            return null;
        }

        public static string Land(this ElasticArchiveRecord record)
        {
            Log.Verbose("Getting property land.");
            if (record.HasCustomProperty("land"))
            {
                if (record.CustomFields.land is string)
                {
                    Log.Verbose("Property land: {land}", record.CustomFields.land);
                    return record.CustomFields.land;
                }

                if (record.CustomFields.land is List<object>)
                {
                    var land = string.Join(", ", record.CustomFields.land);
                    Log.Verbose("Property land: {land}", land);
                    return land;
                }
            }

            return null;
        }

        public static string FrüheresAktenzeichen(this ElasticArchiveRecord record)
        {
            Log.Verbose("Getting property früheresAktenzeichen.");
            if (record.HasCustomProperty("früheresAktenzeichen"))
            {
                if (record.CustomFields.früheresAktenzeichen is string)
                {
                    Log.Verbose("Property früheresAktenzeichen: {früheresAktenzeichen}", record.CustomFields.früheresAktenzeichen);
                    return record.CustomFields.früheresAktenzeichen;
                }

                if (record.CustomFields.früheresAktenzeichen is List<object>)
                {
                    var früheresAktenzeichen = string.Join(", ", record.CustomFields.früheresAktenzeichen);
                    Log.Verbose("Property früheresAktenzeichen: {früheresAktenzeichen}", früheresAktenzeichen);
                    return früheresAktenzeichen;
                }
            }

            return null;
        }

        public static string Thema(this ElasticArchiveRecord record)
        {
            Log.Verbose("Getting property thema.");
            if (record.HasCustomProperty("thema"))
            {
                if (record.CustomFields.thema is string)
                {
                    Log.Verbose("Property thema: {thema}",  record.CustomFields.thema);
                    return record.CustomFields.thema;
                }

                if (record.CustomFields.thema is List<object>)
                {
                    var thema = string.Join(", ", record.CustomFields.thema);
                    Log.Verbose("Property thema: {thema}", thema);
                    return thema;
                }
            }

            return null;
        }

        public static string Format(this ElasticArchiveRecord record)
        {
            Log.Verbose("Getting property format.");
            if (record.HasCustomProperty("format"))
            {
                if (record.CustomFields.format is string)
                {
                    Log.Verbose("Property format: {format}", record.CustomFields.format);
                    return record.CustomFields.format;
                }

                if (record.CustomFields.format is List<object>)
                {
                    var format = string.Join(", ", record.CustomFields.format);
                    Log.Verbose("Property format: {format}", format);
                    return format;
                }
            }

            return null;
        }

        public static string Urheber(this ElasticArchiveRecord record)
        {
            Log.Verbose("Getting property urheber.");
            if (record.HasCustomProperty("urheber"))
            {
                if (record.CustomFields.urheber is string)
                {
                    Log.Verbose("Property urheber: {urheber}",  record.CustomFields.urheber);
                    return record.CustomFields.urheber;
                }

                if (record.CustomFields.urheber is List<object>)
                {
                    var urheber = string.Join(", ", record.CustomFields.urheber);
                    Log.Verbose("Property urheber: {urheber}", urheber);
                    return urheber;
                }
            }

            return null;
        }

        public static string Verleger(this ElasticArchiveRecord record)
        {
            Log.Verbose("Getting property verleger.");
            if (record.HasCustomProperty("verleger"))
            {
                if (record.CustomFields.verleger is string)
                {
                    Log.Verbose("Property verleger: {verleger}", record.CustomFields.verleger);
                    return record.CustomFields.verleger;
                }

                if (record.CustomFields.verleger is List<object>)
                {
                    var verleger = string.Join(", ", record.CustomFields.verleger);
                    Log.Verbose("Property verleger: {verleger}", verleger);
                    return verleger;
                }
            }

            return null;
        }

        public static string Abdeckung(this ElasticArchiveRecord record)
        {
            Log.Verbose("Getting property abdeckung.");
            if (record.HasCustomProperty("abdeckung"))
            {
                if (record.CustomFields.abdeckung is string)
                {
                    Log.Verbose("Property abdeckung: {abdeckung}", record.CustomFields.abdeckung);
                    return record.CustomFields.abdeckung;
                }

                if (record.CustomFields.abdeckung is List<object>)
                {
                    var abdeckung = string.Join(", ", record.CustomFields.abdeckung);
                    Log.Verbose("Property abdeckung: {abdeckung}", abdeckung);
                    return abdeckung;
                }
            }

            return null;
        }

        public static string Ablieferung(this ElasticArchiveRecord record)
        {
            Log.Verbose("Getting property veAblieferungLink.");
            if (record.HasCustomProperty("veAblieferungLink"))
            {
                var veAblieferungLink = record.CustomFields.veAblieferungLink;

                if (veAblieferungLink is List<object>)
                {
                    var ablieferung = new List<string>();
                    foreach (var elasticEntityLink in veAblieferungLink)
                    {
                        if (HasProperty(elasticEntityLink, "value"))
                        {
                            ablieferung.Add(elasticEntityLink.value);
                        }
                    }

                    Log.Verbose("Property veAblieferungLink: {veAblieferungLink}", string.Join(", ", ablieferung));

                    return string.Join(", ", ablieferung);
                }

                if (HasProperty(veAblieferungLink, "value"))
                {
                    Log.Verbose("Property veAblieferungLink: {veAblieferungLink}", veAblieferungLink.value);
                    return veAblieferungLink.value;
                }
            }

            return null;
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
                    searchRecord.TranslateCustomFieldZugaenglichkeitGemässBga(cultureInfo);
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

        private static void TranslateCustomFieldZugaenglichkeitGemässBga(this SearchRecord record, CultureInfo cultureInfo)
        {
            dynamic customFields = record.CustomFields;
            var fields = (IDictionary<string, object>)customFields;

            if (fields.ContainsKey("zugänglichkeitGemässBga"))
            {
                var value = fields["zugänglichkeitGemässBga"].ToString();
                fields.Remove("zugänglichkeitGemässBga");
                var result = ResourceManager.GetString(value, cultureInfo); 
                ((IDictionary<string, object>)customFields).Add("zugänglichkeitGemässBga", result != string.Empty ? result : value);
            }
        }

        private static bool HasProperty(dynamic expandoObject, string propertyName)
        {
            return ((IDictionary<string, object>) expandoObject).ContainsKey(propertyName);
        }
    }
}