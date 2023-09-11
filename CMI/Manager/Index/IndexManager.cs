using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using CMI.Access.Common;
using CMI.Contract.Common;
using CMI.Contract.Messaging;
using CMI.Manager.Index.Config;
using CMI.Manager.Index.ValueExtractors;
using MassTransit;
using Serilog;

namespace CMI.Manager.Index
{
    public class IndexManager : IIndexManager
    {
        private const string dossierLevelIdentifier = "Dossier";
        private readonly ISearchIndexDataAccess dbAccess;
        private readonly IArchiveRecordProcessor archiveRecordProcessor;
        private readonly CustomFieldsConfiguration fieldsConfiguration;

        /// <summary>
        ///     Initializes a new instance of the <see cref="IndexManager" /> class.
        /// </summary>
        /// <param name="dbAccess">The database access object.</param>
        /// <param name="fieldsConfiguration">The fields configuration object.</param>
        public IndexManager(ISearchIndexDataAccess dbAccess, IArchiveRecordProcessor processorInstance, CustomFieldsConfiguration fieldsConfiguration)
        {
            this.dbAccess = dbAccess;
            this.fieldsConfiguration = fieldsConfiguration;
            this.archiveRecordProcessor = processorInstance;
        }

        /// <summary>
        ///     Updates an archive record in ElasticSearch
        /// </summary>
        /// <param name="updateContext">The update context with the information from the bus.</param>
        public void UpdateArchiveRecord(ConsumeContext<IUpdateArchiveRecord> updateContext)
        {
            var archiveRecord = updateContext.Message.ArchiveRecord;
            var elasticArchiveRecord = ConvertArchiveRecord(archiveRecord); // TODO: speziell BAR , muss verallgemeinet werden

            archiveRecordProcessor.PostProcessElasticArchiveRecord(elasticArchiveRecord, archiveRecord);
            
            // Save in elastic
            dbAccess.UpdateDocument(elasticArchiveRecord);
        }

        public void RemoveArchiveRecord(ConsumeContext<IRemoveArchiveRecord> removeContext)
        {
            dbAccess.RemoveDocument(removeContext.Message.ArchiveRecordId);
        }

        public ElasticArchiveRecord FindArchiveRecord(string archiveRecordId, bool includeFulltextContent)
        {
            return dbAccess.FindDocument(archiveRecordId, includeFulltextContent);
        }

        // Returns all archive records for a given package, that is from dossier down to document
        // or in case of a document packageId all the items up to the dossier.
        public List<ElasticArchiveRecord> GetArchiveRecordsForPackage(string packageId)
        {
            var retVal = new List<ElasticArchiveRecord>();
            var entryItem = dbAccess.FindDocumentByPackageId(packageId);
            if (entryItem != null)
            {
                retVal.Add(entryItem);
                Log.Verbose("Added ordered item with id {ArchiveRecordId} to list", entryItem.ArchiveRecordId);
                retVal.AddRange(dbAccess.GetChildren(entryItem.ArchiveRecordId, true));
                Log.Verbose("Added the children of the ordered item with id {ArchiveRecordId} to list. Found {Count} children",
                    entryItem.ArchiveRecordId, retVal.Count - 1);

                // Is the package we fetched already on dossier level?
                // If not, traverse up the hierarchie until we find the dossier level
                while (entryItem != null && entryItem.Level.Equals(dossierLevelIdentifier, StringComparison.InvariantCultureIgnoreCase) == false)
                {
                    Log.Verbose("Ordered item is not dossier level. So we traverse up.");
                    if (!string.IsNullOrEmpty(entryItem.ParentArchiveRecordId))
                    {
                        entryItem = dbAccess.FindDocument(entryItem.ParentArchiveRecordId, false);
                        if (entryItem != null)
                        {
                            Log.Verbose("Found parent item with id {ArchiveRecordId}. Adding to collection.", entryItem.ArchiveRecordId);
                            retVal.Insert(0, entryItem);
                        }
                    }
                }
            }

            return retVal;
        }

        public void UpdateTokens(string id, string[] primaryDataDownloadAccessTokens, string[] primaryDataFulltextAccessTokens,
            string[] metadataAccessTokens)
        {
            dbAccess.UpdateTokens(id, primaryDataDownloadAccessTokens, primaryDataFulltextAccessTokens, metadataAccessTokens);
        }


