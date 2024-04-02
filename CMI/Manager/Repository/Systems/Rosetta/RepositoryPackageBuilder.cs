using CMI.Access.Repository.Systems.Rosetta;
using CMI.Contract.Common;
using CMI.Contract.Common.Gebrauchskopie;
using CMI.Manager.Repository.Systems.Rosetta.Schema;
using MassTransit;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

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
            var mets = Mets.LoadFromFile(fileUrl);

            // Das erste DIV des Masters ist immer die "Table of Contents". Dieses DIV ist für uns das Root
            var tableOfContent = mets.GetTableOfContent();

            var package = GetPackageFromXml(archiveRecord);
            var ordnungssystemposition = package.Ablieferung.Ordnungssystem.Ordnungssystemposition.First();
            // Jetzt lesen wir Rekursiv die Dateien und Unterordner aus und fügen diese dem Dossier hinzu
            ordnungssystemposition.Dossier = new List<DossierDIP> { GetDossierFromElastic(archiveRecord) };
            // Jetzt lesen wir Rekursiv die Dateien und Unterordner aus und fügen diese dem Dossier hinzu
            var dossierDip = ordnungssystemposition.Dossier.First();
            AddSubdossiers(dossierDip, tableOfContent, mets);

            // TODO: Noch nicht vollständig Generiere noch das Inhaltsverzeichnis
            var contentRoot = new OrdnerDIP
            {
                Id = $"contentRoot{DateTime.Now.Ticks}",
                Name = "content",
                OriginalName = "content",
            };

            package.Inhaltsverzeichnis.Ordner.Add(contentRoot);
            var folder = mets.GetImportFolderName();

            var rootPath = Path.Combine(Path.GetDirectoryName(fileUrl), folder);
            AddInhaltsverzeichnis(contentRoot, rootPath, mets);

            var metadataXmlPath = Path.Combine(Path.GetDirectoryName(fileUrl), "metadata.xml");
            ((Paket)package).SaveToFile(metadataXmlPath);

            var result = new RepositoryPackage
            {
                // TODO: Hier muss noch das Package in das Repository geschrieben werden
            };

            return await Task.FromResult(result);
        }

        private static void AddSubdossiers(DossierDIP dossier, DivType root, Mets mets)
        {
            foreach (var div in root.Div)
            {
                // Verarbeite File nodes
                if (div.IsFileNode())
                {
                    ProcessFileNode(dossier, div, mets);
                }

                // Verarbeite Verzeicnisse or Null Type nodes welche als Verzeichnisse interpretiert werden
                if (div.IsFolderNode() || div.IsEmptyTypeNode())
                {
                    ProcessFolderOrEmptyNode(dossier, div, mets);
                }
            }
        }

        private static void ProcessFileNode(DossierDIP dossier, DivType div, Mets mets)
        {
            var firstFptr = div.Fptr.First();
            var dokument = new DokumentDIP()
            {
                Id = firstFptr.ID,
                Titel = GetTechnicalMetadataForFile(firstFptr.FILEID, Section.GeneralFileCharacteristics, SectionGeneralFileCharacteristics.Label, mets),
                Erscheinungsform = ErscheinungsformDokument.digital,
                // Bei Rosetta gibt es vermutlich immer nur einen FilePointer, aber theoretisch könnte es auch mehr sein
                DateiRef = div.Fptr.Select(fptr => fptr.FILEID).ToList()
            };

            dossier.Dokument.Add(dokument);
        }

        private static void ProcessFolderOrEmptyNode(DossierDIP dossier, DivType div, Mets mets)
        {
            var subDossier = new DossierDIP()
            {
                Id = div.ID,
                DateiRef = new List<string>(),
                Titel = div.LABEL,
                Entstehungszeitraum = dossier.Entstehungszeitraum
            };

            if (div.HasSubNodes())
            {
                AddSubdossiers(subDossier, div, mets);
                dossier.Dossier.Add(subDossier);
            }
        }

        private static void AddInhaltsverzeichnis(OrdnerDIP ordner, string rootPath, Mets mets)
        {
            var subdirs = Directory.GetDirectories(rootPath);

            // Get the files in the repository directory and copy to the local directory
            foreach (var file in Directory.GetFiles(rootPath))
            {
                var fileInfo = new FileInfo(file);
                if (fileInfo.Exists)
                {
                    var datei = new DateiDIP();
                    if (fileInfo.Name.Contains('_'))
                    {
                        var id = fileInfo.Name.Substring(0, fileInfo.Name.IndexOf('_'));
                        datei.Id = id;
                        datei.Name = fileInfo.Name;
                        datei.Pruefalgorithmus = DateiPruefalgorithmus(mets, id);
                        datei.OriginalName = GetTechnicalMetadataForFile(id, Section.GeneralFileCharacteristics,
                                SectionGeneralFileCharacteristics.FileOriginalName, mets);
                        datei.Pruefsumme = GetTechnicalMetadataForFile(id, Section.FileFixity, SectionFileFixity.FixityValue, mets);

                        datei.Eigenschaft = new List<EigenschaftDatei>
                        {
                            new EigenschaftDatei
                            {
                                Value = GetTechnicalMetadataForFile(id, Section.GeneralFileCharacteristics,
                                    SectionGeneralFileCharacteristics.FileSizeBytes, mets),
                                Name = "FileSizeBytes"
                            },
                            new EigenschaftDatei
                            {
                                Value = GetTechnicalMetadataForFile(id, Section.GeneralFileCharacteristics,
                                    SectionGeneralFileCharacteristics.FormatLibraryId, mets),
                                Name = "FormatLibraryId"
                            },
                            new EigenschaftDatei
                            {
                                Value = GetTechnicalMetadataForFile(id, Section.GeneralFileCharacteristics,
                                    SectionGeneralFileCharacteristics.FileModificationDate, mets),
                                Name = "FileModificationDate"
                            }
                        };
                    }
                    else
                    {
                        datei.Id = fileInfo.Name.Substring(0, fileInfo.Name.Length - fileInfo.Extension.Length);
                        datei.Name = fileInfo.Name;
                    }

                    ordner.Datei.Add(datei);
                }
            }

            foreach (var subDir in subdirs)
            {
                var subOrdner = new OrdnerDIP
                {
                    Id = subDir,
                    Name = subDir
                };
                AddInhaltsverzeichnis(subOrdner, subDir, mets);
                ordner.Ordner.Add(subOrdner);
            }
        }

        private static Pruefalgorithmus DateiPruefalgorithmus(Mets mets, string id)
        {
            switch (GetTechnicalMetadataForFile(id, Section.FileFixity, SectionFileFixity.FixityType, mets))
            {
                case "MD5":
                    return Pruefalgorithmus.MD5;
                case "SHA1":
                    return Pruefalgorithmus.SHA1;
                case "SHA256":
                    return Pruefalgorithmus.SHA256;
                case "SHA512":
                    return Pruefalgorithmus.SHA512;
                default:
                    return Pruefalgorithmus.MD5;

            }
        }

        private static string GetAmdSecIdFromFileSec(string fileId, List<object> items)
        {
            foreach (var item in items)
            {
                if (item is FileType file && file.ID.Equals(fileId))
                {
                    return file.ADMID;
                }

                if (item is FileGrpType grp)
                {
                    var retVal = GetAmdSecIdFromFileSec(fileId, grp.Items);
                    if (retVal != null)
                    {
                        return retVal;
                    }
                }
            }

            return null;
        }


        private static string GetTechnicalMetadataForFile(string fileId, string sectionName, string keyId, Mets mets)
        {
            try
            {
                var amdSecId = GetAmdSecIdFromFileSec(fileId, mets.FileSec.FileGrp.Cast<object>().ToList());

                var amdSec = mets.AmdSec.Find(a => a.ID.Equals(amdSecId));

                if (amdSec.TechMD[0].MdWrap.Item is MdSecTypeMdWrapXmlData technischeMetadatenWrapper)
                {
                    var xmlDoc = XDocument.Parse(technischeMetadatenWrapper.Any[0].OuterXml);
                    var element = xmlDoc.Descendants().FirstOrDefault(n =>
                        n.Attributes().Any(a => a.Name.LocalName.Equals("id") && a.Value.Equals(sectionName)));
                    if (element != null)
                    {
                        var value = element.Descendants().FirstOrDefault(n => n.Name.LocalName.Equals("key") &&
                                                                              n.Attributes().Any(a => a.Name.LocalName.Equals("id")
                                                                                  && a.Value.Equals(keyId)));
                        return value?.Value;
                    }
                }

            }
            catch (Exception e)
            {
                Log.Warning(e, $"Field {fileId} was in xml not found");
                return $"Field {fileId} Not found";
            }

            // Finde File in fileSec
            return fileId;
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
                    AblieferndeStelle = "Aus Feld Akzessionen der VE, ggf. nach oben navigieren",
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

        private DossierDIP GetDossierFromElastic(ElasticArchiveRecord archiveRecord)
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
