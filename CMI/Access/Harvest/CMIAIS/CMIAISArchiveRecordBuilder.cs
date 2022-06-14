using CMI.Access.Harvest.CMIAIS.Mapping;
using CMI.Contract.Common;
using System.Linq;
using System.Threading.Tasks;

namespace CMI.Access.Harvest.CMIAIS
{
    public class CMIAISArchiveRecordBuilder : IArchiveRecordBuilder
    {
        private readonly IAISSpecificRecordAccess<Verzeichnungseinheit> aisSpecificRecordAccess;
        private readonly LanguageSettings languageSettings;
        private readonly IArchiveRecordProcessHandler processHandler;

        public CMIAISArchiveRecordBuilder(IAISDataProvider cmiAisDataProvider, IAISSpecificRecordAccess<Verzeichnungseinheit> aisSpecificRecordAccess, LanguageSettings languageSettings, IArchiveRecordProcessHandler processHandler)
        {
   
            this.aisSpecificRecordAccess = aisSpecificRecordAccess;
            this.languageSettings = languageSettings;
            this.processHandler = processHandler;
        }

        public async Task<ArchiveRecord> Build(string archiveRecordId)
        {
            var cmiRecord = await aisSpecificRecordAccess.GetAisSpecificRecord(archiveRecordId);
            var archiveRecordBuilder = new ArchiveRecordMapperBuilder(cmiRecord, languageSettings, aisSpecificRecordAccess);
            
            var metaDataBuilder = await archiveRecordBuilder
                    .AddMedataData()
                    .WithUsageInfos()
                    .WithNodeInfos();
            
            AddDetailData(metaDataBuilder);
            
            var record = archiveRecordBuilder.Build();

            await GetDisplaySection(cmiRecord, record);  // TODO: Review
            await processHandler.PostProcessArchiveRecord(record);

            return record;
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
                .FromCustomFields();
        }
        private async Task<ArchiveRecordDisplay> GetDisplaySection(Verzeichnungseinheit cmiRecord, ArchiveRecord archiveRecord)
        {
            var display = new ArchiveRecordDisplay
            {
                InternalDisplayTemplateName = $"{cmiRecord.TypeName}_Intern",
                ExternalDisplayTemplateName = $"{cmiRecord.TypeName}_Extern",
                ContainsImages = archiveRecord.Metadata.DetailData.Any(d => d.ElementType == DataElementElementType.image),
                ContainsMedia = archiveRecord.Metadata.DetailData.Any(d => d.ElementType == DataElementElementType.media),
                CanBeOrdered = true, // ToDo: Abklären, wie sich das ermittelt
            };

            await CalculateTreeContext(display, cmiRecord, archiveRecord);
            return display;
        }

        private async Task CalculateTreeContext(ArchiveRecordDisplay display, Verzeichnungseinheit cmiRecord, ArchiveRecord archiveRecord)
        {
            if (string.IsNullOrWhiteSpace(archiveRecord.Metadata.NodeInfo.ParentArchiveRecordId))
                return;

            var parentRecord = await aisSpecificRecordAccess.GetAisSpecificRecord(archiveRecord.Metadata.NodeInfo.ParentArchiveRecordId);
            
            display.ParentArchiveRecordId = parentRecord.OBJ_GUID;            
            display.FirstChildArchiveRecordId = cmiRecord.Children.FirstOrDefault()?.OBJ_GUID;

            if (parentRecord.Children.Length > 1)
            {
                var indexOfMe = parentRecord.Children.ToList().FindIndex(c => c.OBJ_GUID == cmiRecord.OBJ_GUID);
                var indexNext = indexOfMe + 1;
                var indexPrev = indexOfMe - 1;

                if (parentRecord.Children.Length >= indexNext)
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
                var ancestorRecord = await aisSpecificRecordAccess.GetAisSpecificRecord(ancestor.OBJ_GUID);
                ArchiveplanContextItem contextItem;

                if (ancestorRecord != null)
                {
                    contextItem = new ArchiveplanContextItem
                    {
                        ArchiveRecordId = ancestorRecord.OBJ_GUID,
                        Level = ancestorRecord.TypeName,
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
                Level = cmiRecord.TypeName,
                Title = cmiRecord.Titel,
                DateRangeText = cmiRecord.Entstehungszeitraum?.Text,
                RefCode = cmiRecord.Signatur,
                IconId = -1,
            });
        }
    }
}
