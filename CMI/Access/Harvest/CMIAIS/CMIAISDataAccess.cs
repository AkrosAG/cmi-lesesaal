using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CMI.Contract.Common;
using CMI.Contract.Harvest;

namespace CMI.Access.Harvest.CMIAIS
{
    public class CMIAISDataAccess: IDbMutationQueueAccess, IDisposable
    {
        private readonly IAISDataProvider dataProvider;

        public CMIAISDataAccess(IAISDataProvider dataProvider)
        {
            this.dataProvider = dataProvider;
        }

        public async Task<List<MutationRecord>> GetPendingMutations()
        {
            return await dataProvider.GetPendingMutations(); 
        }

        public Task<int> UpdateMutationStatus(MutationStatusInfo info)
        {
            return dataProvider.UpdateMutationStatus(info);
        }

        public Task<int> BulkUpdateMutationStatus(List<MutationStatusInfo> infos)
        {
            return dataProvider.BulkUpdateMutationStatus(infos);
        }

        public Task<int> ResetFailedSyncOperations(int maxRetries)
        {
            // Nothing to do with CMI
            return Task.FromResult(0);
        }

        public void Dispose()
        {
            dataProvider?.Dispose();
        }
    }
}
