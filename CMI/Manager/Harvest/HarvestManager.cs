using CMI.Contract.Common;
using CMI.Contract.Harvest;
using Serilog;
using System.Threading.Tasks;

namespace CMI.Manager.Harvest
{
    public class HarvestManager : IHarvestManager
    {
        private readonly IDbMetadataAccess dbAccess;
        private readonly IDbMutationQueueAccess queueAccess;
        private readonly IDbResyncAccess resyncAccess;
        private readonly IDbStatusAccess statusAccess;

        public HarvestManager(IDbMetadataAccess dbAccess, IDbMutationQueueAccess queueAccess, IDbResyncAccess resyncAccess,
            IDbStatusAccess statusAccess)
        {
            this.dbAccess = dbAccess;
            this.queueAccess = queueAccess;
            this.resyncAccess = resyncAccess;
            this.statusAccess = statusAccess;
        }

        /// <summary>
        ///     Creates an ArchiveRecord reading data from the AIS
        ///     The data structure contains all required information for
        ///     indexing and displaying the record in a web application.
        /// </summary>
        /// <param name="archiveRecordId">The id of the archive id in the AIS</param>
        /// <returns></returns>
        public async Task<ArchiveRecord> BuildArchiveRecord(string archiveRecordId)
        {
            return await dbAccess.GetArchiveRecord(archiveRecordId);
        }

        /// <summary>
        ///     Updates the mutation status in the mutation table.
        /// </summary>
        /// <param name="info">Object with information about the change.</param>
        /// <returns>Task.</returns>
        public int UpdateMutationStatus(MutationStatusInfo info)
        {
            // TODO: Review
            return queueAccess.UpdateMutationStatus(info).GetAwaiter().GetResult();
        }

        /// <summary>
        ///     Initiates a full resync of all archive records.
        /// </summary>
        /// <param name="info">Information about who and when the request was sent.</param>
        /// <returns>Number of added records to the mutation table</returns>
        public int InitiateFullResync(ResyncRequestInfo info)
        {
            Log.Information("About to insert record ids into mutation table for full resync. Command was started by {username} at {startTime}",
                info.Username, info.IssueDate);
            // TODO: Review
            var affectedRecords = resyncAccess.InitiateFullResync(info).GetAwaiter().GetResult();
            Log.Information("Finished inserting record ids into mutation table. A total of {affectedRecords} were added.", affectedRecords);
            return affectedRecords;
        }

        /// <summary>
        ///     Gets the status information on how many records are waiting for sync, or are in sync.
        /// </summary>
        /// <param name="dateRange">A date range to analize</param>
        /// <returns>HarvestStatusInfo.</returns>
        public HarvestStatusInfo GetStatusInfo(QueryDateRangeEnum dateRange)
        {
            // TODO: Review
            return statusAccess.GetStatusInfo(dateRange).GetAwaiter().GetResult();
        }

        /// <summary>
        ///     Gets the detailed log information for the data harvesting.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>HarvestLogInfo.</returns>
        public HarvestLogInfoResult GetLogInfo(HarvestLogInfoRequest request)
        {
            // TODO: Review
            return statusAccess.GetLogInfo(request).GetAwaiter().GetResult();
        }
    }
}