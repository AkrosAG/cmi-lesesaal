using System.Collections.Generic;
using System.Threading.Tasks;

namespace CMI.Access.Sql.Lesesaal.EF;

public interface ISynchronisationAccess
{
    LesesaalDb Context { get; }
    Task<List<SyncActionLogDto>> LogData(long? id);

    Task<List<SyncActionDto>> SyncData(int filterId);
    Task<List<VSyncNumberPerHourDto>> SyncNumberPerHour(int days);

}