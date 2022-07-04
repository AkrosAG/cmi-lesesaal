using CMI.Access.Common.Compiler;
using CMI.Contract.Common;

namespace CMI.Manager.Index
{
    public class ArchiveRecordProcessor : IArchiveRecordProcessor
    {
        private readonly IDynamicScriptProvider dynamicScriptProvider;  

        public ArchiveRecordProcessor(IDynamicScriptProvider provider)
        {
            dynamicScriptProvider = provider;
        }

        public void PostProcessArchiveRecord(ArchiveRecord archiveRecord)
        {
            var script = dynamicScriptProvider.GetInstanceByType<IDynamicScript>();
            script.PostProcessArchiveRecord(archiveRecord);
        }

        public void PostProcessElasticArchiveRecord(ElasticArchiveRecord elasticArchiveRecord, ArchiveRecord archiveRecord)
        {
            var script = dynamicScriptProvider.GetInstanceByType<IDynamicScript>();
            script.PostProcessElasticArchiveRecord(elasticArchiveRecord, archiveRecord);
        }
    }
}
