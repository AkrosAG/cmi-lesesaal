using CMI.Contract.Common;
using CMI.Contract.Harvest;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.Caching;
using System.Threading.Tasks;
using CMI.Access.Sql.Lesesaal.EF;


namespace CMI.Access.Harvest.CMIAIS
{
    public class CMIAISDataProvider : IAISDataProvider, IAISSpecificRecordAccess<Verzeichnungseinheit>
    {
        private readonly LesesaalDb dbContext;
        private readonly MemoryCache cache;
        private readonly HttpClient cdwsRequestClient;
        private readonly string indexName;

        public CMIAISDataProvider(LesesaalDb dbContext, MemoryCache cache)
        {
            this.dbContext = dbContext;
            this.cache = cache;
            var uri = new Uri(Properties.Settings.Default.CdwsEndpoint);
            cdwsRequestClient = new HttpClient();
            cdwsRequestClient.BaseAddress = uri;

            indexName = Properties.Settings.Default.CdwsIndexName;
        }

        public async Task<int> BulkUpdateMutationStatus(List<MutationStatusInfo> infos)
        {
            // Target status must be the same for all elements
            var statusGroup = infos.GroupBy(g => g.NewStatus).ToList();
            if (statusGroup.Count > 1)
            {
                throw new ArgumentException("All elements in the list must have the same NewStatus value.");
            }

            foreach (var info in infos)
            {
                var newAction = new SyncAction
                {
                    SyncActionId = info.MutationId,
                    ActionStatus = (int) info.NewStatus,
                    ActionType = info.MutationType,
                    ArchiveRecordId = info.ArchiveRecordId,
                    NumberOfTries = 0,
                };
                dbContext.SyncActions.AddObject(newAction);
                await dbContext.SaveChangesAsync();
            }

            return await Task.FromResult(infos.Count);
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
            var cachedItem = cache.Get(id);
            if (cachedItem != null)
            {
                return cachedItem as Verzeichnungseinheit;
            }

            // No cache, then fetch it
            var url = $"{indexName}/searchdetails?q=obj_guid%20any%20{id}&l=de-CH";
            var response = await cdwsRequestClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var stringContent = await response.Content.ReadAsStringAsync();
            try
            {
                var searchResponse = XMLConvert.FromXML<SearchDetailResponseType>(stringContent);
                if (searchResponse.Hit.Any())
                {
                    var item = XMLConvert.FromXML<Verzeichnungseinheit>(searchResponse.Hit.First().Any.OuterXml);
                    cache.Add(id, item, new CacheItemPolicy {SlidingExpiration = TimeSpan.FromSeconds(120)});
                    return item;
                }

                throw new InvalidOperationException($"Record with id {id} does not exist in CDWS. Aborting sync of record in method {nameof(GetAisSpecificRecord)}.");
            }
            catch (Exception ex)
            {
                Log.Error("Fehler beim Abholen des ArchiveRecords von CMI AIS {guid}. Fehler ist {Message}", id, ex.Message);
                throw;
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
            var pendingMutations = new List<MutationRecord>();

            try
            {
                var lastSequenceNr = ReadLastSequenceNr();
                // First, make a query to obtain the number of hits. For that we just need to fetch one item
                var searchResponse = await GetChangeInfo(indexName, lastSequenceNr, 1);
                var finalSequenceNr = searchResponse.IDXSEQ;

                // Now iterate to get all hits
                do
                {
                    Log.Information("Fetching {maxHits} records from CDWS for index {indexName}, using seq = {lastSequenceNr} records...", maxHits, indexName, lastSequenceNr);
                    searchResponse = await GetChangeInfo(indexName, lastSequenceNr, maxHits);
                    
                    foreach (var change in searchResponse.Change)
                    {
                        pendingMutations.Add(new MutationRecord
                        {
                            Action = change.Action.ToLower() == "delete" ? "Delete" : "Update",
                            ArchiveRecordId = change.Guid,
                            MutationId = change.SEQ
                        });
                    }

                    // Fetch the latest sequence number from this batch
                    if (searchResponse.Change.Any())
                    {
                        lastSequenceNr = searchResponse.Change.Last().SEQ;
                    }
                } while (lastSequenceNr < finalSequenceNr);
                

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
            return Task.FromResult(0);
        }

        public async Task<int> UpdateMutationStatus(MutationStatusInfo info)
        {
            try
            {
                var record = dbContext.SyncActions.FirstOrDefault(s => s.SyncActionId == info.MutationId &&
                                                                       // If the status udpate is only allowed from a specific existing status, 
                                                                       // add the required where clause.
                                                                       info.ChangeFromStatus.HasValue
                                                                        ? s.ActionStatus == (int) info.ChangeFromStatus.Value
                                                                        : s.ActionStatus > 0);
                if (record != null)
                {
                    record.ActionStatus = (int) info.NewStatus;
                    record.NumberOfTries++;

                    // Add the a log entry
                    var error = string.IsNullOrEmpty(info.ErrorMessage)
                        ? null
                        : info.ErrorMessage + Environment.NewLine + Environment.NewLine + info.StackTrace;
                    var logEntry = new SyncActionLog()
                    {
                        SyncActionId = info.MutationId,
                        ActionStatusHistory = info.NewStatus.ToString(),
                        ErrorReason = error
                    };
                    record.SyncActionLogs.Add(logEntry);

                    return await dbContext.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, ex.Message);
                throw;
            }

            return 0;
        }


        private void SaveLastSequenceNr(long sequenceNr)
        {
            var fileName = "lastSequenceNr.txt";
            File.WriteAllText(fileName, sequenceNr.ToString());
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

        private async Task<ChangesResponseType> GetChangeInfo(string cdwsIndexName, long lastSequenceNr, int maxHits)
        {
            var response = await cdwsRequestClient.GetAsync($"{cdwsIndexName}/getChanges?seq={lastSequenceNr}&m={maxHits}");
            response.EnsureSuccessStatusCode();

            var stringContent = await response.Content.ReadAsStringAsync();
            var searchResponse = ChangesResponseType.Deserialize(stringContent);
            return searchResponse;
        }
    }
}
