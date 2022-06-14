using System.Threading.Tasks;

using CMI.Contract.Common;

namespace CMI.Manager.Index
{
    public interface IArchiveRecordProcessor
    {
        void PostProcessArchiveRecord(ArchiveRecord archiveRecord, ElasticArchiveRecord elasticArchiveRecord);
    }
}
