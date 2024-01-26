using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CMI.Contract.Common;
using CMI.Contract.Repository;

namespace CMI.Engine.PackageMetadata.Systems.Rosetta
{
    public class RosettaPackageHandler: IPackageHandler
    {
        public Task CreateMetadataXml(string folderName, RepositoryPackage package, List<RepositoryFile> filesToIgnore)
        {
            throw new NotImplementedException();
        }
    }
}
