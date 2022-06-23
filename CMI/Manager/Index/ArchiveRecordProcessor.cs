using CMI.Contract.Common;
using CMI.Manager.Index.Compiler;

namespace CMI.Manager.Index
{
    public class ArchiveRecordProcessor : IArchiveRecordProcessor
    {
        private readonly IDynamicScriptProvider dynamicScriptProvider;  

        public ArchiveRecordProcessor(IDynamicScriptProvider provider)
        {
            dynamicScriptProvider = provider;
        }

        public void PostProcessArchiveRecord(ArchiveRecord archiveRecord, ElasticArchiveRecord elasticArchiveRecord)
        {
            var script = dynamicScriptProvider.GetInstanceByType<IDynamicScript>();
            script.PostProcessArchiveRecord(archiveRecord, elasticArchiveRecord);
        }

        public void PostProcessElasticArchiveRecord(ArchiveRecord archiveRecord, ElasticArchiveRecord elasticArchiveRecord)
        {
            var script = dynamicScriptProvider.GetInstanceByType<IDynamicScript>();
            script.PostProcessElasticArchiveRecord(archiveRecord, elasticArchiveRecord);
        }
    }
}
