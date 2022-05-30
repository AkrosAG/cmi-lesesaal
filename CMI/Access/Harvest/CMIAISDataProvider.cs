using CMI.Access.Harvest.ScopeArchiv.DataSets;
using CMI.Contract.Common;
using CMI.Contract.Harvest;
using System;
using System.Collections.Generic;

namespace CMI.Access.Harvest.CMIAIS
{

    public class CMIAISDataProvider : IAISDataProvider
    {
        public int BulkUpdateMutationStatus(List<MutationStatusInfo> infos)
        {
            throw new NotImplementedException();
        }

        public List<OrderDetailData> GetArchiveRecordOrderDetailDataForContainer(long containerId)
        {
            throw new NotImplementedException();
        }

        public string GetBusinessObjectIdName(long recordId)
        {
            throw new NotImplementedException();
        }

        public List<OrderDetailData> GetChildrenRecordOrderDetailDataForArchiveRecord(long recordId)
        {
            throw new NotImplementedException();
        }

        public string GetDbVersion()
        {
            throw new NotImplementedException();
        }

        public DetailDataDataSet.DetailDataDataTable GetDetailDataForElement(long recordId, int dataElementId)
        {
            throw new NotImplementedException();
        }

        public HarvestLogInfoResult GetHarvestLogInfo(HarvestLogInfoRequest request)
        {
            throw new NotImplementedException();
        }

        public HarvestStatusInfo GetHarvestStatusInfo(QueryDateRange dataRange)
        {
            throw new NotImplementedException();
        }

        public AccessionDataSet.AcessionRecordRow GetLinkedAccessionToArchiveRecord(long recordId)
        {
            throw new NotImplementedException();
        }

        public List<MutationRecord> GetPendingMutations()
        {
            throw new NotImplementedException();
        }

        public int InitiateFullResync()
        {
            throw new NotImplementedException();
        }

        public ArchivePlanInfoDataSet LoadArchivePlanInfo(long[] recordIdList)
        {
            throw new NotImplementedException();
        }

        public ContainerDataSet LoadContainers(long recordId)
        {
            throw new NotImplementedException();
        }

        public DescriptorDataSet LoadDescriptors(long recordId)
        {
            throw new NotImplementedException();
        }

        public DetailDataDataSet LoadDetailData(long recordId)
        {
            throw new NotImplementedException();
        }

        public List<FondLink> LoadFondLinks()
        {
            throw new NotImplementedException();
        }

        public List<string> LoadMetadataSecurityTokens(long recordId)
        {
            throw new NotImplementedException();
        }

        public NodeContext LoadNodeContext(long recordId)
        {
            throw new NotImplementedException();
        }

        public NodeInfoDataSet LoadNodeInfo(long recordId)
        {
            throw new NotImplementedException();
        }

        public OrderDetailData LoadOrderDetailData(long recordId)
        {
            throw new NotImplementedException();
        }

        public PrimaryDataSecurityTokenResult LoadPrimaryDataSecurityTokens(long recordId)
        {
            throw new NotImplementedException();
        }

        public ReferencesDataSet LoadReferences(long recordId)
        {
            throw new NotImplementedException();
        }

        public int ResetFailedSyncOperations(int maxRetries)
        {
            throw new NotImplementedException();
        }

        public int UpdateMutationStatus(MutationStatusInfo info)
        {
            throw new NotImplementedException();
        }
    }
}
