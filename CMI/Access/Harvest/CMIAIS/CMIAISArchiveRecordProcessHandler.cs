using System;
using System.Threading.Tasks;
using CMI.Contract.Common;
using CMI.Contract.Common.Compiler;
using Serilog;

namespace CMI.Access.Harvest.CMIAIS
{
    public class CMIAISArchiveRecordProcessHandler : IArchiveRecordProcessHandler
    {
        private readonly IAISDataProvider dataProvider;
        private readonly IDynamicScriptProvider scriptProvider;

        public CMIAISArchiveRecordProcessHandler(IAISDataProvider dataProvider, IDynamicScriptProvider scriptProvider)
        {
            this.dataProvider = dataProvider;
            this.scriptProvider = scriptProvider;
        }

        public async Task PostProcessArchiveRecord(ArchiveRecord record)
        {
            try
            {
                var script = scriptProvider.GetInstanceByType<IDynamicScript>();
                script.PostProcessArchiveRecord(record);
            }
            catch (Exception e)
            {
                Log.Error(e, $"Cannot run the custom script for the archive record with id {record.ArchiveRecordId}.");
            }

            await Task.FromResult(record);
        }
    }
}
