using CMI.Access.Harvest.CMIAIS.Schemas;
using CMI.Contract.Common;
using CMI.Contract.Harvest;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;


namespace CMI.Access.Harvest.CMIAIS
{
    public class CMIAISDataProvider : IAISDataProvider
    {
        private readonly HttpClient cdwsRequestClient;
        private readonly string[] indexNames;

        public CMIAISDataProvider()
        {
            var uri = new Uri(Properties.Settings.Default.CdwsSearchEndpoint);
            cdwsRequestClient = new HttpClient();
            cdwsRequestClient.BaseAddress = uri;

            indexNames = Properties.Settings.Default.CdwsIndexNames.Split(',');

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

        public async Task<List<MutationRecord>> GetPendingMutations()
        {
            var maxHits = 1000;
            var finalSequenceNr = 0L;
            var pendingMutations = new List<MutationRecord>();

            try
            {
                var lastSequenceNr = ReadLastSequenceNr();
                foreach (var indexName in indexNames)
                {
                    var skipRecords = 0;

                    // First, make a query to obtain the number of hits
                    var searchResponse = await GetChangeInfo(indexName, lastSequenceNr, skipRecords, maxHits);
                    var numHits = searchResponse.numHits;
                    finalSequenceNr = searchResponse.IDXSEQ;

                    // Now iterate to get all hits
                    do
                    {
                        Log.Information("Fetching {maxHits} records from CDWS for index {indexName}, skipping {skipRecords} records...", maxHits, indexName, skipRecords);
                        searchResponse = await GetChangeInfo(indexName, lastSequenceNr, skipRecords, maxHits);
                        foreach (var hit in searchResponse.Hit)
                        {
                            pendingMutations.Add(new MutationRecord
                            {
                                Action = "Update",
                                ArchiveRecordId = $"{indexName}.{hit.Guid}",
                                MutationId = hit.SEQ
                            });
                        }

                        skipRecords += maxHits;
                    } while (skipRecords <= numHits);
                }

                // If we have found changes, get the latest sequence Number from the result
                // Then make sanity check
                lastSequenceNr = pendingMutations.Any() ? pendingMutations.Max(m => m.MutationId) : finalSequenceNr;
                if (lastSequenceNr != finalSequenceNr)
                {
                    Log.Warning("Cdws returned {finalSequenceNr} but we found {lastSequenceNr}", finalSequenceNr, lastSequenceNr);
                }
                SaveLastSequenceNr(lastSequenceNr);

                return pendingMutations;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Fehler auslesen der Änderungen vom CDWS");
                return null;
            }
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
            //ToDo: Implement Table in sql db to watch status
            return Task.FromResult(0);
        }


        private long SaveLastSequenceNr(long sequenceNr)
        {
            var fileName = "lastSequenceNr.txt";
            File.WriteAllText(fileName, sequenceNr.ToString());

            return 0;
        }

        private long ReadLastSequenceNr()
        {
            var fileName = "lastSequenceNr.txt";
            if (File.Exists(fileName))
            {
                var num = File.ReadAllText(fileName);
                return Convert.ToInt64(num);
            }

            return 0;
        }

        private async Task<SearchResponseType> GetChangeInfo(string indexName, long lastSequenceNr, int currentPage, int maxHits)
        {
            var response = await cdwsRequestClient.GetAsync($"{indexName}/search?q=seq>{lastSequenceNr}&l=de-CH&s={currentPage}&m={maxHits}");
            response.EnsureSuccessStatusCode();

            var stringContent = await response.Content.ReadAsStringAsync();
            var searchResponse = SearchResponseType.Deserialize(stringContent);
            return searchResponse;
        }
    }
}
