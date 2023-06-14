using System.Threading.Tasks;


namespace CMI.Access.Harvest.CMIAIS
{
    public interface IAISSpecificRecordAccess
    {
        Task<Tektonik.Verzeichnungseinheit> GetAisTectonicRecord(string id);
        Task<Verzeichnungseinheit> GetAisDataRecord(string id);
    }
}
