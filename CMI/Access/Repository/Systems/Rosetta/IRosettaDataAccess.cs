using System.Threading.Tasks;

namespace CMI.Access.Repository.Systems.Rosetta
{
    public interface IRosettaDataAccess
    {
        public Task<string> ExportIntellectualEntity(string entityId);
    }
}
