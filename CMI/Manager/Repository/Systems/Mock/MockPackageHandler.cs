using CMI.Contract.Repository;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CMI.Contract.Common;
using System.IO;
using Newtonsoft.Json;

namespace CMI.Manager.Repository.Systems.Mock
{
    public class MockPackageHandler : IPackageHandler
    {
        public Task CreateMetadataXml(string folderName, RepositoryPackage package, List<RepositoryFile> filesToIgnore)
        {
            return Task.CompletedTask;
        }
    }
}
