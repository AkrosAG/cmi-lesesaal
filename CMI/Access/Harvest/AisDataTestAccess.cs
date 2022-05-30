using CMI.Contract.Harvest;
using System.Threading.Tasks;

namespace CMI.Access.Harvest
{
    public partial class AISDataAccess : IDbTestAccess
    {
        public async Task<string> GetDbVersion()
        {
            return await dataProvider.GetDbVersion();
        }
    }
}