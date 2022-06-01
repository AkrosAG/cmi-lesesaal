using CMI.Access.Harvest.CMIAIS.Schemas;
using CMI.Contract.Common;
using CMI.Contract.Harvest;
using Serilog;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;


namespace CMI.Access.Harvest.CMIAIS
{
    public class CMIAISDataProvider : IAISDataProvider, IAISSpecificRecordAccess<Verzeichnungseinheit>
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

        public Task<LinkedAccessionInfo> GetLinkedAccessionToArchiveRecord(string recordId)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetAccessionBuilderName(string recordId)
        {
            throw new NotImplementedException();
        }

        public Task<List<OrderDetailData>> GetArchiveRecordOrderDetailDataForContainer(string containerId)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetBusinessObjectIdName(string recordId)
        {
            throw new NotImplementedException();
        }

        public Task<List<OrderDetailData>> GetChildrenRecordOrderDetailDataForArchiveRecord(string recordId)
        {
            throw new NotImplementedException();
        }

        public async Task<Verzeichnungseinheit> GetAisSpecificRecord(string id)
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
        
        public Task<List<ContainerInfo>> LoadContainers(string recordId)
        {
            throw new NotImplementedException();
        }

        public Task<List<FondLink>> LoadFondLinks()
        {
            throw new NotImplementedException();
        }

        public Task<List<string>> LoadMetadataSecurityTokens(string recordId)
        {
            throw new NotImplementedException();
        }

        public Task<NodeContext> LoadNodeContext(string recordId)
        {
            throw new NotImplementedException();
        }

        public Task<OrderDetailData> LoadOrderDetailData(string recordId)
        {
            throw new NotImplementedException();
        }

        public Task<PrimaryDataSecurityTokenResult> LoadPrimaryDataSecurityTokens(string recordId)
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
    }
}
