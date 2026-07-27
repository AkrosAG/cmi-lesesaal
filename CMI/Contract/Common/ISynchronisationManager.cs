using CMI.Contract.Common.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using CMI.Access.Sql.Lesesaal.EF;

namespace CMI.Contract.Common;

public interface ISynchronisationManager
{
    Task<List<SyncActionLogDto>> GetLogData(long syncActionId);
    Task<List<SyncActionDto>> GetSyncData(int filterId);
    Task<bool> BatchAddSyncActions(string[] ids, int action);
    Task<List<VSyncNumberPerHourDto>> GetSyncNumberPerHour(int days);
}