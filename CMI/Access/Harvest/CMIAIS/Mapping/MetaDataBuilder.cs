using CMI.Contract.Common;

namespace CMI.Access.Harvest.CMIAIS.Mapping;

public class MetaDataBuilder
{
    private readonly Verzeichnungseinheit cmiRecord;
    private readonly ArchiveRecord archiveRecord;
    internal readonly ArchiveRecordMapperBuilder archiveRecordMapperBuilder;

    public MetaDataBuilder(Verzeichnungseinheit cmiRecord, ArchiveRecord archiveRecord, ArchiveRecordMapperBuilder archiveRecordMapperBuiler)
    {
        this.cmiRecord = cmiRecord;
        this.archiveRecord = archiveRecord;
        archiveRecordMapperBuilder = archiveRecordMapperBuiler;
    }

    public DetailDataBuilder AddDetailData()
    {
        var detailData = new DetailDataBuilder(cmiRecord, archiveRecord, this);
        return detailData;
    }

    public MetaDataBuilder WithNodeInfos()
    {
        var childrenCount = cmiRecord.Children?.Length ?? 0;
        var ancestorsCount = cmiRecord.Ancestors?.Length ?? 0;

        archiveRecord.Metadata.NodeInfo = new NodeInfo
        {
            ChildCount = childrenCount,
            IsLeaf = childrenCount == 0,
            IsRoot = ancestorsCount == 0,
            Level = (int)(ancestorsCount > 0 ? cmiRecord.Ancestors[0].Depth + 1 : 0),
            ParentArchiveRecordId = ancestorsCount > 0 ? cmiRecord.Ancestors[cmiRecord.Ancestors.Length - 1].OBJ_GUID : null,
            Path = cmiRecord.Tektonikpfad,
            Sequence = 0, // ToDo: Kennen wir nicht?
        };

        return this;
    }

    public MetaDataBuilder WithUsageInfos()
    {
        archiveRecord.Metadata.Usage = new ArchiveRecordMetadataUsage
        {
            AlwaysVisibleOnline = true, // ToDo: Validate
            IsPhysicalyUsable = true, // ToDo: Mapping
            ContainsPersonRelatedData = false, // ToDo: Mapping
            ProtectionCategory = null, // ToDo: Mapping
            ProtectionBaseDate = null, // ToDo: Mapping im CDWS
            ProtectionDuration = null, // ToDo: Mapping im CDWS
            ProtectionEndDate = null, // ToDo: Mapping im CDWS,
            ProtectionNote = null, // ToDo: Validate, Führen wir nicht?
            CannotFallBelow = true, // ToDo: Validate, Führen wir nicht?
            AdjustedManually = false, // ToDo: Validate, Führen wir nicht
            Permission = cmiRecord.Zugangsbestimmungen, // ToDo: Validate,
            PhysicalUsability = cmiRecord.PhysischeBeschaffenheit, // ToDo: Validate
            Accessibility = cmiRecord.Zugangsbestimmungen, // ToDo: Validate
            UsageNotes = null, // ToDo: Mapping
            License = ArchiveRecordMetadataUsageLicense.Undefined, // ToDo: Mapping
        };

        return this;
    }


    public ArchiveRecordMapperBuilder EndMetaData()
    {
        return archiveRecordMapperBuilder;
    }

}