using CMI.Contract.Common.Gebrauchskopie;
using Iiif.API.Presentation;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace CMI.Engine.Asset.PostProcess
{
    public interface IPostProcessManifestCreator
    {
        void CreateManifest(string archiveRecordId, PaketDIP paket, string packageRoot);
    }

    public class PostProcessManifestCreator : IPostProcessManifestCreator
    {
        private const string presentationContextUri = "http://iiif.io/api/presentation/3/context.json";
        private readonly IiifManifestSettings manifestSettings;
        private readonly ViewerFileLocationSettings locationSettings;
        private string rootDirectory;
        private readonly List<string> pathItems = new List<string>();

        public PostProcessManifestCreator(IiifManifestSettings manifestSettings, ViewerFileLocationSettings locationSettings)
        {
            this.manifestSettings = manifestSettings;
            this.locationSettings = locationSettings;
        }

        public void CreateManifest(string archiveRecordId, PaketDIP paket, string packageRoot)
        {
            rootDirectory = packageRoot;
            pathItems.Add(archiveRecordId);
            var dossier = GetRootDossier(paket);

            // Manifest für das Root Dossier
            var id = CreateCollectionManifestForDossier(dossier, null, archiveRecordId, paket);

            // Verarbeitung von Unterordnern des Root Dossiers
            ProcessDossiers(dossier.Dossier.OrderItems(), id, archiveRecordId, paket);
            // Verarbeitung von Dokumenten des Root Dossiers
            ProcessDocuments(dossier.Dokument.OrderItems(), id, archiveRecordId, paket);
        }

        private DossierDIP GetRootDossier(PaketDIP paket)
        {
            foreach (var ordnungssystemposition in paket.Ablieferung.Ordnungssystem.Ordnungssystemposition)
            {
                var dossier = FindRootDossier(ordnungssystemposition);
                if (dossier != null)
                {
                    return dossier;
                }
            }

            return null;
        }

        private void ProcessDocuments(List<DokumentDIP> documents, Uri id, string archiveRecordId, PaketDIP paket)
        {
            // Verarbeitung der Dokumente gemäss spezieller Sortierreihenfolge
            foreach (var document in documents.OrderItems())
            {
                CreateCollectionManifestForDokument(document, id, archiveRecordId, paket);
            }
        }

        private void ProcessDossiers(List<DossierDIP> dossiers, Uri parentItem, string archiveRecordId, PaketDIP paket)
        {
            // Verarbeitung der Dossiers gemäss spezieller Sortierreihenfolge
            foreach (var dossier in dossiers.OrderItems())
            {
                // subdossiers are saved in a path that has the same name as the dossier
                pathItems.Add(dossier.Titel);
                CreateCollectionManifestForDossier(dossier, parentItem, archiveRecordId, paket);

                // Allfällige Dokumente
                ProcessDocuments(dossier.Dokument.OrderItems(), parentItem, archiveRecordId, paket);

                // Weitere Unterdossiers
                ProcessDossiers(dossier.Dossier.OrderItems(), parentItem, archiveRecordId, paket);
            }
        }

        /// <summary>
        /// Creates a collection manifest for a dossier
        /// </summary>
        /// <param name="dossier">The DIP representation of the dossier</param>
        /// <param name="parentManifestId">The id of the parent collection manifest</param>
        /// <param name="archiveRecordId">The archive record id the collection manifest belongs to</param>
        /// <param name="paket">The complete package of which the dossier is part of</param>
        /// <returns></returns>
        private Uri CreateCollectionManifestForDossier(DossierDIP dossier, Uri parentManifestId, string archiveRecordId, PaketDIP paket)
        {
            var relativePath = string.Join("/", pathItems.Select(Uri.EscapeUriString));

            // Get the data
            var signatur = dossier.zusatzDaten.GetZusatzMerkmal("Signatur");
            var entstehungszeitraum = dossier.zusatzDaten.GetZusatzMerkmal("Entstehungszeitraum Anzeigetext");
            if (string.IsNullOrEmpty(entstehungszeitraum))
            {
                entstehungszeitraum = $"{dossier.Entstehungszeitraum.Von.Datum} - {dossier.Entstehungszeitraum.Bis.Datum}";
            }

            var land = dossier.zusatzDaten.GetZusatzMerkmal("Land");
            var darin = dossier.zusatzDaten.GetZusatzMerkmal("Darin");


            // Start des Manifests
            var presentation = new Presentation();
            var fileName = $"{(parentManifestId == null ? archiveRecordId : Uri.EscapeUriString(dossier.Titel))}.json";
            presentation.Context = presentationContextUri;
            presentation.Id = new Uri(manifestSettings.PublicManifestWebUri, $"{relativePath}/{fileName}");
            presentation.Type = "Collection";
            presentation.Label = new LanguageValue
            {
                Invariant = new List<string> { string.IsNullOrEmpty(dossier.Titel) ? "unbekannt" : dossier.Titel }
            };

            // If we have a parent, create the link
            if (parentManifestId != null)
            {
                presentation.PartOf = new List<PartOfElement>
                {
                    new() {Id = parentManifestId, Type = "Collection"}
                };
            }

            #region Add Metadata

            // Add the metadata
            presentation.Metadata = new List<RequiredStatementElement>();
            if (!string.IsNullOrEmpty(signatur))
            {
                presentation.Metadata.Add(AddRequiredStatementElement("Signatur", signatur));
            }

            if (!string.IsNullOrEmpty(dossier.Titel))
            {
                presentation.Metadata.Add(AddRequiredStatementElement("Titel", dossier.Titel));
            }

            if (!string.IsNullOrEmpty(entstehungszeitraum))
            {
                presentation.Metadata.Add(AddRequiredStatementElement("Entstehungszeitraum", entstehungszeitraum));
            }

            if (!string.IsNullOrEmpty(dossier.Aktenzeichen))
            {
                presentation.Metadata.Add(AddRequiredStatementElement("Aktenzeichen", dossier.Aktenzeichen));
            }

            if (!string.IsNullOrEmpty(land))
            {
                presentation.Metadata.Add(AddRequiredStatementElement("Land", land));
            }

            if (!string.IsNullOrEmpty(darin))
            {
                presentation.Metadata.Add(AddRequiredStatementElement("Darin", darin));
            }

            #endregion

            // Add the sub items
            presentation.Items = AddDokumentsAndSubdossiers(dossier, paket, archiveRecordId, relativePath);

            // Save the file
            var path = Path.Combine(locationSettings.ManifestOutputSaveDirectory, string.Join("\\", pathItems));
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            var fullFileName = Path.Combine(path, fileName);
            File.WriteAllText(fullFileName, presentation.ToJson());

            // If a dossier directly has documents, then we need to create a dokument for that dossier as well
            if (dossier.DateiRef.Any(d => !d.EndsWith("_PREMIS.xml")))
            {
                var dokument = new DokumentDIP
                {
                    Titel = dossier.Id,
                    DateiRef = dossier.DateiRef,
                };
                CreateCollectionManifestForDokument(dokument, presentation.Id, archiveRecordId, paket, true);
            }

            // Return the identifier
            return presentation.Id;
        }

        private Uri CreateCollectionManifestForDokument(DokumentDIP dokument, Uri parentManifest, string archiveRecordId, PaketDIP paket, bool isTempDocument = false)
        {
            var relativePath = string.Join("/", pathItems.Select(Uri.EscapeUriString));

            // Get the data
            var signatur = dokument.zusatzDaten.GetZusatzMerkmal("Signatur");
            var entstehungszeitraum = dokument.zusatzDaten.GetZusatzMerkmal("Entstehungszeitraum Anzeigetext");
            if (string.IsNullOrEmpty(entstehungszeitraum))
            {
                entstehungszeitraum = $"{dokument.Entstehungszeitraum.Von.Datum} - {dokument.Entstehungszeitraum.Bis.Datum}";
            }

            var darin = dokument.zusatzDaten.GetZusatzMerkmal("Darin");


            // Start des Manifests
            var presentation = new Presentation();
            var fileName = $"{(parentManifest == null ? archiveRecordId : Uri.EscapeUriString(dokument.Titel))}.json";
            presentation.Context = presentationContextUri;
            presentation.Id = new Uri(manifestSettings.PublicManifestWebUri, $"{relativePath}/{fileName}");
            presentation.Type = "Manifest";
            presentation.Label = new LanguageValue
            {
                Invariant = new List<string> { string.IsNullOrEmpty(dokument.Titel) ? "unbekannt" : dokument.Titel }
            };

            // If we have a parent, create the link
            presentation.PartOf = new List<PartOfElement>
            {
                new() {Id = parentManifest, Type = "Collection"}
            };

            // Add the metadata
            presentation.Metadata = new List<RequiredStatementElement>();
            if (!string.IsNullOrEmpty(signatur))
            {
                presentation.Metadata.Add(AddRequiredStatementElement("Signatur", signatur));
            }

            if (!string.IsNullOrEmpty(dokument.Titel))
            {
                presentation.Metadata.Add(AddRequiredStatementElement("Titel", dokument.Titel));
            }

            if (!string.IsNullOrEmpty(entstehungszeitraum))
            {
                presentation.Metadata.Add(AddRequiredStatementElement("Entstehungszeitraum", entstehungszeitraum));
            }

            if (!string.IsNullOrEmpty(darin))
            {
                presentation.Metadata.Add(AddRequiredStatementElement("Darin", darin));
            }

            // Add thumbnail
            presentation.Thumbnail = AddThumbnailElement(dokument, paket, relativePath, isTempDocument);

            // Add rendering
            presentation.Rendering = new List<RenderingElement>
            {
                new()
                {
                    Id = new Uri(manifestSettings.PublicDetailRecordUri, $"#/de/archiv/einheit/{archiveRecordId}"),
                    Type = "Text",
                    Label = new LanguageValue
                    {
                        German = new List<string> {"Download"},
                        French = new List<string> {"Download"},
                        Italian = new List<string> {"Download"},
                        Englisch = new List<string> {"Download"}
                    },
                    Format = "text/plain"
                },
                new()
                {
                    Id = new Uri(manifestSettings.PublicOcrWebUri,
                        $"{relativePath}{(isTempDocument ? "" : Uri.EscapeUriString(dokument.Titel))}/{Uri.EscapeUriString(dokument.Titel)}_OCR.txt"),
                    Type = "Text",
                    Label = new LanguageValue
                    {
                        German = new List<string> {"OCR"},
                        French = new List<string> {"OCR"},
                        Italian = new List<string> {"OCR"},
                        Englisch = new List<string> {"OCR"}
                    },
                    Format = "text/plain"
                }
            };

            // Add the sub items
            presentation.Items = AddDokumentPages(dokument, paket, relativePath, isTempDocument);

            // Save the file
            var path = Path.Combine(locationSettings.ManifestOutputSaveDirectory, string.Join("\\", pathItems));
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            var fullFileName = Path.Combine(path, fileName);
            File.WriteAllText(fullFileName, presentation.ToJson());

            // Return the identifier
            return presentation.Id;
        }

        private List<IiifItem> AddDokumentPages(DokumentDIP dokument, PaketDIP paket, string relativeUri, bool isTempDocument)
        {
            var retVal = new List<IiifItem>();

            foreach (var dateiRef in dokument.DateiRef)
            {
                var index = dokument.DateiRef.IndexOf(dateiRef);
                var dateiLocation = FindFileInPackage(dateiRef, paket.Inhaltsverzeichnis.Ordner);
                if (dateiLocation != null && !dateiLocation.FullName.EndsWith("_PREMIS.xml"))
                {
                    var dimensions = GetImageDimensions(dateiLocation);

                    var fi = new FileInfo(dateiLocation.FullName);
                    string fileName;
                    switch (fi.Extension.ToLower())
                    {
                        case ".jp2":
                        case ".tif":
                        case ".tiff":
                            fileName = Path.ChangeExtension(fi.Name, ".jpg");
                            break;
                        default:
                            fileName = fi.Name;
                            break;
                    }

                    retVal.Add(new IiifItem
                    {
                        Id = new Uri(manifestSettings.ImageServerUri, $"iiif/2/{relativeUri}/{Uri.EscapeUriString($"{dokument.Titel}-{index}")}"),
                        Type = "Canvas",
                        Label = new LanguageValue { Invariant = new List<string> { $"Seite {index + 1}" } },
                        Width = dimensions.Width,
                        Height = dimensions.Height,
                        Items = new List<AnnotationPage>
                        {
                            new()
                            {
                                Context = new Uri("http://iiif.io/api/presentation/3/context.json"),
                                // Id according to "convention". Does not point anywhere
                                Id = new Uri(manifestSettings.PublicManifestWebUri,
                                    $"{relativeUri}/{dokument.Titel}.json#annotations-page-painting-{dokument.Titel}-{index}"),
                                Type = "AnnotationPage",
                                Items = new List<Annotation>
                                {
                                    new()
                                    {
                                        Id = new Uri(manifestSettings.PublicManifestWebUri,
                                            $"{relativeUri}/{dokument.Titel}.json#annotations-content-painting-{dokument.Titel}-{index}"),
                                        Type = "Annotation",
                                        Motivation = "painting",
                                        Target = new Uri(manifestSettings.PublicManifestWebUri, $"{relativeUri}/canvasses/{dokument.Titel}-{index}"),
                                        Body = new BodyClass
                                        {
                                            Id = new Uri(manifestSettings.ImageServerUri,
                                                $"iiif/2/{Uri.EscapeDataString($"{relativeUri}/{(isTempDocument ? "" : dokument.Titel + "/")}{fileName}")}/full/full/0/default.jpg"),
                                            Type = "Image",
                                            Format = "image/jpeg",
                                            Width = dimensions.Width,
                                            Height = dimensions.Height,
                                            Service = new List<ServiceElement>
                                            {
                                                new()
                                                {
                                                    Id = new Uri(manifestSettings.ImageServerUri,
                                                        $"iiif/2/{Uri.EscapeDataString($"{relativeUri}/{(isTempDocument ? "" : dokument.Titel + "/")}{fileName}")}"),
                                                    Type = "ImageService2",
                                                    Profile = "level2",
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    });
                }
            }

            return retVal;
        }


        private Size GetImageDimensions(PackageFileLocation dateiLocation)
        {
            var retVal = new Size();
            var imagePath = Path.Combine(rootDirectory, dateiLocation.FullName);

            if (imagePath.EndsWith(".jp2", StringComparison.InvariantCultureIgnoreCase))
            {
                imagePath = Path.ChangeExtension(imagePath, ".jpg");
            }

            if (!File.Exists(imagePath))
                throw new FileNotFoundException("Unable to find image file", imagePath);

            using var fileStream = new FileStream(imagePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var image = Image.FromStream(fileStream, false, false);
            retVal.Height = image.Height;
            retVal.Width = image.Width;

            return retVal;
        }

        private List<IiifItem> AddDokumentsAndSubdossiers(DossierDIP dossier, PaketDIP paket, string archiveRecordId, string relativePath)
        {
            var retVal = new List<IiifItem>();

            foreach (var dossierDip in dossier.Dossier.OrderItems())
            {
                var newRelPath = $"{relativePath}/{(string.IsNullOrEmpty(dossierDip.Titel) ? "" : $"{Uri.EscapeUriString(dossierDip.Titel)}/")}";
                retVal.Add(new IiifItem
                {
                    Id = new Uri(manifestSettings.PublicManifestWebUri, $"{newRelPath}{dossierDip.Titel}.json"),
                    Type = "Collection",
                    Label = new LanguageValue { Invariant = new List<string> { dossierDip.Titel } }
                });
            }

            foreach (var dokumentDip in dossier.Dokument.OrderItems())
            {
                retVal.Add(new IiifItem
                {
                    Id = new Uri(manifestSettings.PublicManifestWebUri, $"{relativePath}/{dokumentDip.Titel}.json"),
                    Type = "Manifest",
                    Label = new LanguageValue { Invariant = new List<string> { dokumentDip.Titel } }
                });

                // Add thumbnail(s)
                retVal.Last().Thumbnail = AddThumbnailElement(dokumentDip, paket, relativePath);
            }

            foreach (var datei in dossier.DateiRef.Where(d => !d.EndsWith("_PREMIS.xml")))
            {
                retVal.Add(new IiifItem
                {
                    Id = new Uri(manifestSettings.PublicManifestWebUri, $"{relativePath}/{dossier.Id}.json"),
                    Type = "Manifest",
                    Label = new LanguageValue { Invariant = new List<string> { dossier.Titel } }
                });

                // Creating ad-hoc element for getting the thumbnail
                var dokumentDip = new DokumentDIP
                {
                    Titel = dossier.Id,
                    DateiRef = new List<string> {datei}
                };

                // Add thumbnail(s)
                retVal.Last().Thumbnail = AddThumbnailElement(dokumentDip, paket, relativePath, true);
            }

            return retVal;
        }

        private List<ThumbnailElement> AddThumbnailElement(DokumentDIP dokument, PaketDIP paket, string relativePath, bool isTempDocument = false)
        {
            // Add thumbnail
            var fileRef = dokument.DateiRef.FirstOrDefault();
            if (fileRef != null)
            {
                var location = FindFileInPackage(fileRef, paket.Inhaltsverzeichnis.Ordner);
                var fi = new FileInfo(location.Datei.Name);
                string thumbnailName;
                switch (fi.Extension.ToLower())
                {
                    case ".jp2":
                    case ".tif":
                    case ".tiff":
                        thumbnailName = $"{Path.ChangeExtension(fi.Name, ".jpg")}";
                        break;
                    case ".pdf":
                        thumbnailName = "pdfThumbnail.jpg";
                        break;
                    default:
                        thumbnailName = "defaultThumbnail.jpg";
                        break;
                }

                var newRelativePath = !isTempDocument
                    ? $"{Uri.EscapeDataString($"{relativePath}/{dokument.Titel}/{thumbnailName}")}/full/150,/0/default.jpg"
                    : $"{Uri.EscapeDataString($"{relativePath}/{thumbnailName}")}/full/150,/0/default.jpg";

                return new List<ThumbnailElement>
                {
                    new()
                    {
                        Id = new Uri(manifestSettings.ImageServerUri, $"iiif/2/{newRelativePath}"),
                        Format = "image/jpeg",
                        Type = "Image"
                    }
                };
            }

            return new List<ThumbnailElement>();
        }

        /// <summary>
        ///     Adds a label value pair. If label contains a value the label is added as language invariant. Else you must provide
        ///     a value for each language.
        ///     Same with the value. If value contains a value, then this is used as language invariant. Else you need to provide a
        ///     value for each language.
        /// </summary>
        /// <param name="label"></param>
        /// <param name="labelDe"></param>
        /// <param name="labelFr"></param>
        /// <param name="labelIt"></param>
        /// <param name="labelEn"></param>
        /// <param name="value"></param>
        /// <param name="valueDe"></param>
        /// <param name="valueFr"></param>
        /// <param name="valueIt"></param>
        /// <param name="valueEn"></param>
        /// <returns></returns>
        private static RequiredStatementElement AddRequiredStatementElement(string label, string value,
            string labelDe = null, string labelFr = null, string labelIt = null, string labelEn = null,
            string valueDe = null, string valueFr = null, string valueIt = null, string valueEn = null)
        {
            return new RequiredStatementElement
            {
                Label = string.IsNullOrEmpty(label)
                    ? new LanguageValue
                    {
                        German = new List<string> { labelDe },
                        Englisch = new List<string> { labelEn },
                        French = new List<string> { labelFr },
                        Italian = new List<string> { labelIt }
                    }
                    : new LanguageValue { Invariant = new List<string> { label } },
                Value = string.IsNullOrEmpty(value)
                    ? new LanguageValue
                    {
                        German = new List<string> { valueDe },
                        Englisch = new List<string> { valueEn },
                        French = new List<string> { valueFr },
                        Italian = new List<string> { valueIt }
                    }
                    : new LanguageValue { Invariant = new List<string> { value } }
            };
        }

        private DossierDIP FindRootDossier(OrdnungssystempositionDIP ordnungssystemposition)
        {
            if (ordnungssystemposition.Dossier.Any())
            {
                return ordnungssystemposition.Dossier.First();
            }

            // Process any sub items
            foreach (var ordnungssystemSubPosition in ordnungssystemposition.Ordnungssystemposition)
            {
                var dossier = FindRootDossier(ordnungssystemSubPosition);
                if (dossier != null)
                {
                    return dossier;
                }
            }

            return null;
        }

        private static PackageFileLocation FindFileInPackage(string dateiRef, List<OrdnerDIP> ordnerList, PackageFileLocation retVal = null,
            OrdnerDIP parentOrdner = null)
        {
            if (!ordnerList.Any())
                return retVal;

            if (retVal == null)
            {
                retVal = new PackageFileLocation();
            }
            else
            {
                retVal.OrdnerList.Add(parentOrdner);
            }

            foreach (var ordner in ordnerList)
            {
                foreach (var datei in ordner.Datei)
                {
                    if (datei.Id == dateiRef)
                    {
                        retVal.OrdnerList.Add(ordner);
                        retVal.Datei = datei;
                        return retVal;
                    }
                }

                var dateiSub = FindFileInPackage(dateiRef, ordner.Ordner, retVal, ordner);
                if (dateiSub is {Datei: { }})
                {
                    return dateiSub;
                }
            }

            return null;
        }
    }
}
