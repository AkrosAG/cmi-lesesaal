using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using CMI.Access.Harvest.CMIAIS.Mapping;
using CMI.Access.Harvest.Properties;
using CMI.Contract.Common;
using Serilog;

namespace CMI.Access.Harvest.CMIAIS
{
    public class CMIAISArchiveRecordBuilder : IArchiveRecordBuilder
    {
        private readonly IAISSpecificRecordAccess aisSpecificRecordAccess;
        private readonly LanguageSettings languageSettings;
        private readonly IArchiveRecordProcessHandler processHandler;

        public CMIAISArchiveRecordBuilder(IAISSpecificRecordAccess aisSpecificRecordAccess, LanguageSettings languageSettings, IArchiveRecordProcessHandler processHandler)
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
                var cmiRecord = await aisSpecificRecordAccess.GetAisDataRecord(archiveRecordId);
                var cmiRecordTectonic = await aisSpecificRecordAccess.GetAisTectonicRecord(archiveRecordId); 
                Log.Verbose($"Took {stopwatch.ElapsedMilliseconds} ms to fetch detail record from CDWS with id {archiveRecordId}");

                var archiveRecordBuilder = new ArchiveRecordMapperBuilder(cmiRecord, cmiRecordTectonic, languageSettings, aisSpecificRecordAccess);

                var metaDataBuilder = await archiveRecordBuilder
                        .AddMedataData()
                        .WithUsageInfos()
                        .WithNodeInfos();

                AddDetailData(metaDataBuilder);
                
                Log.Information($"CDWS Root is {Settings.Default.CdwsRoot}");

                var record = archiveRecordBuilder.Build();

                record.Display = await GetDisplaySection(cmiRecord, cmiRecordTectonic, record);

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
                .From(nameof(Verzeichnungseinheit.BemerkungStandort), vz => vz.BemerkungStandort)

                .From(nameof(Verzeichnungseinheit.CustomFreeBoolField01), vz => vz.CustomFreeBoolField01)
                .From(nameof(Verzeichnungseinheit.CustomFreeBoolField02), vz => vz.CustomFreeBoolField02)
                .From(nameof(Verzeichnungseinheit.CustomFreeBoolField03), vz => vz.CustomFreeBoolField03)
                .From(nameof(Verzeichnungseinheit.CustomFreeBoolField04), vz => vz.CustomFreeBoolField04)
                .From(nameof(Verzeichnungseinheit.CustomFreeBoolField05), vz => vz.CustomFreeBoolField05)
                .From(nameof(Verzeichnungseinheit.CustomFreeBoolField06), vz => vz.CustomFreeBoolField06)
                .From(nameof(Verzeichnungseinheit.CustomFreeBoolField07), vz => vz.CustomFreeBoolField07)
                .From(nameof(Verzeichnungseinheit.CustomFreeBoolField08), vz => vz.CustomFreeBoolField08)
                .From(nameof(Verzeichnungseinheit.CustomFreeBoolField09), vz => vz.CustomFreeBoolField09)
                .From(nameof(Verzeichnungseinheit.CustomFreeBoolField10), vz => vz.CustomFreeBoolField10)

                .From(nameof(Verzeichnungseinheit.CustomFreeDateField01), vz => vz.CustomFreeDateField01)
                .From(nameof(Verzeichnungseinheit.CustomFreeDateField02), vz => vz.CustomFreeDateField02)
                .From(nameof(Verzeichnungseinheit.CustomFreeDateField03), vz => vz.CustomFreeDateField03)
                .From(nameof(Verzeichnungseinheit.CustomFreeDateField04), vz => vz.CustomFreeDateField04)
                .From(nameof(Verzeichnungseinheit.CustomFreeDateField05), vz => vz.CustomFreeDateField05)
                .From(nameof(Verzeichnungseinheit.CustomFreeDateField06), vz => vz.CustomFreeDateField06)
                .From(nameof(Verzeichnungseinheit.CustomFreeDateField07), vz => vz.CustomFreeDateField07)
                .From(nameof(Verzeichnungseinheit.CustomFreeDateField08), vz => vz.CustomFreeDateField08)
                .From(nameof(Verzeichnungseinheit.CustomFreeDateField09), vz => vz.CustomFreeDateField09)
                .From(nameof(Verzeichnungseinheit.CustomFreeDateField10), vz => vz.CustomFreeDateField10)