        private ElasticArchiveRecord ConvertArchiveRecord(ArchiveRecord archiveRecord)
        {
            // ReSharper disable once UseObjectOrCollectionInitializer
            // Helps in case of an exception. Line number point to exact location
            var elasticArchiveRecord = new ElasticArchiveRecord();
            elasticArchiveRecord.ArchiveRecordId = archiveRecord.ArchiveRecordId;
            elasticArchiveRecord.MetadataAccessTokens = archiveRecord.Security?.MetadataAccessToken;
            elasticArchiveRecord.PrimaryDataDownloadAccessTokens = archiveRecord.Security?.PrimaryDataDownloadAccessToken;
            elasticArchiveRecord.PrimaryDataFulltextAccessTokens = archiveRecord.Security?.PrimaryDataFulltextAccessToken;
            elasticArchiveRecord.PrimaryDataLink = archiveRecord.Metadata.PrimaryDataLink;
            elasticArchiveRecord.HasImage = archiveRecord.Display.ContainsImages;
            elasticArchiveRecord.HasAudioVideo = archiveRecord.Display.ContainsMedia;
            elasticArchiveRecord.CanBeOrdered = archiveRecord.Display.CanBeOrdered;
            elasticArchiveRecord.TreePath = archiveRecord.Metadata.NodeInfo.Path;
            elasticArchiveRecord.TreeSequence = archiveRecord.Metadata.NodeInfo.Sequence;
            elasticArchiveRecord.TreeLevel = archiveRecord.Metadata.NodeInfo.Level;
            elasticArchiveRecord.IsLeaf = archiveRecord.Metadata.NodeInfo.IsLeaf;
            elasticArchiveRecord.IsRoot = archiveRecord.Metadata.NodeInfo.IsRoot;
            elasticArchiveRecord.NichtOnlineRecherchierbareDossiers = archiveRecord.Metadata?.DetailData
                ?.FirstOrDefault(data => data?.ElementName == "NICHT_ONLINE_RECHERCHIERBARE_DOSSIERS")?.ElementValue?.FirstOrDefault()?.TextValues
                ?.FirstOrDefault()?.Value;
            elasticArchiveRecord.ChildCount = archiveRecord.Metadata.NodeInfo.ChildCount;
            elasticArchiveRecord.AccessionDate = archiveRecord.Metadata.AccessionDate;
            elasticArchiveRecord.ProtectionStartDate = archiveRecord.Metadata.Usage.ProtectionBaseDate;
            elasticArchiveRecord.ProtectionEndDate = archiveRecord.Metadata.Usage.ProtectionEndDate.HasValue
                ? new ElasticDateWithYear
                {
                    Date = archiveRecord.Metadata.Usage.ProtectionEndDate.Value,
                    Year = archiveRecord.Metadata.Usage.ProtectionEndDate.Value.Year
                }
                : null;
            
            elasticArchiveRecord.ProtectionCategory = archiveRecord.Metadata.Usage.ProtectionCategory;
            elasticArchiveRecord.ProtectionDuration = archiveRecord.Metadata.Usage.ProtectionDuration;
            elasticArchiveRecord.Accessibility = archiveRecord.Metadata.Usage.Accessibility;
            elasticArchiveRecord.PhysicalUsability = archiveRecord.Metadata.Usage.PhysicalUsability;
            elasticArchiveRecord.Permission = archiveRecord.Metadata.Usage.Permission;
            elasticArchiveRecord.IsPhysicalyUsable = archiveRecord.Metadata.Usage.IsPhysicalyUsable;
            elasticArchiveRecord.ContainsPersonRelatedInformation = archiveRecord.Metadata.Usage.ContainsPersonRelatedData;
            elasticArchiveRecord.Places = null;
            elasticArchiveRecord.ParentContentInfos =
                archiveRecord.Display.ArchiveplanContext.Select(c => new ElasticParentContentInfo {Title = c.Title}).ToList();
            elasticArchiveRecord.ArchiveplanContext = archiveRecord.Display.ArchiveplanContext.Select(a => new ElasticArchiveplanContextItem
                {
                    ArchiveRecordId = a.ArchiveRecordId,
                    DateRangeText = a.DateRangeText,
                    Level = a.Level,
                    IconId = a.IconId,
                    RefCode = a.RefCode,
                    Title = a.Title
                })
                .ToList();
            elasticArchiveRecord.FirstChildArchiveRecordId = archiveRecord.Display.FirstChildArchiveRecordId;
            elasticArchiveRecord.PreviousArchiveRecordId = archiveRecord.Display.PreviousArchiveRecordId;
            elasticArchiveRecord.NextArchiveRecordId = archiveRecord.Display.NextArchiveRecordId;
            elasticArchiveRecord.ParentArchiveRecordId = archiveRecord.Metadata.NodeInfo.ParentArchiveRecordId;
            elasticArchiveRecord.DisplayTemplateName = archiveRecord.Display.ExternalDisplayTemplateName;
            elasticArchiveRecord.LastSyncDate = DateTime.Now;
            elasticArchiveRecord.Facetten = new ElasticFacetten
            {
                // ToDo: Facetten create in IndexCustomScript.cs
            };
            elasticArchiveRecord.References = archiveRecord.Metadata.References.Select(s => new ElasticReference
                {
                    ArchiveRecordId = s.ArchiveRecordId,
                    ReferenceName = s.ReferenceName,
                    Role = s.Role
                })
                .ToList();
            elasticArchiveRecord.Containers = archiveRecord.Metadata.Containers.Container.Select(s => new ElasticContainer
                {
                    ContainerLocation = s.ContainerLocation,
                    ContainerType = s.ContainerType,
                    IdName = s.IdName,
                    ContainerCode = s.ContainerCode,
                    ContainerCarrierMaterial = s.ContainerCarrierMaterial
                })
                .ToList();
            elasticArchiveRecord.Descriptors = archiveRecord.Metadata.Descriptors.Select(s => new ElasticDescriptor
                {
                    Description = s.Description,
                    Function = s.Function,
                    IdName = s.IdName,
                    OtherLanguageNames = s.OtherLanguageNames,
                    Name = s.Name,
                    SeeAlso = s.SeeAlso,
                    Source = s.Source,
                    Thesaurus = s.Thesaurus,
                    SortingNumber = -1,
                    DateOfBirth = s.DateOfBirth.HasValue
                        ? new ElasticDateWithYear
                        {
                            Date = s.DateOfBirth.Value,
                            Year = s.DateOfBirth.Value.Year
                        }
                        : null,
                    DateOfDeath = s.DateOfDeath.HasValue
                        ? new ElasticDateWithYear
                        {
                            Date = s.DateOfDeath.Value,
                            Year = s.DateOfDeath.Value.Year
                        }
                        : null

                })
                .ToList();

            // If we receive data in the ElasticPrimaryData field, then we use this field. If not, then the PrimaryData field is used
            elasticArchiveRecord.PrimaryData = archiveRecord.ElasticPrimaryData != null && archiveRecord.ElasticPrimaryData.Any()
                ? archiveRecord.ElasticPrimaryData
                : archiveRecord.PrimaryData.ToElasticArchiveRecordPackage();

            TransferDataFromPropertyBag(elasticArchiveRecord, archiveRecord.Metadata.DetailData);
            
            // check if the field Metadata.Files has items and copy them to the Elastic record
            if (archiveRecord.Metadata.Files.Any())
            {
                var files = archiveRecord.Metadata.Files.Select(f => new ElasticArchiveRecordFile()
                {
                    Filename = GetFileNameForDownload(f.FileName, elasticArchiveRecord.ReferenceCode),
                    Description = f.Description,
                    Extension = f.FileExtension,
                    SizeInBytes = f.FileSize,
                    Base64Content = f.ContentText,
                    Art = f.Kind,
                    Version = f.SortOrder // SortOrder is used here to transport the version
                });
                elasticArchiveRecord.Files.AddRange(files);
                Log.Information($"Added {files.Count()} files to {nameof(elasticArchiveRecord)}");
            }

            // Add the creation period aggregation records
            // https://www.elastic.co/guide/en/elasticsearch/reference/current/search-aggregations-bucket-histogram-aggregation.html
            // According to elastic documentation histograms are calculated with this formula
            // bucket_key = Math.floor((value - offset) / interval) * interval + offset
            CalculateCreationPeriodBuckets(elasticArchiveRecord);
            return elasticArchiveRecord;
        }

