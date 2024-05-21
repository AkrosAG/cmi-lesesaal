using CMI.Contract.Common;
using System.Threading.Tasks;

namespace CMI.Engine.PackageMetadata.Systems.Rosetta
{
    public interface IRepositoryPackageBuilder
    {
        Task<RepositoryPackage> BuildRepositoryPackageAsync(string archiveRecordId, string packageId);
        Task CreateMetadataXml(string archiveRecordId);
        void BuildZipFile(string archiveRecordId, string primaryDataLink);
    }
}