                .From(nameof(Verzeichnungseinheit.CustomFreeNumberField01), vz => vz.CustomFreeNumberField01)
                .From(nameof(Verzeichnungseinheit.CustomFreeNumberField02), vz => vz.CustomFreeNumberField02)
                .From(nameof(Verzeichnungseinheit.CustomFreeNumberField03), vz => vz.CustomFreeNumberField03)
                .From(nameof(Verzeichnungseinheit.CustomFreeNumberField04), vz => vz.CustomFreeNumberField04)
                .From(nameof(Verzeichnungseinheit.CustomFreeNumberField05), vz => vz.CustomFreeNumberField05)
                .From(nameof(Verzeichnungseinheit.CustomFreeNumberField06), vz => vz.CustomFreeNumberField06)
                .From(nameof(Verzeichnungseinheit.CustomFreeNumberField07), vz => vz.CustomFreeNumberField07)
                .From(nameof(Verzeichnungseinheit.CustomFreeNumberField08), vz => vz.CustomFreeNumberField08)
                .From(nameof(Verzeichnungseinheit.CustomFreeNumberField09), vz => vz.CustomFreeNumberField09)
                .From(nameof(Verzeichnungseinheit.CustomFreeNumberField10), vz => vz.CustomFreeNumberField10)

                .From(nameof(Verzeichnungseinheit.CustomFreeTextField01), vz => vz.CustomFreeTextField01)
                .From(nameof(Verzeichnungseinheit.CustomFreeTextField02), vz => vz.CustomFreeTextField02)
                .From(nameof(Verzeichnungseinheit.CustomFreeTextField03), vz => vz.CustomFreeTextField03)
                .From(nameof(Verzeichnungseinheit.CustomFreeTextField04), vz => vz.CustomFreeTextField04)
                .From(nameof(Verzeichnungseinheit.CustomFreeTextField05), vz => vz.CustomFreeTextField05)
                .From(nameof(Verzeichnungseinheit.CustomFreeTextField06), vz => vz.CustomFreeTextField06)
                .From(nameof(Verzeichnungseinheit.CustomFreeTextField07), vz => vz.CustomFreeTextField07)
                .From(nameof(Verzeichnungseinheit.CustomFreeTextField08), vz => vz.CustomFreeTextField08)
                .From(nameof(Verzeichnungseinheit.CustomFreeTextField09), vz => vz.CustomFreeTextField09)
                .From(nameof(Verzeichnungseinheit.CustomFreeTextField10), vz => vz.CustomFreeTextField10)
                .From(nameof(Verzeichnungseinheit.CustomFreeTextField11), vz => vz.CustomFreeTextField11)
                .From(nameof(Verzeichnungseinheit.CustomFreeTextField12), vz => vz.CustomFreeTextField12)
                .From(nameof(Verzeichnungseinheit.CustomFreeTextField13), vz => vz.CustomFreeTextField13)
                .From(nameof(Verzeichnungseinheit.CustomFreeTextField14), vz => vz.CustomFreeTextField14)
                .From(nameof(Verzeichnungseinheit.CustomFreeTextField15), vz => vz.CustomFreeTextField15)
                .From(nameof(Verzeichnungseinheit.CustomFreeTextField16), vz => vz.CustomFreeTextField16)
                .From(nameof(Verzeichnungseinheit.CustomFreeTextField17), vz => vz.CustomFreeTextField17)
                .From(nameof(Verzeichnungseinheit.CustomFreeTextField18), vz => vz.CustomFreeTextField18)
                .From(nameof(Verzeichnungseinheit.CustomFreeTextField19), vz => vz.CustomFreeTextField19)
                .From(nameof(Verzeichnungseinheit.CustomFreeTextField20), vz => vz.CustomFreeTextField20)

