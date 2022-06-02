using System;
using System.Threading.Tasks;
using CMI.Contract.Common;

namespace CMI.Access.Harvest.CMIAIS
{
    public class CMIAISArchiveRecordSecurityHandler : IArchiveRecordSecurityHandler
    {
        private readonly IAISDataProvider dataProvider;
        public CMIAISArchiveRecordSecurityHandler(IAISDataProvider dataProvider)
        {
            this.dataProvider = dataProvider;
        }

        public async Task ProcessArchiveRecord(ArchiveRecord record)
        {
            record.Security = new ArchiveRecordSecurity();
            await Task.FromResult(record);
        }
    }
}
