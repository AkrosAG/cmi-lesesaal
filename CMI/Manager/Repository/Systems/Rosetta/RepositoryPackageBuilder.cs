using System;
using System.Collections.Generic;
using System.Diagnostics;
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

            var package = GetPackageFromXml(root, archiveRecord);
            dip = package.Ablieferung.Ordnungssystem.Ordnungssystemposition.First();

            var dossier = GetDossierFromXml(root, archiveRecord);
            dip.Dossier.Add(dossier);

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

        private PaketDIP GetPackageFromXml(XDocument root, ElasticArchiveRecord archiveRecord)
        {
            var package = new PaketDIP
            {
                Generierungsdatum = DateTime.Now,
                SchemaVersion = SchemaVersion.Item41,
                Ablieferung = new AblieferungDIP()
                {
                    Ablieferungstyp = Ablieferungstyp.FILES, // Anhand Feld Erwerbsarten ableiten
                    AblieferndeStelle =  "Aus Feld Akzessionen der VE, ggf. nach oben navigieren",
                    Provenienz = new ProvenienzDIP()
                    {
                        AktenbildnerName = "Die ersten 200 Zeichen aus der Verwaltungsgeschichte übernehmen.",
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

        private DossierDIP GetDossierFromXml(XDocument root, ElasticArchiveRecord archiveRecord)
        {
            var dossier = new DossierDIP()
            {
                Id = Guid.NewGuid().ToString(), // ID wäre die GUID der VE,
                Aktenzeichen = "Verwaltungssignatur",
                Titel = "Titel der VE oder sonst DC.title aus DmdSec",
                Inhalt = "Form und Inhalt",
                Erscheinungsform = ErscheinungsformDossier.digital, // Ableiten aus Überlieferungsformen
                Umfang = "Aus Feld Umfang",
                Entstehungszeitraum = new HistorischerZeitraum()
                {
                    Von = new HistorischerZeitpunkt() { Datum = "aus Entstehungszeitraum" },
                    Bis = new HistorischerZeitpunkt() { Datum = "aus Entstehungszeitraum" }
                },
                EntstehungszeitraumAnmerkung = "Aus Feld Bemerkungen zur Datierung",
                Datenschutz = false, // Ableiten aus ablauf schutzfrist
                Oeffentlichkeitsstatus = "Feld Benutzbarkeit",
                SonstigeBestimmungen = "Aus Feld Zugangsbestimmungen",
                zusatzDaten = new List<ZusatzDatenMerkmal>()
                {
                    new ZusatzDatenMerkmal() {Name = "Signatur", Value = "Aus Feld Signatur"},
                    new ZusatzDatenMerkmal() {Name = "Stufe", Value = "Aus Feld Stufe"},
                    new ZusatzDatenMerkmal() {Name = "Frühere Signaturen", Value = "Aus Feld Alte Signatur"},
                    new ZusatzDatenMerkmal()
                        {Name = "Archivplankontext", Value = "Serialisiertes XML des Archivplankontexts"},
                    new ZusatzDatenMerkmal()
                        {Name = "Identifikation digitales Magazin", Value = "Nummer der IE"},
                },
            };

            return dossier;
        }
    }
}
