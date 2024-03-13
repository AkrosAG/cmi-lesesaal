using System.Threading.Tasks;

namespace CMI.Access.Repository.Systems.Rosetta
{
    public interface IRosettaDataAccess
    {
        public Task<bool> ExportIntellectualEntity(string defaultTempStoragePath, string entityId);
    }
}
