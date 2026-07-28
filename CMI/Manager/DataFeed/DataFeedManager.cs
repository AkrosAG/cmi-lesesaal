using CMI.Access.Sql.Lesesaal.EF;
using CMI.Contract.Common;
using CMI.Manager.DataFeed.SyncLog;
using Serilog;
using System;
using System.Threading.Tasks;

namespace CMI.Manager.DataFeed;

public class DataFeedManager : IDataFeedManager
{
    private readonly IDbSyncLogAccess dbSyncLogAccess;

    public DataFeedManager(IDbSyncLogAccess dbSyncLogAccess)
    {
        this.dbSyncLogAccess = dbSyncLogAccess;
    }

    public async Task HandleSyncRecordAsync(MutationRecord syncRecord)
    {
        try
        {
            var now = DateTime.Now;

            var syncAction = new SyncActionDto
            {
                ArchiveRecordId = syncRecord.ArchiveRecordId,
                ActionType = syncRecord.Action,
                ActionStatus = (int)ActionStatus.WaitingForSync,
                NumberOfTries = 0,
                CreatedOn = now,
                SyncActionLogs = [new() { ActionStatusHistory = nameof(ActionStatus.WaitingForSync), LogDate = DateTime.Now }]
            };

            await dbSyncLogAccess.InsertSyncActionAsync(syncAction);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Unexpected error while trying to insert record from ActaPro into the SyncAction table.");
            throw; // Causes automatic retry
        }
    }
}

