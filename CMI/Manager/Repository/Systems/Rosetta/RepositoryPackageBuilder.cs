using CMI.Access.Repository.Systems.Rosetta;
using CMI.Contract.Common;
using CMI.Contract.Common.Gebrauchskopie;
using CMI.Manager.Repository.Properties;
using CMI.Manager.Repository.Systems.Rosetta.Schema;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using File = System.IO.File;

namespace CMI.Manager.Repository.Systems.Rosetta
{
    public class RepositoryPackageBuilder
    {
        private readonly XmlNamespaceManager defaultNamespaceManager;
        private int totalFileSize;
        private List<RepositoryFile> files;
        private List<RepositoryFolder> rootFolder;

        public RepositoryPackageBuilder()
        {
            defaultNamespaceManager = new XmlNamespaceManager(new NameTable());
            defaultNamespaceManager.AddNamespace("mets", "http://www.loc.gov/METS/");
        }

        public async Task<RepositoryPackage> BuildRepositoryPackageAsync(ElasticArchiveRecord archiveRecord)
        {
            totalFileSize = 0;
            files = new List<RepositoryFile>();
            rootFolder = new List<RepositoryFolder>();

            var fileUrl = $@"{Path.Combine(Settings.Default.TempStoragePath, archiveRecord.PrimaryDataLink)}\ie.xml";
            var mets = Mets.LoadFromFile(fileUrl);

            // Das erste DIV des Masters ist immer die "Table of Contents". Dieses DIV ist für uns das Root
            var tableOfContent = mets.GetTableOfContent();

            var package = GetPackageFromXml(archiveRecord);
            var ordnungssystemposition = package.Ablieferung.Ordnungssystem.Ordnungssystemposition.First();
            
            ordnungssystemposition.Dossier = new List<DossierDIP> { GetDossierFromElastic(archiveRecord) };
            var dossierDip = ordnungssystemposition.Dossier.First();
            // Jetzt lesen wir Rekursiv die Dateien und Unterordner aus und fügen diese dem Dossier hinzu
            AddSubdossiers(dossierDip, tableOfContent, mets);

            var contentRoot = new OrdnerDIP
            {
                Id = $"contentRoot{DateTime.Now.Ticks}",
                Name = "content",
                OriginalName = "content",
            };

            package.Inhaltsverzeichnis.Ordner.Add(contentRoot);
            var folder = mets.GetImportFolderName();

            var rootPath = Path.Combine(Path.GetDirectoryName(fileUrl), folder);

            AddInhaltsverzeichnis(contentRoot, rootPath, mets, rootFolder);

            var zipFile = Path.Combine(Settings.Default.FileCopyDestinationPath, archiveRecord.ArchiveRecordId + ".zip");
            var zipDir = Path.Combine(Settings.Default.FileCopyDestinationPath, archiveRecord.ArchiveRecordId);
            var contentDir = Path.Combine(zipDir, "content");
            var headerDir = Path.Combine(zipDir, "header");
            Directory.CreateDirectory(headerDir);
            RosettaDataAccess.CopyDirectory(rootPath, contentDir);
            var metadataXmlPath = Path.Combine(headerDir, "metadata.xml");
            ((Paket)package).SaveToFile(metadataXmlPath);

            if (File.Exists(zipFile))
            {
                File.Delete(zipFile);
            }

            var preZip = DateTime.Now;
            ZipFile.CreateFromDirectory(zipDir, zipFile);
            Directory.Delete(zipDir, true);
            var result = new RepositoryPackage
            {
                PackageFileName = archiveRecord.ArchiveRecordId + ".zip",
                PackageId= archiveRecord.PrimaryDataLink,
                ArchiveRecordId = archiveRecord.ArchiveRecordId,
                SizeInBytes = totalFileSize,
                FileCount = package.Inhaltsverzeichnis.Datei.Count,
                RepositoryExtractionDuration = DateTime.Now.Ticks - preZip.Ticks,
                FulltextExtractionDuration = 0,
                Files = files,
                Folders = rootFolder
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
                else if (div.IsFolderNode() || div.IsEmptyTypeNode())
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
                Id = string.IsNullOrEmpty(firstFptr.ID) ?  firstFptr.FILEID : firstFptr.ID,
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

        private void AddInhaltsverzeichnis(OrdnerDIP ordner, string rootPath, Mets mets, List<RepositoryFolder> folders)
        {
            var subFolders = new List<RepositoryFolder>();
            var subdirs = Directory.GetDirectories(rootPath);
            var subFiles = new List<RepositoryFile>();
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

                        var sizeInBytes = GetTechnicalMetadataForFile(id, Section.GeneralFileCharacteristics,
                            SectionGeneralFileCharacteristics.FileSizeBytes, mets);
                        totalFileSize = Convert.ToInt32(sizeInBytes);
                        datei.Eigenschaft = new List<EigenschaftDatei>
                        {
                            new ()
                            {
                                Value = sizeInBytes,
                                Name = "FileSizeBytes"
                            },
                            new()
                            {
                                Value = GetTechnicalMetadataForFile(id, Section.GeneralFileCharacteristics,
                                    SectionGeneralFileCharacteristics.FormatLibraryId, mets),
                                Name = "FormatLibraryId"
                            },
                            new ()
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
                    subFiles.Add(new RepositoryFile
                    {
                        PhysicalName = fileInfo.FullName,
                        Exported = true,
                        HashAlgorithm = datei.Pruefalgorithmus.ToString(),
                        Hash = datei.Pruefsumme,
                        MimeType = fileInfo.Extension,
                        Id  = datei.Id
                    });
                }
            }

            files.AddRange(subFiles);

            foreach (var subDir in subdirs)
            {
                var subOrdner = new OrdnerDIP
                {
                    Id = subDir,
                    Name = subDir
                };
               
                subFolders.Add(new RepositoryFolder()
                {
                    LogicalName = subDir,
                    PhysicalName = subDir, 
                    Id = subDir
                });
                AddInhaltsverzeichnis(subOrdner, subDir, mets, subFolders);
                ordner.Ordner.Add(subOrdner);

            }

            if (subFolders.Count > 0)
            {
                folders.Add(new RepositoryFolder
                {
                    Files = files,
                    Folders = subFolders
                });
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
