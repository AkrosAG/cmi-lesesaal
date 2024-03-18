using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
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
using CMI.Manager.Repository.Properties;
using MassTransit;
using Newtonsoft.Json;
using Serilog;

namespace CMI.Manager.Repository.Systems.Rosetta
{
    public class RepositoryPackageBuilder
    {
        private readonly IRosettaDataAccess rosettaDataAccess;
        private readonly IBus bus;

        private readonly XmlNamespaceManager defaultNamespaceManager;

        public RepositoryPackageBuilder(IRosettaDataAccess rosettaDataAccess, IBus bus)
        {
            this.rosettaDataAccess = rosettaDataAccess;
            this.bus = bus;

            this.defaultNamespaceManager = new XmlNamespaceManager(new NameTable());
            this.defaultNamespaceManager.AddNamespace("mets", "http://www.loc.gov/METS/");
        }

        public async Task<RepositoryPackage> BuildRepositoryPackageAsync(string fileUrl, ElasticArchiveRecord archiveRecord)
        {
            var success = false;
            OrdnungssystempositionDIP dip;
            
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

            var dossier = GetDossierFromElastic(archiveRecord);
            dip.Dossier = new List<DossierDIP>{ dossier };

            var structureMapLogical = root.XPathSelectElement("/mets:mets/mets:structMap[@TYPE='LOGICAL']", defaultNamespaceManager);
            var structureMapPhysical = root.XPathSelectElement("/mets:mets/mets:structMap[@TYPE='PHYSICAL']", defaultNamespaceManager);

            var structureMap = structureMapLogical ?? structureMapPhysical;
            var master = structureMap.XPathSelectElement("//mets:structMap/mets:div[contains(@LABEL,'MASTER')]", defaultNamespaceManager);
            
            if(master == null)
            {
                Log.Error("Der Preservation Master muss mindestens vorhanden sein.");
                return await Task.FromResult<RepositoryPackage>(null);
            }

            var tableOfContent = master.XPathSelectElement("//mets:div/mets:div[1]",defaultNamespaceManager);

            AddSubdossiers(dossier, tableOfContent);

            // Generiere noch das Inhaltsverzeichnis
            var contentRoot = new OrdnerDIP
            {
                Id = $"contentRoot{DateTime.Now.Ticks}",
                Name = "content",
                OriginalName = "content"
            };
            
            var metadataXmlPath = Path.Combine(Settings.Default.TempStoragePath, "metadata.xml");
            success = success ? CreateMetadataXml(metadataXmlPath, archiveRecord) : false;

            return await Task.FromResult<RepositoryPackage>(null);
        }


        private bool CreateMetadataXml(string fileUrl, ElasticArchiveRecord archiveRecord)
        {
            var package = GetPackageFromXml(archiveRecord);
            var dip = package.Ablieferung.Ordnungssystem.Ordnungssystemposition.First();

            dip.Dossier = new List<DossierDIP> { GetDossierFromElastic(archiveRecord) };

            ((Paket)package).SaveToFile(Path.Combine(fileUrl, "metadata.xml"));
            return true;
        }

        private string GetAmdSecIdFromFileSec(string fileId, List<object> items)
        {
            throw new NotImplementedException();
        }

        private string GetTechnicalMetadataForFile()
        {
            throw new NotImplementedException();
        }

        private void AddSubdossiers(DossierDIP dossier, XElement root)
        {
            if (root == null)
            {
                return;
            }
            var elements = root.XPathSelectElements("mets:div",defaultNamespaceManager).ToList();
            foreach (var element in elements)
            {
                var subDossier = new DossierDIP
                {
                    // TODO:
                };
                dossier.Dossier = new List<DossierDIP> { subDossier };
                AddSubdossiers(subDossier, element);
            }
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
