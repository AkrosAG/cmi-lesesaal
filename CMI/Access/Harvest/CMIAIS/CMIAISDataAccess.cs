using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CMI.Contract.Common;
using CMI.Contract.Harvest;

namespace CMI.Access.Harvest.CMIAIS
{
    public class CMIAISDataAccess: IDbMutationQueueAccess
    {
        public Task<List<MutationRecord>> GetPendingMutations()
        {
            throw new NotImplementedException();
        }

        public Task<int> UpdateMutationStatus(MutationStatusInfo info)
        {
            throw new NotImplementedException();
        }

        public Task<int> BulkUpdateMutationStatus(List<MutationStatusInfo> infos)
        {
            throw new NotImplementedException();
        }

        public Task<int> ResetFailedSyncOperations(int maxRetries)
        {
            throw new NotImplementedException();
        }
    }
}
