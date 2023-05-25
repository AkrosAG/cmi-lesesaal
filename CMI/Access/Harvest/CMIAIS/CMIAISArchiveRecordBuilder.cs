using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using CMI.Access.Harvest.CMIAIS.Mapping;
using CMI.Contract.Common;
using Serilog;

namespace CMI.Access.Harvest.CMIAIS
{
    public class CMIAISArchiveRecordBuilder : IArchiveRecordBuilder
    {
        private readonly IAISSpecificRecordAccess<Verzeichnungseinheit> aisSpecificRecordAccess;
        private readonly LanguageSettings languageSettings;
        private readonly IArchiveRecordProcessHandler processHandler;

        public CMIAISArchiveRecordBuilder(IAISSpecificRecordAccess<Verzeichnungseinheit> aisSpecificRecordAccess, LanguageSettings languageSettings, IArchiveRecordProcessHandler processHandler)
        {
   
            this.aisSpecificRecordAccess = aisSpecificRecordAccess;
            this.languageSettings = languageSettings;
            this.processHandler = processHandler;
        }

        public async Task<ArchiveRecord> Build(string archiveRecordId)
        {
            try
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();
                var cmiRecord = await aisSpecificRecordAccess.GetAisSpecificRecord(archiveRecordId);
                Log.Verbose($"Took {stopwatch.ElapsedMilliseconds} ms to fetch detail record from CDWS with id {archiveRecordId}");

                var archiveRecordBuilder = new ArchiveRecordMapperBuilder(cmiRecord, languageSettings, aisSpecificRecordAccess);

                var metaDataBuilder = await archiveRecordBuilder
                        .AddMedataData()
                        .WithUsageInfos()
                        .WithNodeInfos();

                AddDetailData(metaDataBuilder);

                var record = archiveRecordBuilder.Build();

                record.Display = await GetDisplaySection(cmiRecord, record);

                await processHandler.PostProcessArchiveRecord(record);
                Log.Information($"Took {stopwatch.ElapsedMilliseconds} ms to build the record with id {archiveRecordId}");
                stopwatch.Stop();

                return record;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unexpected error while building the record for id {id}", archiveRecordId);
                return null;
            }
        }