                .FromCollection(nameof(Verzeichnungseinheit.Reproduktionsart), vz => vz?.Reproduktionsart?.Select(s => s.Bezeichnung))
                .FromCollection(nameof(Verzeichnungseinheit.Schrift), vz => vz?.Schrift?.Select(s => s.Bezeichnung))
                .FromCollection(nameof(Verzeichnungseinheit.Sprache), vz => vz?.Sprache?.Select(s => s.Bezeichnung))
                .FromCollection(nameof(Verzeichnungseinheit.Ueberlieferungsform), vz => vz?.Ueberlieferungsform?.Select(u => u.Bezeichnung))
                .FromCollection(nameof(Verzeichnungseinheit.Provenienz), vz => vz?.Provenienz?.Select(a => a.OffiziellerName))
                .FromCollection(nameof(Verzeichnungseinheit.Archivalienart), vz => vz?.Archivalienart?.Select(a => a.Bezeichnung))
                .FromCollection(nameof(Verzeichnungseinheit.Standort), vz => vz?.Standort?.Select(a => a.ToString()))
                .FromCollection(nameof(Verzeichnungseinheit.Umfang), vz => vz?.Umfang?.Select(u => $"{u.Wert} {u.Masseinheit}"))
                .FromCollection(nameof(Verzeichnungseinheit.Akzession), vz => vz.Akzession?.Select(a => $"{a.Akzessionsnummer} {a.Titel}"));

        }
        private async Task<ArchiveRecordDisplay> GetDisplaySection(Verzeichnungseinheit cmiRecord, Tektonik.Verzeichnungseinheit cmiRecordTectonic, ArchiveRecord archiveRecord)
        {
            var display = new ArchiveRecordDisplay
            {
                InternalDisplayTemplateName = $"{cmiRecord.TypeName}_Intern",
                ExternalDisplayTemplateName = $"{cmiRecord.TypeName}_Extern",
                ContainsImages = archiveRecord?.Metadata?.DetailData?.Any(d => d.ElementType == DataElementElementType.image) ?? false,
                ContainsMedia = archiveRecord?.Metadata?.DetailData?.Any(d => d.ElementType == DataElementElementType.media) ?? false,
                CanBeOrdered = false // Wird im Custom Script gesetzt/überschrieben
            };
            
            await CalculateTreeContext(display, cmiRecord, cmiRecordTectonic, archiveRecord);
            return display;
        }

        private async Task CalculateTreeContext(ArchiveRecordDisplay display, Verzeichnungseinheit cmiRecord, Tektonik.Verzeichnungseinheit cmiRecordTectonic, ArchiveRecord archiveRecord)
        {
            if (string.IsNullOrWhiteSpace(archiveRecord.Metadata.NodeInfo.ParentArchiveRecordId))
                return;

            var parentRecordId = archiveRecord.Metadata.NodeInfo.ParentArchiveRecordId;
            var parentRecord = await aisSpecificRecordAccess.GetAisTectonicRecord(parentRecordId);
            if (parentRecord == null)
            {
                Log.Warning("ParentArchiveRecordId {0} konnte nict geladen werden", parentRecordId);
                return;
            }
            
            display.ParentArchiveRecordId = parentRecord.OBJ_GUID;
            display.FirstChildArchiveRecordId = cmiRecordTectonic.Children.FirstOrDefault()?.OBJ_GUID;

            if (parentRecord.Children.Any())
            {
                var indexOfMe = parentRecord.Children.ToList().FindIndex(c => c.OBJ_GUID == cmiRecordTectonic.OBJ_GUID);
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

            foreach(var ancestor in cmiRecordTectonic.Ancestors.OrderByDescending(b => b.Depth))
            {
                // Mandant is never published, so we skip it 
                // Archiv has no ancestor
                if (ancestor.TypeKey.Equals("Archiv", StringComparison.InvariantCultureIgnoreCase) || ancestor.TypeKey.Equals("Mandant", StringComparison.InvariantCultureIgnoreCase))
                {
                    continue;
                }
                
                var ancestorRecord = await aisSpecificRecordAccess.GetAisDataRecord(ancestor.OBJ_GUID);
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
                IconId = parentRecord.Ancestors.Count > 0 ? (int)parentRecord.Ancestors?.Last()?.TypeId : 0

            });
        }
    }
}
