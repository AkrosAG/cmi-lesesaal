using System;
using System.Threading.Tasks;
using CMI.Contract.Common;

namespace CMI.Access.Harvest.CMIAIS
{
    public class CMIAISArchiveRecordSecurityProvider : IArchiveRecordSecurityProvider
    {
        private readonly IAISDataProvider dataProvider;
        public CMIAISArchiveRecordSecurityProvider(IAISDataProvider dataProvider)
        {
            this.dataProvider = dataProvider;
        }
        public async Task<ArchiveRecordSecurity> GetArchiveRecordSecurity(string archiveRecordId)
        {
            // TODO: Review
            return await Task.FromResult(new ArchiveRecordSecurity());
        }
    }
}
