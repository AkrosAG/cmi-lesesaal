using System.Data;
using System.Threading.Tasks;

using CMI.Contract.Common;

namespace CMI.Access.Harvest
{
    public interface IArchiveRecordDisplayProvider
    {
        Task<ArchiveRecordDisplay> GetDisplayData(string archiveRecordId);
    }
}