        /// <summary>
        /// Calculate the Period how create the Record
        /// </summary>
        /// <param name="elasticArchiveRecord"></param>
        public static void CalculateCreationPeriodBuckets(ElasticArchiveRecord elasticArchiveRecord)
        {
            if (elasticArchiveRecord.CreationPeriod != null && elasticArchiveRecord.CreationPeriod.Years.Any())
            {
                elasticArchiveRecord.Facetten.CreationPeriodYears001 =
                    elasticArchiveRecord.CreationPeriod.Years;
                elasticArchiveRecord.Facetten.CreationPeriodYears005 =
                    elasticArchiveRecord.CreationPeriod.Years.Select(year => (int) Math.Floor(year / 5m) * 5).Distinct().ToList();
                elasticArchiveRecord.Facetten.CreationPeriodYears010 =
                    elasticArchiveRecord.CreationPeriod.Years.Select(year => (int) Math.Floor(year / 10m) * 10).Distinct().ToList();
                elasticArchiveRecord.Facetten.CreationPeriodYears025 =
                    elasticArchiveRecord.CreationPeriod.Years.Select(year => (int) Math.Floor(year / 25m) * 25).Distinct().ToList();
                elasticArchiveRecord.Facetten.CreationPeriodYears100 =
                    elasticArchiveRecord.CreationPeriod.Years.Select(year => (int) Math.Floor(year / 100m) * 100).Distinct().ToList();
            }
        }

