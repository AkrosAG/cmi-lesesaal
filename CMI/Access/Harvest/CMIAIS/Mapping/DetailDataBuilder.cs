using CMI.Contract.Common;

namespace CMI.Access.Harvest.CMIAIS.Mapping;

public class DetailDataBuilder
{
    private readonly Verzeichnungseinheit cmiRecord;
    private readonly ArchiveRecord archiveRecord;
    private readonly MetaDataBuilder metaDataBuilder;

    public DetailDataBuilder(Verzeichnungseinheit cmiRecord, ArchiveRecord archiveRecord, MetaDataBuilder metaDataBuilder)
    {
        this.cmiRecord = cmiRecord;
        this.archiveRecord = archiveRecord;
        this.metaDataBuilder = metaDataBuilder;
 
    }

    public ElementDataBuilder WithMappings()
    {
        return new ElementDataBuilder(cmiRecord, archiveRecord.Metadata.DetailData, metaDataBuilder.archiveRecordMapperBuilder.languageSettings);
    }
}
