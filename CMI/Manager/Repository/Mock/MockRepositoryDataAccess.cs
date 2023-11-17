using CMI.Contract.Common;
using CMI.Contract.Repository;
using DotCMIS.Client;
using System.Collections.Generic;
using System.IO;

namespace CMI.Manager.Repository.Mock
{
    public class MockRepositoryDataAccess : IRepositoryDataAccess
    {
        public List<RepositoryFolder> GetFolders(string folderId)
        {
            // Wird nie aufgerufen im Mock
            throw new System.NotImplementedException();
        }

        public List<RepositoryFile> GetFiles(string folderId, List<string> filePatternsToIgnore, out List<RepositoryFile> ignoredFiles)
        {
            // Wird nie aufgerufen im Mock
            throw new System.NotImplementedException();
        }

        public RepositoryFolder GetRepositoryRoot(string packageId)
        {
            throw new System.NotImplementedException();
        }

        public IFolder GetCmisFolder(string folderId)
        { // Wird nie aufgerufen im Mock
            throw new System.NotImplementedException();
        }

        public Stream GetFileContent(string fileId)
        {
            // Wird nie aufgerufen im Mock
            throw new System.NotImplementedException();
        }

        public string GetRepositoryName()
        {
            return "Alfresco";
        }
    }
}
