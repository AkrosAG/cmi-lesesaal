using System.Collections.Generic;
using System.Threading.Tasks;
using CMI.Contract.Common;

namespace CMI.Manager.Repository.Systems;

public interface IRepositoryProvider
{
    Task<RepositoryPackageResult> GetPackage(string packageId, string archiveRecordId, bool createMetadataXml, List<string> fileTypesToIgnore, int primaerdatenAuftragId);

    RepositoryPackageInfoResult ReadPackageMetadata(string packageId, string archiveRecordId);
}