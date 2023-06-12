using System.Threading.Tasks;


namespace CMI.Access.Harvest.CMIAIS
{
    public interface IAISSpecificRecordAccess<T>
    {
        Task<T> GetAisSpecificRecord(string id);

        Task<Tektonik.Verzeichnungseinheit> GetTectonicRecord(string id);
    }
}
