using CMI.Contract.Common;
using CMI.Contract.Harvest;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.Caching;
using System.Threading.Tasks;
using CMI.Access.Sql.Lesesaal.EF;
using CMI.Contract.Common.Exceptions;


namespace CMI.Access.Harvest.CMIAIS
{
    public class CMIAISDataProvider : IAISDataProvider, IAISSpecificRecordAccess
    {
        private readonly LesesaalDb dbContext;
        private readonly MemoryCache cache;
        private readonly HttpClient cdwsRequestClient;
        private readonly string indexName;
        private readonly string indexTectonicName;

        public CMIAISDataProvider(LesesaalDb dbContext, MemoryCache cache)
        {
            this.dbContext = dbContext;
            this.cache = cache;
            var uri = new Uri(Properties.Settings.Default.CdwsEndpoint);
            cdwsRequestClient = new HttpClient();
            cdwsRequestClient.BaseAddress = uri;

            indexName = Properties.Settings.Default.CdwsIndexName;
            indexTectonicName = Properties.Settings.Default.CdwsTectonicIndexName;
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
                var existingAction = dbContext.SyncActions.FirstOrDefault(s => s.SyncActionId == info.MutationId);
                if (existingAction == null)
                {
                    var newAction = new SyncAction
                    {
                        ActionStatus = (int)info.NewStatus,
                        ActionType = info.MutationType,
                        ArchiveRecordId = info.ArchiveRecordId,
                        NumberOfTries = 0,
                        CreatedOn = DateTime.Now
                    };

                    var logEntry = new SyncActionLog
                    {
                        ActionStatusHistory = info.NewStatus.ToString(),
                        LogDate = DateTime.Now
                    };
                    newAction.SyncActionLogs.Add(logEntry);
                    dbContext.SyncActions.AddObject(newAction);
                }
                else
                {
                    existingAction.ActionStatus = (int)info.NewStatus;
                    existingAction.ModifiedOn = DateTime.Now;

                    // Add the log entry
                    var error = string.IsNullOrEmpty(info.ErrorMessage)
                        ? null
                        : info.ErrorMessage + Environment.NewLine + Environment.NewLine + info.StackTrace;
                    var logEntry = new SyncActionLog
                    {
                        ActionStatusHistory = info.NewStatus.ToString(),
                        LogDate = DateTime.Now,
                        ErrorReason = error
                    };
                    existingAction.SyncActionLogs.Add(logEntry);
                }
            }

            await dbContext.SaveChangesAsync();
            return infos.Count;
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
        
        public async Task<Tektonik.Verzeichnungseinheit> GetAisTectonicRecord(string id)
        {
            var url = $"{indexTectonicName}/searchdetails?q=obj_guid%20any%20{id}&l=de-CH";
            var response = await cdwsRequestClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var stringContent = await response.Content.ReadAsStringAsync();
            try
            {
                var searchResponse = XMLConvert.FromXML<SearchDetailResponseType>(stringContent);
                if (searchResponse.Hit.Any())
                {
                    var item = XMLConvert.FromXML<Tektonik.Verzeichnungseinheit>(searchResponse.Hit.First().Any.OuterXml);
                    return item;
                }

                throw new InvalidOperationException(
                    $"Record with id {id} does not exist in CDWS. Aborting sync of record in method {nameof(GetAisTectonicRecord)}.");
            }
            catch (Exception ex)
            {
                Log.Error("Fehler beim Abholen des ArchiveRecords von CMI AIS {indexTectonicName} {guid}. Fehler ist {Message}",
                    indexTectonicName, id, ex.Message);
                throw new AisTectonicRecordNotFoundException() {ArchiveRecordId = id};
            }
        }

        public async Task<Verzeichnungseinheit> GetAisDataRecord(string id, bool fetchArchivPlanContextRecord = false)
        {

            var cachedItem = cache.Get(id);
            if (cachedItem != null)
            {
                return (Verzeichnungseinheit)cachedItem;
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
                    cache.Add(id, item, new CacheItemPolicy { SlidingExpiration = TimeSpan.FromSeconds(120) });
                    return item;
                }

                throw new InvalidOperationException($"Record with id {id} does not exist in CDWS. Aborting sync of record in method {nameof(GetAisDataRecord)}.");
            }
            catch (Exception ex)
            {
                Log.Error("Fehler beim Abholen des ArchiveRecords von CMI AIS {indexName} {guid}. Fehler ist {Message}", indexName, id, ex.Message);
                if (fetchArchivPlanContextRecord)
                {
                    throw new AisParentRecordNotFoundException {ParentRecordId = id};
                }

                throw new AisRecordNotFoundException {ArchiveRecordId = id};
            }
        }
        
