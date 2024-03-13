using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using Autofac.Features.Metadata;
using CMI.Access.Repository.Systems.Rosetta;
using CMI.Contract.Common;
using CMI.Contract.Common.Gebrauchskopie;
using CMI.Contract.Parameter;
using CMI.Contract.Repository;
using MassTransit;
using Newtonsoft.Json;
using Serilog;

namespace CMI.Manager.Repository.Systems.Rosetta
{
    public class RepositoryPackageBuilder
    {
        private readonly IRosettaDataAccess rosettaDataAccess;
        private readonly IBus bus;

        public RepositoryPackageBuilder(IRosettaDataAccess rosettaDataAccess, IBus bus)
        {
            this.rosettaDataAccess = rosettaDataAccess;
            this.bus = bus;
        }

        public async Task<RepositoryPackage> BuildAsync(string fileUrl, ElasticArchiveRecord archiveRecord)
        {
            OrdnungssystempositionDIP dip;
            var namespaceManager = new XmlNamespaceManager(new NameTable());
            namespaceManager.AddNamespace("mets", "http://www.loc.gov/METS/");
            
            XDocument root;
            try
            {
                root = XDocument.Load(fileUrl);
                
            }
            catch (Exception ex)
            {
                Log.Error(ex, $@"Error while loading {fileUrl} file.");
                throw;
            }

            var package = GetPackageFromXml(archiveRecord);
            dip = package.Ablieferung.Ordnungssystem.Ordnungssystemposition.First();

            dip.Dossier = new List<DossierDIP>{ GetDossierFromElastic(archiveRecord) };

            var structureMapLogical = root.XPathSelectElement("/mets:mets/mets:structMap[@TYPE='LOGICAL']", namespaceManager);
            var structureMapPhysical = root.XPathSelectElement("/mets:mets/mets:structMap[@TYPE='PHYSICAL']", namespaceManager);

            var structureMap = structureMapLogical ?? structureMapPhysical;
            var master = structureMap.XPathSelectElement("//mets:structMap/mets:div[contains(@LABEL,'MASTER')]", namespaceManager);
            if(master == null)
            {
                Log.Error("Der Preservation Master muss mindestens vorhanden sein.");
                return null;
            }

            return await Task.FromResult<RepositoryPackage>(null);
        }


        public bool CreateMetadataXml(string fileUrl, ElasticArchiveRecord archiveRecord)
        {
            var package = GetPackageFromXml(archiveRecord);
            var dip = package.Ablieferung.Ordnungssystem.Ordnungssystemposition.First();

            dip.Dossier = new List<DossierDIP> { GetDossierFromElastic(archiveRecord) };

            ((Paket)package).SaveToFile(Path.Combine(fileUrl, "metadata.xml"));
            return true;
        }

        private PaketDIP GetPackageFromXml(ElasticArchiveRecord archiveRecord)
        {
            var package = new PaketDIP
            {
                Generierungsdatum = DateTime.Now,
                SchemaVersion = SchemaVersion.Item41,
                Ablieferung = new AblieferungDIP
                {
                    Ablieferungstyp = Ablieferungstyp.FILES, // Anhand Feld Erwerbsarten ableiten
                    AblieferndeStelle =  "Aus Feld Akzessionen der VE, ggf. nach oben navigieren",
                    Provenienz = new ProvenienzDIP
                    {
                        AktenbildnerName = archiveRecord.AdministrativeHistory?.Substring(0, 200) // "Die ersten 200 Zeichen aus der Verwaltungsgeschichte übernehmen.",
                    },
                    Ordnungssystem = new OrdnungssystemDIP()
                    {
                        Name = "Ordnungssystem",
                        Ordnungssystemposition = new List<OrdnungssystempositionDIP>()
                        {
                            new OrdnungssystempositionDIP() {Id = "1"},
                        }
                    }
                }
            };

            return package;
        }

        private DossierDIP GetDossierFromElastic( ElasticArchiveRecord archiveRecord)
        {
            var dossier = new DossierDIP
            {
                Id = archiveRecord.ArchiveRecordId,
                Aktenzeichen = archiveRecord.AdministrativeHistory,
                Titel = archiveRecord.Title,
                Inhalt = archiveRecord.Contains,
                Erscheinungsform = ErscheinungsformDossier.digital, // Ableiten aus Überlieferungsformen
                Umfang = archiveRecord.DetailData.Any(dd => dd.ElementName.Equals("Umfang"))
                    ? string.Join(",", archiveRecord.DetailData.First(dd => dd.ElementName.Equals("Umfang")).TextValues)
                    : string.Empty,
                Entstehungszeitraum = new HistorischerZeitraum
                {
                    Von = new() { Datum = archiveRecord.CreationPeriod?.StartDateText },
                    Bis = new() { Datum = archiveRecord.CreationPeriod?.EndDateText }
                },
                EntstehungszeitraumAnmerkung = archiveRecord.DetailData.Any(dd => dd.ElementName.Equals("BemerkungDatierung"))
                    ? string.Join(",", archiveRecord.DetailData.First(dd => dd.ElementName.Equals("BemerkungDatierung")).TextValues)
                    : string.Empty,
                Datenschutz = archiveRecord.ProtectionEndDate?.Date <= DateTime.Now, // Ableiten aus ablauf schutzfrist
                Oeffentlichkeitsstatus = archiveRecord.Permission,
                SonstigeBestimmungen = archiveRecord.Accessibility,
                zusatzDaten = new List<ZusatzDatenMerkmal>
                {
                    new() { Name = "Signatur", Value = archiveRecord.ReferenceCode },
                    new() { Name = "Stufe", Value = archiveRecord.Level },
                    new() { Name = "Frühere Signaturen", Value = archiveRecord.FormerReferenceCode },
                    new() { Name = "Archivplankontext", Value = JsonConvert.SerializeObject(archiveRecord.ArchiveplanContext) },
                    new() { Name = "Identifikation digitales Magazin", Value = archiveRecord.PrimaryDataLink }
                }
            };

            return dossier;
        }
    }
}
