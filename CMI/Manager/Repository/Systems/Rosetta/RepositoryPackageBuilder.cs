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
            var mets = Mets.LoadFromFile(Path.Combine(fileUrl, "ie.xml"));
            var namespaceManager = new XmlNamespaceManager(new NameTable());
            namespaceManager.AddNamespace("mets", "http://www.loc.gov/METS/");

            if (mets.DmdSec[0].MdWrap.Item is MdSecTypeMdWrapXmlData metadatenWrapper)
            {
                var dcCoreList = metadatenWrapper.Any[0].ChildNodes.OfType<XmlElement>().Select(n => new DcElement
                {
                    Element = n.Name,
                    Text = n.InnerText
                }).ToList();
            }

            // --------------------------------------------------------------------------------------------------
            // Für jede Repräsentation gibt es eine Struct Map. 
            // Repräsentationen können z.B. sein
            // - Preservation Master
            // - Modified Master
            // - Derivative Copy
            // Und für jede Repräsentation kann es eine Logische und eine Physische Struct Map geben.
            // Uns interessiert die Struktur und Dateien des Modified Masters. Ist dieser nicht vorhanden,
            // dann verwenden wir den Preservation Master, der immer vorhanden ist.
            // Ebenso suchen wir zuerst nach den "LOGICAL" StructMaps und erst danach nach den "LOGICAL"
            // --------------------------------------------------------------------------------------------------

            // Gibt es logische Struct Maps?
            var entryStruct =
                mets.StructMap.Where(s => s.TYPE.Equals("LOGICAL", StringComparison.InvariantCultureIgnoreCase)).ToList();
            DivType master = null;
            // Gibt es keine logische Struct Maps (müsste es neu immer geben), kehren wir zur physischen zurück
            if (entryStruct.Any())
            {
                // Jetzt holen wir den Modified oder wenn nicht vorhanden den Preservation Master
                master = entryStruct.Find(m => m.Div.LABEL.ToUpper().Contains("MODIFIED_MASTER"))?.Div ??
                         entryStruct.Find(m => m.Div.LABEL.ToUpper().Contains("PRESERVATION_MASTER")).Div;
            }
            else
            {
                entryStruct = mets.StructMap.Where(s => s.TYPE.Equals("PHYSICAL", StringComparison.InvariantCultureIgnoreCase)).ToList();
                // Jetzt holen wir den Modified oder wenn nicht vorhanden den Preservation Master
                master = entryStruct.Find(m => m.Div.LABEL.ToUpper().Contains("MODIFIED MASTER"))?.Div ??
                         entryStruct.Find(m => m.Div.LABEL.ToUpper().Contains("PRESERVATION MASTER")).Div;
            }
             
            Debug.Assert(master != null, "Der Preservation Master muss mindestens vorhanden sein.");

            // Das erste DIV des Masters ist immer die "Table of Contents". Dieses DIV ist für uns das Root
            var tableOfContent = master.Div.First();
          
            var package = GetPackageFromXml(archiveRecord);
            var ordnungssystemposition = package.Ablieferung.Ordnungssystem.Ordnungssystemposition.First();
            // Jetzt lesen wir Rekursiv die Dateien und Unterordner aus und fügen diese dem Dossier hinzu
            ordnungssystemposition.Dossier = new List<DossierDIP> { GetDossierFromElastic(archiveRecord) };
            // Jetzt lesen wir Rekursiv die Dateien und Unterordner aus und fügen diese dem Dossier hinzu
            AddSubdossiers(ordnungssystemposition.Dossier.First(), tableOfContent.Div, mets);

            // TODO: Noch nicht vollständig
            // Generiere noch das Inhaltsverzeichnis
            var contentRoot = new OrdnerDIP
            {
                Id = $"contentRoot{DateTime.Now.Ticks}",
                Name = "content",
                OriginalName = "content"
            };

            package.Inhaltsverzeichnis.Ordner.Add(contentRoot);

            AddInhaltsverzeichnis(contentRoot, fileUrl, mets);

            var metadataXmlPath = Path.Combine(fileUrl, "metadata.xml");
            ((Paket)package).SaveToFile(metadataXmlPath);

            return await Task.FromResult(new RepositoryPackage());
        }

        private static void AddSubdossiers(DossierDIP dossier, List<DivType> div, Mets mets)
        {
            foreach (var divType in div)
            {
                if (string.Equals(divType.TYPE, "FILE", StringComparison.InvariantCultureIgnoreCase))
                {
                    var dokument = new DokumentDIP()
                    {
                        Id = divType.Fptr.First().ID,
                        Titel = GetTechnicalMetadataForFile(divType.Fptr.First().FILEID, Section.GeneralFileCharacteristics, SectionGeneralFileCharacteristics.Label, mets),
                        Erscheinungsform = ErscheinungsformDokument.digital,  // Abzuleiten vom Feld Überlieferungsformen

                        DateiRef = new List<string>()
                    };

                    // Bei Rosetta gibt es vermutlich immer nur einen FilePointer, aber theoretisch könnte es auch mehr sein
                    foreach (var fptr in divType.Fptr)
                    {
                        dokument.DateiRef.Add(fptr.FILEID);
                    }

                    dossier.Dokument.Add(dokument);
                }

                // Muss verifiziert werden, ob es wirklich Folder ist, oder was anderes
                // bei unseren Beispieldaten gibt es keine Unterordner
                if (string.Equals(divType.TYPE, "FOLDER", StringComparison.InvariantCultureIgnoreCase))
                {
                    var subDossier = new DossierDIP()
                    {
                        Id = divType.ID,
                        DateiRef = new List<string>(),
                        Titel = divType.LABEL,
                        Entstehungszeitraum = dossier.Entstehungszeitraum  // Vererben von Vater
                    };

                    // Rekursiv Dateien und weitere Dossiers hinzufügen
                    AddSubdossiers(subDossier, divType.Div, mets);

                    dossier.Dossier.Add(subDossier);
                }
            }
        }

        private static void AddInhaltsverzeichnis(OrdnerDIP ordner, string filePath, Mets mets)
        {
            var dirs = Directory.GetDirectories(filePath);

            // Get the files in the repository directory and copy to the local directory
            foreach (var file in Directory.GetFiles(filePath))
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

            foreach (var subDir in dirs)
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
            string retVal = null;
            foreach (var item in items)
            {
                if (item is FileType file)
                {
                    if (file.ID.Equals(fileId))
                    {
                        retVal = file.ADMID;
                        break;
                    }
                }

                if (item is FileGrpType grp)
                {
                    retVal = GetAmdSecIdFromFileSec(fileId, grp.Items);
                }
            }

            return retVal;
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
