using CMI.Contract.Common;
using CMI.Contract.Common.Gebrauchskopie;
using CMI.Contract.Messaging;
using CMI.Contract.Repository;
using MassTransit;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using CMI.Engine.PackageMetadata.Systems.Rosetta.Schema;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace CMI.Engine.PackageMetadata.Systems.Rosetta
{
    public class RosettaPackageHandler : IPackageHandler
    {
        private IRequestClient<FindArchiveRecordRequest> indexClient;
        public RosettaPackageHandler(IRequestClient<FindArchiveRecordRequest> indexClient)
        {
            this.indexClient = indexClient;
        }

        public async Task CreateMetadataXml(string folderName, RepositoryPackage package, List<RepositoryFile> filesToIgnore)
        {
            var archiveRecord = indexClient.GetResponse<FindArchiveRecordResponse>(new FindArchiveRecordRequest { ArchiveRecordId = package.ArchiveRecordId }).Result.Message.ElasticArchiveRecord;
            var pickupDirectory = Path.Combine(folderName, package.ArchiveRecordId);
            if (!Directory.Exists(pickupDirectory))
            {
                Directory.CreateDirectory(pickupDirectory);
            }

            var headerDir = Path.Combine(pickupDirectory, "header");
            if (!Directory.Exists(headerDir))
            {
                Directory.CreateDirectory(headerDir);
            }

            var paket = GetPackageFromXml(archiveRecord);
            var ordnungssystemposition = paket.Ablieferung.Ordnungssystem.Ordnungssystemposition.First();
            var fileUrl = $@"{Path.Combine(folderName, archiveRecord.PrimaryDataLink)}\ie.xml";
            var mets = Mets.LoadFromFile(fileUrl);

            // Das erste DIV des Masters ist immer die "Table of Contents". Dieses DIV ist für uns das Root
            var tableOfContent = mets.GetTableOfContent();

            ordnungssystemposition.Dossier = new List<DossierDIP> { GetDossierFromElastic(archiveRecord) };
            var dossierDip = ordnungssystemposition.Dossier.First();
            // Jetzt lesen wir Rekursiv die Dateien und Unterordner aus und fügen diese dem Dossier hinzu
            AddSubdossiers(dossierDip, tableOfContent, mets);

            var contentRoot = new OrdnerDIP
            {
                Id = $"contentRoot{DateTime.Now.Ticks}",
                Name = "content",
                OriginalName = "content"
            };

            paket.Inhaltsverzeichnis.Ordner.Add(contentRoot);
            var folder = mets.GetImportFolderName();

            var sourcePath = Path.Combine(Path.GetDirectoryName(fileUrl), folder);
            Log.Information($"Package Source Path: {sourcePath}");
            AddInhaltsverzeichnis(contentRoot, sourcePath, mets);
            var metadataXmlPath = Path.Combine(headerDir, "metadata.xml");
            ((Paket)paket).SaveToFile(metadataXmlPath);
        }
        
        private void AddInhaltsverzeichnis(OrdnerDIP ordner, string rootPath, Mets mets)
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

                        var sizeInBytes = GetTechnicalMetadataForFile(id, Section.GeneralFileCharacteristics,
                            SectionGeneralFileCharacteristics.FileSizeBytes, mets);
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
        
        private DossierDIP GetDossierFromElastic(ElasticArchiveRecord archiveRecord)
        {
            var dossier = new DossierDIP
            {
                Id = archiveRecord.ArchiveRecordId,
                Aktenzeichen = archiveRecord.DetailData.Any(d => d.ElementName == "Verwaltungssignatur") ? string.Join(",", archiveRecord.DetailData?.FirstOrDefault(d => d.ElementName == "Verwaltungssignatur")?.TextValues.ToArray()) : string.Empty,
                Titel = archiveRecord.Title,
                Inhalt = archiveRecord.Contains,
                Erscheinungsform = archiveRecord.DetailData.Any(d => d.ElementName == "Ueberlieferungsform") ?
                    archiveRecord.DetailData?.FirstOrDefault(d => d.ElementName == "Ueberlieferungsform")?.TextValues.FirstOrDefault().ToLower() == "analog" ?
                    ErscheinungsformDossier.nichtdigital : ErscheinungsformDossier.digital : ErscheinungsformDossier.keineAngabe,
                Umfang = archiveRecord.DetailData.Any(dd => dd.ElementName.Equals("Umfang"))
                    ? string.Join(",", archiveRecord.DetailData.First(dd => dd.ElementName.Equals("Umfang")).TextValues)
                    : string.Empty,
                Entstehungszeitraum = GetEntstehungszeitraum(archiveRecord.CreationPeriod),
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

        private static void AddSubdossiers(DossierDIP dossier, DivType root, Mets mets)
        {
            foreach (var div in root.Div)
            {
                // Verarbeite Nodes vom Typ File
                if (div.IsFileNode())
                {
                    ProcessFileNode(dossier, div, mets);
                }

                // Verarbeite Nodes vom Typ Verzeichniss or Null Type nodes welche als Verzeichnisse interpretiert werden
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
                Id = string.IsNullOrEmpty(firstFptr.ID) ? firstFptr.FILEID : firstFptr.ID,
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
                Id = div.ID ?? div.LABEL,
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
        
        private PaketDIP GetPackageFromXml(ElasticArchiveRecord archiveRecord)
        {
            var package = new PaketDIP
            {
                Generierungsdatum = DateTime.Now,
                SchemaVersion = SchemaVersion.Item41,
                Ablieferung = new AblieferungDIP
                {
                    Ablieferungstyp = Ablieferungstyp.FILES, // Anhand Feld Erwerbsarten ableiten
                    AblieferndeStelle = archiveRecord?.DetailData?.FirstOrDefault(d => d.ElementName.Equals("AbgebendeStelle", StringComparison.InvariantCultureIgnoreCase))?.TextValues?.First(),
                    Provenienz = new ProvenienzDIP
                    {
                        AktenbildnerName = archiveRecord?.AdministrativeHistory?.Substring(0, 200) // "Die ersten 200 Zeichen aus der Verwaltungsgeschichte übernehmen.",
                    },
                    Ordnungssystem = new OrdnungssystemDIP()
                    {
                        Name = "Ordnungssystem",
                        Ordnungssystemposition = new List<OrdnungssystempositionDIP>
                        {
                            new () {Id = "1"}
                        }
                    }
                }
            };

            return package;
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

        private HistorischerZeitraum GetEntstehungszeitraum(ElasticTimePeriod creationPeriod)
        {
            var retVal = new HistorischerZeitraum
            {
                Von = new HistorischerZeitpunkt
                    { Ca = creationPeriod.StartDateApproxIndicator, Datum = creationPeriod.StartDate.ToString("yyyy-MM-dd") },
                Bis = new HistorischerZeitpunkt { Ca = creationPeriod.EndDateApproxIndicator, Datum = creationPeriod.EndDate.ToString("yyyy-MM-dd") }
            };
            return retVal;
        }
    }

}
