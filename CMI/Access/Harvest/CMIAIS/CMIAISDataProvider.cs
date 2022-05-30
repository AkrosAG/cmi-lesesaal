using CMI.Access.Harvest.CMIAIS.Schemas;
using CMI.Contract.Common;
using CMI.Contract.Harvest;
using Serilog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Net.Http;
using System.Threading.Tasks;


namespace CMI.Access.Harvest.CMIAIS
{
    public class CMIAISDataProvider : IAISDataProvider
    {
        private readonly HttpClient cdwsRequestClient;

        public CMIAISDataProvider()
        {
            var uri = new Uri(Properties.Settings.Default.CdwsSearchEndpoint);
            cdwsRequestClient = new HttpClient();
            cdwsRequestClient.BaseAddress = uri;
        }

        public Task<int> BulkUpdateMutationStatus(List<MutationStatusInfo> infos)
        {
            throw new NotImplementedException();
        }

        public Task<DataRow> GetLinkedAccessionToArchiveRecord(long recordId)
        {
            throw new NotImplementedException();
        }

        public Task<List<OrderDetailData>> GetArchiveRecordOrderDetailDataForContainer(long containerId)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetBusinessObjectIdName(long recordId)
        {
            throw new NotImplementedException();
        }

        public Task<List<OrderDetailData>> GetChildrenRecordOrderDetailDataForArchiveRecord(long recordId)
        {
            throw new NotImplementedException();
        }

        public async Task<Verzeichnungseinheit> GetCmiArchiveRecord(string id)
        {
            var response = await cdwsRequestClient.GetAsync($"searchdetails?q=obj_guid%20any%20{id}&l=de-CH");
            response.EnsureSuccessStatusCode();

            var stringContent = await response.Content.ReadAsStringAsync();
            try
            {
                var searchResponse = XMLConvert.FromXML<SearchDetailResponse>(stringContent);
                return searchResponse.Hit?.Verzeichnungseinheit;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Fehler beim Abholen des ArchiveRecords von CMI AIS {guid}", id);
                return null;
            }
        }

        public Task<string> GetDbVersion()
        {
            throw new NotImplementedException();
        }

        public Task<HarvestLogInfoResult> GetHarvestLogInfo(HarvestLogInfoRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<HarvestStatusInfo> GetHarvestStatusInfo(QueryDateRange dataRange)
        {
            throw new NotImplementedException();
        }

        public Task<List<MutationRecord>> GetPendingMutations()
        {
            throw new NotImplementedException();
        }

        public Task<int> InitiateFullResync()
        {
            throw new NotImplementedException();
        }
        
        public Task<DataSet> LoadContainers(long recordId)
        {
            throw new NotImplementedException();
        }

        public Task<List<FondLink>> LoadFondLinks()
        {
            throw new NotImplementedException();
        }

        public Task<List<string>> LoadMetadataSecurityTokens(long recordId)
        {
            throw new NotImplementedException();
        }

        public Task<NodeContext> LoadNodeContext(long recordId)
        {
            throw new NotImplementedException();
        }

        public Task<OrderDetailData> LoadOrderDetailData(long recordId)
        {
            throw new NotImplementedException();
        }

        public Task<PrimaryDataSecurityTokenResult> LoadPrimaryDataSecurityTokens(long recordId)
        {
            throw new NotImplementedException();
        }

        public Task<int> ResetFailedSyncOperations(int maxRetries)
        {
            throw new NotImplementedException();
        }

        public Task<int> UpdateMutationStatus(MutationStatusInfo info)
        {
            throw new NotImplementedException();
        }
        public Task<DataTable> GetDetailDataForElement(long recordId, int dataElementId)
        {
            throw new NotImplementedException();
        }
    }
}
