using CMI.Contract.Common;
using CMI.Contract.Repository;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CMI.Engine.PackageMetadata.Systems.Rosetta
{
    public class RosettaPackageHandler : IPackageHandler
    {
        private readonly IRepositoryPackageBuilder builder;


        public RosettaPackageHandler(IRepositoryPackageBuilder builder)
        {
            this.builder = builder;
        }

        public async Task CreateMetadataXml(string folderName, RepositoryPackage package, List<RepositoryFile> filesToIgnore)
        {
            await builder.CreateMetadataXml(package.ArchiveRecordId);
        }
    }

}
