using System.Collections.Generic;
using System.Threading.Tasks;

using CMI.Contract.Common;

namespace CMI.Contract.Harvest
{
    public interface IDbMutationQueueAccess
    {
        /// <summary>
        ///     Gets the pending mutations from the AIS.
        /// </summary>
        /// <returns>A list with the records that need to be synced.</returns>
        Task<List<MutationRecord>> GetPendingMutations();
    }
}