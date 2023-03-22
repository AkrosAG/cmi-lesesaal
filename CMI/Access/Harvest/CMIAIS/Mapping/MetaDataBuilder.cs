using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        archiveRecord.Metadata.DetailData = new List<DataElement>();
        var detailData = new DetailDataBuilder(cmiRecord, archiveRecord, this);
        return detailData;
    }

    public async Task<MetaDataBuilder> WithNodeInfos()
    {
        var childrenCount = cmiRecord.Children?.Count ?? 0;
        var ancestorsCount = cmiRecord.Ancestors?.Count ?? 0;

        archiveRecord.Metadata.NodeInfo = new NodeInfo
        {
            ChildCount = childrenCount,
            IsLeaf = childrenCount == 0,
            IsRoot = ancestorsCount == 0,
            Level = (int)(ancestorsCount > 0 ? cmiRecord.Ancestors!.Where(a => !a.TypeKey.Equals("Mandant", StringComparison.InvariantCultureIgnoreCase))
                .Max(a => a.Depth) + 1 : 0),
            ParentArchiveRecordId = ancestorsCount > 0 ? cmiRecord.Ancestors!.OrderBy(a => a.Depth).First().OBJ_GUID : null,
            Path = cmiRecord.Ancestors != null ? string.Join("", cmiRecord.Ancestors.
                Where(a => !a.TypeKey.Equals("Mandant", StringComparison.InvariantCultureIgnoreCase))
                .OrderByDescending(a => a.Depth).Select(a => a.OBJ_GUID)) + cmiRecord.OBJ_GUID : null,
            Sequence = await GetSequence(cmiRecord)
        };

        return this;
    }

    private async Task<long> GetSequence(Verzeichnungseinheit cmiRecord)
    {
        var parent = cmiRecord.Ancestors?.FirstOrDefault(a => a.Depth == 0);
        if (parent == null)
            return 0;

        var parentRecord = await archiveRecordMapperBuilder.cmiSpecificRecordAccess.GetAisSpecificRecord(parent.OBJ_GUID);
        var meAsChild = parentRecord?.Children.FirstOrDefault(c => c.OBJ_GUID == cmiRecord.OBJ_GUID);
        if (meAsChild == null)
            return 0;

        return long.TryParse(meAsChild.Sortierung, out var result) ? result : 0;
    }

    private ArchiveRecordMetadataUsageLicense GetLicense(Verzeichnungseinheit cmiRecord)
    {
        return (cmiRecord.Verwertungsrecht?.ToLowerInvariant() ?? "") switch
        {
            "gemeinfrei" => ArchiveRecordMetadataUsageLicense.CC0,// kein Copyright wenn möglich (Public domain) („no Copyright“)
            "extern" => ArchiveRecordMetadataUsageLicense.CCBYNCND,// Namensnennung, nicht kommerziell, keine Bearbeitung
            _ => ArchiveRecordMetadataUsageLicense.Undefined,
        };
    }

    public MetaDataBuilder WithUsageInfos()
    {
        archiveRecord.Metadata.Usage = new ArchiveRecordMetadataUsage
        {
            AlwaysVisibleOnline = StringComparer.InvariantCultureIgnoreCase.Compare(cmiRecord.Publikation, "sofort") == 0,
            ProtectionCategory = cmiRecord.Schutzfrist?.Item?.Bezeichnung,
            ProtectionBaseDate = cmiRecord.SchutzfristBasisdatum?.Text,
            ProtectionDuration =  (int?)cmiRecord.Schutzfrist?.Item?.Frist ?? 0, 
            ProtectionEndDate = cmiRecord.SchutzfristEnddatum?.Start,
            Permission = cmiRecord.Zugangsbestimmungen,
            PhysicalUsability = cmiRecord.PhysischeBeschaffenheit,
            Accessibility = cmiRecord.Zugangsbestimmungen,
            UsageNotes = cmiRecord.AllgemeineAnmerkungen,
            License = GetLicense(cmiRecord)
        };

        return this;
    }
}