        public Task<string> GetDbVersion()
        {
            var response = cdwsRequestClient.GetAsync($"{indexName}/getChanges?seq=0&m={1}").Result;
           
          
            if (response.IsSuccessStatusCode)
            {
                return Task.FromResult($"{response.Version} IndexName: {indexName}, letzte Seq Nummer: {ReadLastSequenceNr()}");
            }

            throw new InvalidOperationException($"AIS not work. StatusCode: {response.StatusCode}");
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


                // Action 0 Waiting for Sync
                // We are taking a max of 20'000 to prevent message size overflow error in RabbitMq
                // At the current rate of getting the pending mutation once every hour this is sufficient
                // as the system cannot process more than 20'000 syncs in an hour.
                // Or if this shouldn't be enough, then we can reduce the time for the getPendingMutations job.
                // No more than 20,000 per hour will fail, and the process is slightly different from Viaduc.
                var result = dbContext.SyncActions.Where(x => x.ActionStatus == 0).Take(20000);
                Log.Information("Fetching {Count} records from DataTable SyncActions where ActionStatus 0", result.Count());
                foreach (var syncAction in result)
                {
                    pendingMutations.Add(new MutationRecord
                    {
                        Action = syncAction.ActionType,
                        ArchiveRecordId = syncAction.ArchiveRecordId,
                        MutationId = syncAction.SyncActionId
                    });
                }

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

        public async Task<int> ResetFailedSyncOperations(int maxRetries)
        {
            var recordsToReset = dbContext.SyncActions.Where(s => s.ActionStatus == (int)ActionStatus.SyncFailed &&
                                                                  s.NumberOfTries < maxRetries);

            foreach (var syncAction in recordsToReset)
            {
                syncAction.ActionStatus = 0;
                // Add the log entry
                var logEntry = new SyncActionLog
                {
                    SyncActionId = syncAction.SyncActionId,
                    ActionStatusHistory = nameof(ActionStatus.WaitingForSync),
                    LogDate = DateTime.Now
                };
                syncAction.ModifiedOn = DateTime.Now;
                syncAction.SyncActionLogs.Add(logEntry);
            }

            return await dbContext.SaveChangesAsync();
        }

        public async Task<int> UpdateMutationStatus(MutationStatusInfo info)
        {
            try
            {
                var record = dbContext.SyncActions.FirstOrDefault(s => s.SyncActionId == info.MutationId &&
                                                                       // If the status update is only allowed from a specific existing status, 
                                                                       // add the required where clause.
                                                                       (info.ChangeFromStatus.HasValue
                                                                        ? s.ActionStatus == (int) info.ChangeFromStatus.Value
                                                                        : s.ActionStatus > 0));
                if (record != null)
                {
                    record.ActionStatus = (int) info.NewStatus;
                    record.NumberOfTries++;
                    record.ModifiedOn = DateTime.Now;

                    // Add the a log entry
                    var error = string.IsNullOrEmpty(info.ErrorMessage)
                        ? null
                        : info.ErrorMessage + Environment.NewLine + Environment.NewLine + info.StackTrace;
                    var logEntry = new SyncActionLog()
                    {
                        SyncActionId = info.MutationId,
                        ActionStatusHistory = info.NewStatus.ToString(),
                        LogDate = DateTime.Now,
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
            var info = dbContext.SyncInfos.FirstOrDefault();
            if (info == null)
            {
                info = new SyncInfo();
                dbContext.AddToSyncInfos(info);
            }

            info.LastSequenceNumber = sequenceNr;
            dbContext.SaveChanges();
        }

        private long ReadLastSequenceNr()
        {
            var info = dbContext.SyncInfos.FirstOrDefault();
            if (info != null)
            {
                return info.LastSequenceNumber ?? 0;
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
