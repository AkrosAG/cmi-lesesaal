using CMI.Contract.Common;
using CMI.Contract.Harvest;
using System.Threading.Tasks;

namespace CMI.Access.Harvest
{
    public partial class AISDataAccess : IDbStatusAccess
    {
        /// <summary>
        ///     Gets the detailed log information for the data harvesting.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>HarvestLogInfo.</returns>
        public async Task<HarvestLogInfoResult> GetLogInfo(HarvestLogInfoRequest request)
        {
            return await dataProvider.GetHarvestLogInfo(request);
        }

        /// <summary>
        ///     Gets the status information on how many records are waiting for sync, or are in sync.
        /// </summary>
        /// <param name="dateRange">A date range to analize</param>
        /// <returns>HarvestStatusInfo.</returns>
        public async Task<HarvestStatusInfo> GetStatusInfo(QueryDateRangeEnum dateRange)
        {
            return await dataProvider.GetHarvestStatusInfo(new QueryDateRange(dateRange));
        }
    }
}