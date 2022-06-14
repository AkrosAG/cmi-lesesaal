using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            script.Execute(archiveRecord, elasticArchiveRecord);
        }
    }
}
