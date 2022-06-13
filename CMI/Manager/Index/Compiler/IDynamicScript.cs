using CMI.Contract.Common;

namespace CMI.Manager.Index.Compiler
{
    public interface IDynamicScript
    {
        void Execute(ArchiveRecord archiveRecord, ElasticArchiveRecord elasticArchiveRecord);
    }
}
