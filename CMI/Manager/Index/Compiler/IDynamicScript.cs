using CMI.Contract.Common;

namespace CMI.Manager.Index.Compiler
{
    public interface IDynamicScript
    {
        void PostProcessArchiveRecord(ArchiveRecord archiveRecord, ElasticArchiveRecord elasticArchiveRecord);
        void PostProcessElasticArchiveRecord(ArchiveRecord archiveRecord, ElasticArchiveRecord elasticArchiveRecord);
    }
}