        private static void AddDetailData(MetaDataBuilder metaDataBuilder)
        {
            metaDataBuilder
                .AddDetailData()
                .WithMappings()
                .From(nameof(Verzeichnungseinheit.TypeName), vz => vz.TypeName)
                .From(nameof(Verzeichnungseinheit.DisplayName), vz => vz.DisplayName)
                .From(nameof(Verzeichnungseinheit.Signatur), vz => vz.Signatur)
                .From(nameof(Verzeichnungseinheit.AlteSignatur), vz => vz.AlteSignatur)
                .From(nameof(Verzeichnungseinheit.Verwaltungssignatur), vz => vz.Verwaltungssignatur)
                .From(nameof(Verzeichnungseinheit.ID), vz => vz.ID)
                .From(nameof(Verzeichnungseinheit.PID), vz => vz.PID)
                .From(nameof(Verzeichnungseinheit.Titel), vz => vz.Titel)
                .From(nameof(Verzeichnungseinheit.Entstehungszeitraum), vz => vz.Entstehungszeitraum)
                .From(nameof(Verzeichnungseinheit.Bezugszeitraum), vz => vz.Bezugszeitraum)
                .From(nameof(Verzeichnungseinheit.Verzeichnungsstufe), vz => vz.Verzeichnungsstufe)
                .From(nameof(Verzeichnungseinheit.Verwaltungsgeschichte), vz => vz.Verwaltungsgeschichte)
                .From(nameof(Verzeichnungseinheit.Bestandsgeschichte), vz => vz.Bestandsgeschichte)
                .From(nameof(Verzeichnungseinheit.AbgebendeStelle), vz => vz.AbgebendeStelle)
                .From(nameof(Verzeichnungseinheit.FormInhalt), vz => vz.FormInhalt)
                .From(nameof(Verzeichnungseinheit.BewertungKassation), vz => vz.BewertungKassation)
                .From(nameof(Verzeichnungseinheit.OrdnungKlassifikation), vz => vz.OrdnungKlassifikation)
                .From(nameof(Verzeichnungseinheit.Zugangsbestimmungen), vz => vz.Zugangsbestimmungen)
                .From(nameof(Verzeichnungseinheit.Reproduktionsbestimmungen), vz => vz.Reproduktionsbestimmungen)
                .From(nameof(Verzeichnungseinheit.PhysischeBeschaffenheit), vz => vz.PhysischeBeschaffenheit)
                .From(nameof(Verzeichnungseinheit.Findmittel), vz => vz.Findmittel)
                .From(nameof(Verzeichnungseinheit.KopienReproduktionen), vz => vz.KopienReproduktionen)
                .From(nameof(Verzeichnungseinheit.VerwandtesMaterial), vz => vz.VerwandtesMaterial)
                .From(nameof(Verzeichnungseinheit.Veroeffentlichungen), vz => vz.Veroeffentlichungen)
                .From(nameof(Verzeichnungseinheit.AllgemeineAnmerkungen), vz => vz.AllgemeineAnmerkungen)
                .From(nameof(Verzeichnungseinheit.Verzeichnungsgrundsaetze), vz => vz.Verzeichnungsgrundsaetze)
                .From(nameof(Verzeichnungseinheit.BemerkungEntstehungszeitraum), vz => vz.BemerkungEntstehungszeitraum)
                .From(nameof(Verzeichnungseinheit.Benutzbarkeit), vz => vz.Benutzbarkeit)
                .From(nameof(Verzeichnungseinheit.Publikation), vz => vz.Publikation)
                .From(nameof(Verzeichnungseinheit.Verwertungsrecht), vz => vz.Verwertungsrecht)
                .From(nameof(Verzeichnungseinheit.AblaufVerwertungsrecht), vz => vz.AblaufVerwertungsrecht)
                .From(nameof(Verzeichnungseinheit.UrheberBekannt), vz => vz.UrheberBekannt)
                .From(nameof(Verzeichnungseinheit.TodesdatumUrheber), vz => vz.TodesdatumUrheber)
                .From(nameof(Verzeichnungseinheit.Urheber), vz => vz.Urheber)
                .From(nameof(Verzeichnungseinheit.AufbewahrungsortOriginale), vz => vz.AufbewahrungsortOriginale)
                .From(nameof(Verzeichnungseinheit.Verzeichnungsstatus), vz => vz.Verzeichnungsstatus)
                .From(nameof(Verzeichnungseinheit.Bearbeiterzeitraum), vz => vz.Bearbeiterzeitraum)
                .From(nameof(Verzeichnungseinheit.InterneAnmerkungen), vz => vz.InterneAnmerkungen)
                .From(nameof(Verzeichnungseinheit.BemerkungDatierung), vz => vz.BemerkungDatierung)
                .From(nameof(Verzeichnungseinheit.BemerkungProvenienz), vz => vz.BemerkungProvenienz)
                .From(nameof(Verzeichnungseinheit.DateiVorhanden), vz => vz.DateiVorhanden)
                .From(nameof(Verzeichnungseinheit.DigitalVorhanden), vz => vz.DigitalVorhanden)
                .From(nameof(Verzeichnungseinheit.Tektonikpfad), vz => vz.Tektonikpfad)
                .From(nameof(Verzeichnungseinheit.Verfuegbarkeit), vz => vz?.Verfuegbarkeit?.Item?.Bezeichnung)

                .From(nameof(Verzeichnungseinheit.CustomBemerkungSprache), vz => vz.CustomBemerkungSprache)
                .From(nameof(Verzeichnungseinheit.CustomBemerkungStandort), vz => vz.CustomBemerkungStandort)
                .From(nameof(Verzeichnungseinheit.CustomCustomTextField), vz => vz.CustomCustomTextField)
                .From(nameof(Verzeichnungseinheit.CustomKuerzel), vz => vz.CustomKuerzel)
                .From(nameof(Verzeichnungseinheit.CustomLinkAufDigitalesOriginal), vz => vz.CustomLinkAufDigitalesOriginal)
                .From(nameof(Verzeichnungseinheit.CustomLinkZuDigitalisat), vz => vz.CustomLinkZuDigitalisat)
                .From(nameof(Verzeichnungseinheit.CustomLinkZuPrimaerdaten), vz => vz.CustomLinkZuPrimaerdaten)
                .From(nameof(Verzeichnungseinheit.CustomSchadenserhebung), vz => vz.CustomSchadenserhebung)
                .From(nameof(Verzeichnungseinheit.CustomURL), vz => vz.CustomURL)
                .From(nameof(Verzeichnungseinheit.CustomZustandskategorie), vz => vz.CustomZustandskategorie.Item.CustomZustandskategorie1)
                .From(nameof(Verzeichnungseinheit.CustomErwerbsarten), vz => vz?.CustomErwerbsarten?.Item.Bezeichnung)
                .From(nameof(Verzeichnungseinheit.CustomLizenz), vz => vz?.CustomLizenz?.Item.Bezeichnung)

                .From(nameof(Verzeichnungseinheit.CustomFreeBool01Field), vz => vz.CustomFreeBool01Field)
                .From(nameof(Verzeichnungseinheit.CustomFreeBool02Field), vz => vz.CustomFreeBool02Field)

                .From(nameof(Verzeichnungseinheit.CustomFreeDate01Field), vz => vz.CustomFreeDate01Field)
                .From(nameof(Verzeichnungseinheit.CustomFreeDate02Field), vz => vz.CustomFreeDate02Field)

                .From(nameof(Verzeichnungseinheit.CustomFreeText01Field), vz => vz.CustomFreeText01Field)
                .From(nameof(Verzeichnungseinheit.CustomFreeText02Field), vz => vz.CustomFreeText02Field)
                .From(nameof(Verzeichnungseinheit.CustomFreeText03Field), vz => vz.CustomFreeText03Field)
                .From(nameof(Verzeichnungseinheit.CustomFreeText04Field), vz => vz.CustomFreeText04Field)
                .From(nameof(Verzeichnungseinheit.CustomFreeText05Field), vz => vz.CustomFreeText05Field)

                .FromCollection(nameof(Verzeichnungseinheit.Sprache), vz => vz?.Sprache?.Select(s => s.Bezeichnung))
                .FromCollection(nameof(Verzeichnungseinheit.Ueberlieferungsform), vz => vz?.Ueberlieferungsform?.Select(u => u.Bezeichnung))
                .FromCollection(nameof(Verzeichnungseinheit.Provenienz), vz => vz?.Provenienz?.Select(a => a.OffiziellerName))
                .FromCollection(nameof(Verzeichnungseinheit.Archivalienart), vz => vz?.Archivalienart?.Select(a => a.Bezeichnung))
                .FromCollection(nameof(Verzeichnungseinheit.Standort), vz => vz?.Standort?.Select(a => a.ToString()))
                .FromCollection(nameof(Verzeichnungseinheit.Umfang), vz => vz?.Umfang?.Select(u => $"{u.Wert} {u.Masseinheit}"));
        }
        private async Task<ArchiveRecordDisplay> GetDisplaySection(Verzeichnungseinheit cmiRecord, ArchiveRecord archiveRecord)
        {
            var display = new ArchiveRecordDisplay
            {
                InternalDisplayTemplateName = $"{cmiRecord.TypeName}_Intern",
                ExternalDisplayTemplateName = $"{cmiRecord.TypeName}_Extern",
                ContainsImages = archiveRecord.Metadata.DetailData.Any(d => d.ElementType == DataElementElementType.image),
                ContainsMedia = archiveRecord.Metadata.DetailData.Any(d => d.ElementType == DataElementElementType.media),
                CanBeOrdered = false, // Wird im Custom Script gesetzt/überschrieben
            };

            await CalculateTreeContext(display, cmiRecord, archiveRecord);
            return display;
        }

