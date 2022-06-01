using System;
using System.Threading.Tasks;
using CMI.Contract.Common;

namespace CMI.Access.Harvest.CMIAIS
{
    public class ScopeArchiveRecordSecurityProvider : IArchiveRecordSecurityProvider
    {
        private readonly IAISDataProvider dataProvider;;
        public ScopeArchiveRecordSecurityProvider(IAISDataProvider dataProvider)
        {
            this.dataProvider = dataProvider;
        }
        public async Task<ArchiveRecordSecurity> GetArchiveRecordSecurity(int archiveRecordId)
        {
            throw new NotImplementedException();
        }
    }
}
