
namespace CMI.Manager.Repository.Systems.Rosetta.Schema
{
    public class Section
    {
        public const string GeneralFileCharacteristics = "generalFileCharacteristics";
        public const string InternalIdentifier = "internalIdentifier";
        public const string GeneralRepCharacteristics = "generalRepCharacteristics";
        public const string ObjectCharacteristics = "objectCharacteristics";
        public const string FileFixity = "fileFixity";
        public const string VsOutcome = "vsOutcome";
        public const string FileFormat = "fileFormat";
        public const string FileVirusCheck = "fileVirusCheck";
        public const string Event= "event";
        public const string FileTechnicalMetadataExtraction = "fileTechnicalMetadataExtraction";
        public const string FileValidation = "fileValidation";
        public const string SignificantProperties = "significantProperties";
        public const string GeneralIeCharacteristics = "generalIECharacteristics";
        public const string RetentionPolicy = "retentionPolicy";
        public const string AccessRightsPolicy = "accessRightsPolicy";
        public const string Producer = "producer";
        public const string ProducerAgent = "producerAgent";

    }

    public class SectionProducerAgent
    {
        public const string FirstName = "firstName";
        public const string LastName = "lastName";
    }

    public class SectionAccessRightsPolicy
    {
        public const string PolicyId = "policyId";
    }

    public class SectionRetentionPolicy
    {
        public const string PolicyId = "policyId";
        public const string PolicyDescription = "policyDescription";
    }

    public class SectionGeneralIeCharacteristics
    {
        public const string Status = "status";
        public const string Version = "Version";
    }

    public class SectionSignificantProperties
    {
        public const string SignificantPropertiesType = "significantPropertiesType";
        public const string SignificantPropertiesValue = "significantPropertiesValue";
    }

    public class SectionFileValidation
    {
        public const string PluginName = "pluginName";
        public const string Agent = "agent";
        public const string IsValid = "isValid";
        public const string IsWellFormed = "isWellFormed";
        public const string Format = "format";
        public const string Version = "version";
        public const string MimeType = "mimeType";
    }

    public class SectionFileTechnicalMetadataExtraction
    {
        public const string PluginName = "pluginName";
        public const string Agent = "agent";
    }

    public class SectionProducer
    {
        public const string Address1 = "address1";
        public const string Address2 = "address2";
        public const string Address4 = "address4";
        public const string DefaultLanguage = "defaultLanguage";
        public const string EmailAddress = "emailAddress";
        public const string FirstName = "firstName";
        public const string LastName = "lastName";
        public const string Telephone1 = "telephone1";
        public const string AuthorativeName = "authorativeName";
        public const string ProducerId = "producerId";
        public const string UserIdAppId = "userIdAppId";
        public const string Zip = "zip";
    }

    public class SectionObjectCharacteristics
    {
        public const string ObjectType = "objectType";
        public const string CreationDate = "creationDate";
        public const string CreatedBy = "createdBy";
        public const string ModificationDate = "modificationDate";
        public const string ModifiedBy = "modifiedBy";
        public const string Owner = "owner";
    }

    public class SectionInternalIdentifier
    {
        public const string InternalIdentifierType = "internalIdentifierType";
        public const string InternalIdentifierValue = "internalIdentifierValue";
    }

    public class SectionFileFixity
    {
        public const string Agent = "agent";
        public const string FixityType = "fixityType";
        public const string FixityValue = "fixityValue";
    }

    public class SectionVsOutcome
    {
        public const string CheckDate = "checkDate";
        public const string Type = "type";
        public const string VsAgent = "vsAgent";
        public const string Result = "result";
        public const string VsEvaluation = "vsEvaluation";
    }

    public class SectionFileFormat
    {
        public const string Agent = "agent";
        public const string FormatRegistry = "formatRegistry";
        public const string FormatRegistryId = "formatRegistryId";
        public const string FormatName = "formatName";
        public const string FormatVersion = "formatVersion";
        public const string FormatDescription = "formatDescription";
        public const string FormatStatus = "formatStatus";
        public const string ExactFormatIdentification = "exactFormatIdentification";
        public const string MimeType = "mimeType";
        public const string AgentVersion = "agentVersion";
        public const string AgentSignatureVersion = "agentSignatureVersion";
        public const string IdentificationMethod = "identificationMethod";
        public const string FormatLibraryVersion = "formatLibraryVersion";
    }

    public class SectionGeneralFileCharacteristics
    {
        public const string Label = "label";
        public const string FileModificationDate = "fileModificationDate";
        public const string FileLocationType = "fileLocationType";
        public const string FileOriginalName = "fileOriginalName";
        public const string FileOriginalPath = "fileOriginalPath";
        public const string FileOriginalID = "fileOriginalID";
        public const string FileExtension = "fileExtension";
        public const string FileMIMEType = "fileMIMEType";
        public const string FileSizeBytes = "fileSizeBytes";
        public const string FormatLibraryId = "formatLibraryId";
    }

    public class SectionFileVirusCheck
    {
        public const string Status = "status";
        public const string Agent = "agent";
        public const string Content = "content";
    }

    public class SectionEvent
    {
        public const string EventDateTime = "eventDateTime";
        public const string EventType = "eventType";
        public const string EventIdentifierType = "eventIdentifierType";
        public const string EventIdentifierValue = "eventIdentifierValue";
        public const string EventOutcome1 = "eventOutcome1";
        public const string EventDescription = "eventDescription";
        public const string LinkingAgentIdentifierType1 = "linkingAgentIdentifierType1";
        public const string LinkingAgentIdentifierValue1 = "linkingAgentIdentifierValue1";
        public const string EventOutcomeDetail1 = "eventOutcomeDetail1";
    }


}