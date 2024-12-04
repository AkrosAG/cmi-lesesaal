using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CMI.Contract.Common;
using CMI.Contract.Harvest;

namespace CMI.Access.Harvest
{
    public interface IAISDataProvider : IDisposable
    {
        Task<List<MutationRecord>> GetPendingMutations();
        Task<NodeContext> LoadNodeContext(string recordId);
        Task<int> UpdateMutationStatus(MutationStatusInfo info);
        Task<int> BulkUpdateMutationStatus(List<MutationStatusInfo> infos);
        Task<int> ResetFailedSyncOperations(int maxRetries);
        Task<List<string>> LoadMetadataSecurityTokens(string recordId);
        Task<PrimaryDataSecurityTokenResult> LoadPrimaryDataSecurityTokens(string recordId);
        Task<int> InitiateFullResync();
        Task<HarvestStatusInfo> GetHarvestStatusInfo(QueryDateRange dataRange);
        Task<HarvestLogInfoResult> GetHarvestLogInfo(HarvestLogInfoRequest request);
        Task<List<FondLink>> LoadFondLinks();
        Task<string> GetBusinessObjectIdName(string recordId);
        Task<List<OrderDetailData>> GetChildrenRecordOrderDetailDataForArchiveRecord(string recordId);
        Task<List<OrderDetailData>> GetArchiveRecordOrderDetailDataForContainer(string containerId);
        Task<string> GetDbVersion();
        Task<OrderDetailData> LoadOrderDetailData(string recordId);
        Task<List<ContainerInfo>> LoadContainers(string recordId);
        Task<LinkedAccessionInfo> GetLinkedAccessionToArchiveRecord(string recordId);
        Task<string> GetAccessionBuilderName(string recordId);
    }
}