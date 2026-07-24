using CMI.Contract.Common;
using System.Collections.Generic;
using System.Threading.Tasks;
using CMI.Contract.Harvest;

namespace CMI.Access.Sql.Lesesaal.EF;

public interface ILesesaalDataProvider
{
    Task<List<MutationRecord>> GetPendingMutations();
    Task<int> UpdateMutationStatus(MutationStatusInfo info);
    Task<int> BulkUpdateMutationStatus(List<MutationStatusInfo> infos);
    Task<int> ResetFailedSyncOperations(int maxRetries);
    Task InsertSyncAction(SyncActionDto syncActionDto);
    Task DeleteOldSyncActionAsync(int daysAgo);

}