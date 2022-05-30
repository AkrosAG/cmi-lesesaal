using System.Collections.Generic;
using System.Threading.Tasks;

using CMI.Contract.Common;
using CMI.Contract.Harvest;

namespace CMI.Access.Harvest
{
    public interface IAISDataProvider
    {
        Task<List<MutationRecord>> GetPendingMutations();
        Task<NodeContext> LoadNodeContext(long recordId);
        Task<int> UpdateMutationStatus(MutationStatusInfo info);
        Task<int> BulkUpdateMutationStatus(List<MutationStatusInfo> infos);
        Task<int> ResetFailedSyncOperations(int maxRetries);
        Task<List<string>> LoadMetadataSecurityTokens(long recordId);
        Task<PrimaryDataSecurityTokenResult> LoadPrimaryDataSecurityTokens(long recordId);
        Task<int> InitiateFullResync();
        Task<HarvestStatusInfo> GetHarvestStatusInfo(QueryDateRange dataRange);
        Task<HarvestLogInfoResult> GetHarvestLogInfo(HarvestLogInfoRequest request);
        Task<List<FondLink>> LoadFondLinks();
        Task<string> GetBusinessObjectIdName(long recordId);
        Task<List<OrderDetailData>> GetChildrenRecordOrderDetailDataForArchiveRecord(long recordId);
        Task<List<OrderDetailData>> GetArchiveRecordOrderDetailDataForContainer(long containerId);
        Task<string> GetDbVersion();
        Task<OrderDetailData> LoadOrderDetailData(long recordId);
    }
}