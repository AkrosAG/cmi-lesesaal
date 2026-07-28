using CMI.Contract.Common;
using CMI.Contract.Harvest;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CMI.Access.Harvest.CMIAIS
{
    public class CMIAISDataAccess: IDbMutationQueueAccess
    {
        private readonly IAISDataProvider dataProvider;

        public CMIAISDataAccess(IAISDataProvider dataProvider)
        {
            this.dataProvider = dataProvider;
        }

        public async Task<List<MutationRecord>> GetPendingMutations()
        {
            return await dataProvider.GetPendingMutations(); 
        }
    }
}