        public void TransferDataFromPropertyBag(ElasticArchiveRecord elasticArchiveRecord, List<DataElement> detailData)
        {
           elasticArchiveRecord.DetailData = new List<ElasticDetailData>();
           var defaultProperties = elasticArchiveRecord.GetType().GetProperties();
            foreach (var fieldConfiguration in fieldsConfiguration.Fields)
            {
                try
                {
                    var value = GetValue(detailData, fieldConfiguration);

                    if ((fieldConfiguration.TargetField == "ReferenceCode" || fieldConfiguration.TargetField == "Title") &&
                        string.IsNullOrWhiteSpace(value as string))
                    {
                        value = "\u200A";   // Hair space
                    }

                    if (value != null && !(fieldConfiguration.Type == ElasticFieldTypes.TypeString && string.IsNullOrEmpty(value.ToString())))
                    {
                        if (fieldConfiguration.IsDefaultField)
                        {
                            var prop = defaultProperties.FirstOrDefault(d =>
                                d.Name.ToLowerInvariant() == fieldConfiguration.TargetField.ToLowerInvariant());
                            if (prop != null)
                            {
                                prop.SetValue(elasticArchiveRecord, value);
                            }
                        }
                        else
                        {
                            elasticArchiveRecord.DetailData.Add(ConvertToDetailData(detailData, fieldConfiguration));
                            if (fieldConfiguration.CopyTo_fieldKeywordValues)
                            {
                                elasticArchiveRecord.All_FieldKeywordValues += $"{value} ";
                            }

                            if (fieldConfiguration.CopyTo_fieldTextValues)
                            {
                                elasticArchiveRecord.All_FieldTextValues += $"{value} ";
                            }

                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex.Message);
                }
            }
        }

        private static ElasticDetailData ConvertToDetailData(List<DataElement> detailData, FieldConfiguration fieldConfiguration)
        {
            var elasticDetailData = new ElasticDetailData
            {
                ElementName = fieldConfiguration.TargetField,
                TypeName = fieldConfiguration.Type
            };

            switch (fieldConfiguration.Type)
            {
                case ElasticFieldTypes.TypeString:
                    var textExtractor = new TextExtractor();
                    elasticDetailData.TextValues = textExtractor.GetListValues(detailData, fieldConfiguration.ElementName);
                    break;
                case ElasticFieldTypes.TypeTimePeriod:
                    var timePeriodExtractor = new TimePeriodExtractor();
                    elasticDetailData.DateRangeValues = timePeriodExtractor.GetListValues(detailData, fieldConfiguration.ElementName);
                    break;
                case ElasticFieldTypes.TypeBase64:
                    var elasticBase64Extractor = new Base64Extractor();
                    elasticDetailData.BlobValues = elasticBase64Extractor.GetValue(detailData, fieldConfiguration.ElementName);
                    break;
                case ElasticFieldTypes.TypeInt + "?":
                case ElasticFieldTypes.TypeInt:
                    var intExtractor = new IntExtractor();
                    var intValue = intExtractor.GetValue(detailData, fieldConfiguration.ElementName);
                    // ReSharper disable once PossibleInvalidOperationException
                    elasticDetailData.Int64Values = intValue.Value;
                    break;
                case ElasticFieldTypes.TypeBool + "?":
                case ElasticFieldTypes.TypeBool:
                    var boolExtractor = new BoolExtractor();
                    var boolValue = boolExtractor.GetValue(detailData, fieldConfiguration.ElementName);
                    // ReSharper disable once PossibleInvalidOperationException
                    elasticDetailData.BoolValue = boolValue.Value;
                    break;
                case ElasticFieldTypes.TypeFloat + "?":
                case ElasticFieldTypes.TypeFloat:
                    var floatExtractor = new FloatExtractor();
                    var floatValue = floatExtractor.GetValue(detailData, fieldConfiguration.ElementName);
                    elasticDetailData.FloatValue = floatValue;
                    break;
                case ElasticFieldTypes.TypeHyperlink:
                    var hyperlinkExtractor = new HyperlinkExtractor();
                    elasticDetailData.HyperlinkValue = hyperlinkExtractor.GetValue(detailData, fieldConfiguration.ElementName);
                    break;
                case ElasticFieldTypes.TypeEntityLink:
                    var entityLinkExtractor = new EntityLinkExtractor();
                    elasticDetailData.EntityLinkValue = entityLinkExtractor.GetValue(detailData, fieldConfiguration.ElementName);
                    break;
                default:
                    throw new NotImplementedException($"Wrong ElasticFieldTypes: {fieldConfiguration.Type} ");
            }

            return elasticDetailData;
        }

        private object GetValue(List<DataElement> detailData, FieldConfiguration fieldConfiguration)
        {
            object retVal = null;
            try
            {
                switch (fieldConfiguration.Type)
                {
                    case ElasticFieldTypes.TypeString:
                        var textExtractor = new TextExtractor();
                        retVal = GetValue(detailData, fieldConfiguration, textExtractor);
                        break;
                    case ElasticFieldTypes.TypeTimePeriod:
                        var timePeriodExtractor = new TimePeriodExtractor();
                        retVal = GetValue(detailData, fieldConfiguration, timePeriodExtractor);
                        break;
                    case ElasticFieldTypes.TypeDateWithYear:
                        var dateWithYearExtractor = new DateWithYearExtractor();
                        retVal = GetValue(detailData, fieldConfiguration, dateWithYearExtractor);
                        break;
                    case ElasticFieldTypes.TypeBase64:
                        var elasticBase64Extractor = new Base64Extractor();
                        retVal = GetValue(detailData, fieldConfiguration, elasticBase64Extractor);
                        break;
                    case ElasticFieldTypes.TypeInt + "?":
                    case ElasticFieldTypes.TypeInt:
                        var intExtractor = new IntExtractor();
                        retVal = GetValue(detailData, fieldConfiguration, intExtractor);
                        break;
                    case ElasticFieldTypes.TypeBool + "?":
                    case ElasticFieldTypes.TypeBool:
                        var boolExtractor = new BoolExtractor();
                        retVal = GetValue(detailData, fieldConfiguration, boolExtractor);
                        break;
                    case ElasticFieldTypes.TypeFloat + "?":
                    case ElasticFieldTypes.TypeFloat:
                        var floatExtractor = new FloatExtractor();
                        retVal = GetValue(detailData, fieldConfiguration, floatExtractor);
                        break;
                    case ElasticFieldTypes.TypeHyperlink:
                        var hyperlinkExtractor = new HyperlinkExtractor();
                        retVal = GetValue(detailData, fieldConfiguration, hyperlinkExtractor);
                        break;
                    case ElasticFieldTypes.TypeEntityLink:
                        var entityLinkExtractor = new EntityLinkExtractor();
                        retVal = GetValue(detailData, fieldConfiguration, entityLinkExtractor);
                        break;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                throw;
            }

            return retVal;
        }

        private static object GetValue<T>(List<DataElement> detailData, FieldConfiguration fieldConfiguration, ExtractorBase<T> extractor)
        {
            object retVal;
            // The scope type memo is special in that text can be spread over multiple fields in 4000 char chunks. Thus we use the simple GetValue method
            // that stiches the data together in one field.
            if (detailData.First().ElementType == DataElementElementType.memo)
            {
                retVal = extractor.GetValue(detailData, fieldConfiguration.ElementName);
            }
            else
            {
                retVal = fieldConfiguration.IsRepeatable
                    ? (object) extractor.GetListValues(detailData, fieldConfiguration.ElementName)
                    : extractor.GetValue(detailData, fieldConfiguration.ElementName);
            }

            if (fieldConfiguration.IsDefaultField && retVal is List<string>)
            {
                retVal = string.Join(Environment.NewLine, (List<string>) retVal);
            }

            return retVal;
        }


        private static string GetFileNameForDownload(string name, string referenceCode)
        {
            var filename = $"{referenceCode}_{Regex.Match(name, ".{0,20}$").Value}";
            return string.Join("_", filename.Split(Path.GetInvalidFileNameChars()));
        }

        private static bool ContainsCustomFieldDigitaleVersion(dynamic customFields)
        {
            if (customFields is Dictionary<string, object> && ((IDictionary<string, object>)customFields).ContainsKey("digitaleVersion"))
            {
                var field =
                    ((IDictionary<string, object>)customFields).FirstOrDefault(k =>
                        k.Key.Equals("digitaleVersion", StringComparison.InvariantCultureIgnoreCase));
                if (field.Value != null && !string.IsNullOrWhiteSpace(field.Value.ToString()))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
