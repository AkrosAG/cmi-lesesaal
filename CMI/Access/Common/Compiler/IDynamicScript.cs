using CMI.Contract.Common;

namespace CMI.Access.Common.Compiler
{
    public interface IDynamicScript
    {
        void PostProcessArchiveRecord(ArchiveRecord archiveRecord);
        void PostProcessElasticArchiveRecord(ElasticArchiveRecord elasticArchiveRecord, ArchiveRecord archiveRecord);
    }
}