        private async Task CalculateTreeContext(ArchiveRecordDisplay display, Verzeichnungseinheit cmiRecord, ArchiveRecord archiveRecord)
        {
            if (string.IsNullOrWhiteSpace(archiveRecord.Metadata.NodeInfo.ParentArchiveRecordId))
                return;

            var parentRecordId = archiveRecord.Metadata.NodeInfo.ParentArchiveRecordId;
            var parentRecord = await aisSpecificRecordAccess.GetAisSpecificRecord(parentRecordId);
            if(parentRecord == null)
            {
                Log.Warning("ParentArchiveRecordId {0} konnte nict geladen werden", parentRecordId);
                return;
            }
            
            display.ParentArchiveRecordId = parentRecord.OBJ_GUID;            
            display.FirstChildArchiveRecordId = cmiRecord.Children.FirstOrDefault()?.OBJ_GUID;

            if (parentRecord.Children.Any())
            {
                var indexOfMe = parentRecord.Children.ToList().FindIndex(c => c.OBJ_GUID == cmiRecord.OBJ_GUID);
                var indexNext = indexOfMe + 1;
                var indexPrev = indexOfMe - 1;

                if (parentRecord.Children.Count > indexNext)
                {
                    display.NextArchiveRecordId = parentRecord.Children[indexNext].OBJ_GUID;
                }

                if (indexPrev > 0)
                {
                    display.PreviousArchiveRecordId = parentRecord.Children[indexPrev].OBJ_GUID;
                }
            }

            display.ArchiveplanContext = new System.Collections.Generic.List<ArchiveplanContextItem>();

            foreach(var ancestor in cmiRecord.Ancestors.OrderByDescending(b => b.Depth))
            {
                // Mandant is never published, so we skip it
                if (ancestor.TypeKey.Equals("Mandant", StringComparison.InvariantCultureIgnoreCase))
                {
                    continue;
                }

                var ancestorRecord = await aisSpecificRecordAccess.GetAisSpecificRecord(ancestor.OBJ_GUID);
                ArchiveplanContextItem contextItem;

                if (ancestorRecord != null)
                {
                    contextItem = new ArchiveplanContextItem
                    {
                        ArchiveRecordId = ancestorRecord.OBJ_GUID,
                        Level = ancestorRecord.Verzeichnungsstufe,
                        Title = ancestorRecord.Titel,
                        DateRangeText = ancestorRecord.Entstehungszeitraum?.Text,
                        RefCode = ancestorRecord.Signatur,
                        IconId = (int) ancestor.TypeId,
                    };
                }
                else
                {
                     // Fallback - falls es eine Referenz auf eine VE gibt, die eventuell nicht publiziert wurde.
                    contextItem = new ArchiveplanContextItem
                    {
                        ArchiveRecordId = "-1",
                        Level = "?",
                        DateRangeText = "?",
                        IconId = -1,
                        RefCode = "?",
                        Title = "?"
                    };
                }
                
                display.ArchiveplanContext.Add(contextItem);
            }
            
            display.ArchiveplanContext.Add(new ArchiveplanContextItem
            {
                ArchiveRecordId = cmiRecord.OBJ_GUID,
                Level = cmiRecord.Verzeichnungsstufe,
                Title = cmiRecord.Titel,
                DateRangeText = cmiRecord.Entstehungszeitraum?.Text,
                RefCode = cmiRecord.Signatur,
                IconId = (int) cmiRecord.Ancestors.Last().TypeId
            });
        }
    }
}
