using CMI.Contract.Common;
using System.Threading.Tasks;

namespace CMI.Manager.DataFeed
{
    public interface IDataFeedManager
    {
        Task HandleSyncRecordAsync(MutationRecord syncRecord);
    }
}