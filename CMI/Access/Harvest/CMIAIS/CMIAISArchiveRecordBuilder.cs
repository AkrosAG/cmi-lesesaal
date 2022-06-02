using CMI.Access.Harvest.CMIAIS.Mapping;
using CMI.Contract.Common;
using System;
using System.Threading.Tasks;

namespace CMI.Access.Harvest.CMIAIS
{
    public class CMIAISArchiveRecordBuilder : IArchiveRecordBuilder
    {
        private readonly CMIAISDataProvider cmiAisDataProvider;
        private readonly LanguageSettings languageSettings;
        private readonly IArchiveRecordSecurityProvider securityProvider;
        private readonly IArchiveRecordDisplayProvider displayProvider;

        public CMIAISArchiveRecordBuilder(CMIAISDataProvider cmiAisDataProvider,
                                          IArchiveRecordSecurityProvider securityProvider,
                                          IArchiveRecordDisplayProvider displayProvider,
                                          LanguageSettings languageSettings)
        {
            this.cmiAisDataProvider = cmiAisDataProvider;
            this.languageSettings = languageSettings;
            this.securityProvider = securityProvider;
            this.displayProvider = displayProvider;
        }

        public async Task<ArchiveRecord> Build(string archiveRecordId)
        {
            var cmiRecord = await cmiAisDataProvider.GetCmiArchiveRecord(archiveRecordId);

            var record = new ArchiveRecordMapperBuilder(cmiRecord, languageSettings)
                .BeginMetaData()
                    .WithNodeInfos()
                    .WithUsageInfos()
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
                            .From(nameof(Verzeichnungseinheit.AblaufVerwerstungsrecht), vz => vz.AblaufVerwerstungsrecht)
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
                            .FromCustomFields()
                .EndMappings()
                    .EndDetailData()
                .EndMetaData()
                .Build();

            record.Security = await securityProvider.GetArchiveRecordSecurity(archiveRecordId);
            record.Display = await displayProvider.GetDisplayData(archiveRecordId);

            return record;
        }
    }
}
