using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using CMI.Contract.Common;
using CMI.Contract.Repository;
using DotCMIS.Client;
using DotCMIS.Data.Extensions;
using Newtonsoft.Json;
using Serilog;

namespace CMI.Access.Repository.Systems.Dir
{
    public class DirRepositoryDataAccess : IDirRepositoryDataAccess
    {
        private readonly IDirRepositoryConnectionFactory connectionFactory;
        private readonly IDirMetadataDataAccess dirMetadataAccess;

        public DirRepositoryDataAccess(IDirRepositoryConnectionFactory connectionFactory, IDirMetadataDataAccess dirMetadataAccess)
        {
            this.connectionFactory = connectionFactory;
            this.dirMetadataAccess = dirMetadataAccess;
        }

        public List<RepositoryFolder> GetFolders(string folderId)
        {
            var retVal = new List<RepositoryFolder>();

            var folder = GetCmisFolder(folderId);
            if (folder != null)
            {
                var items = folder.GetChildren();
                foreach (var item in items)
                {
                    var subFolder = item as IFolder;
                    if (subFolder != null)
                    {
                        var extensions = subFolder.GetExtensions(ExtensionLevel.Object);
                        var isDossier = !string.IsNullOrEmpty(dirMetadataAccess.GetExtendedPropertyValue(extensions, "ARELDA:dossier/dossier@id"));
                        retVal.Add(GetRepositoryFolder(subFolder, extensions, isDossier));
                    }
                }
            }

            return retVal;
        }

        public List<RepositoryFile> GetFiles(string folderId, List<string> filePatternsToIgnore, out List<RepositoryFile> ignoredFiles)
        {
            Log.Information("Getting files for folder {folderId}", folderId);
            var retVal = new List<RepositoryFile>();
            ignoredFiles = new List<RepositoryFile>();
            var session = connectionFactory.ConnectToFirstRepository();

            var folder = session.GetObject(folderId) as IFolder;
            if (folder != null)
            {
                var items = folder.GetChildren();
                foreach (var item in items)
                {
                    var document = item as IDocument;
                    if (document?.IsLatestVersion != null && document.IsLatestVersion.Value)
                    {
                        var serializedProperties = JsonConvert.SerializeObject(document.Properties);
                        Log.Verbose("Cmis Document {Id} contains the following properties {serializedProperties}", document.Id, serializedProperties);

                        var extensions = document.GetExtensions(ExtensionLevel.Object);

                        var repositoryFile = new RepositoryFile
                        {
                            Id = document.Id,
                            LogicalName = document.Name,
                            SizeInBytes = document.ContentStreamLength ?? 0,
                            MimeType = document.ContentStreamMimeType,
                            Hash = dirMetadataAccess.GetExtendedPropertyValue(extensions, "Fixity Value"),
                            HashAlgorithm = dirMetadataAccess.GetExtendedPropertyValue(extensions, "Fixity Algorithm Ref"),
                            SipOriginalName = dirMetadataAccess.GetExtendedPropertyValue(extensions, "ARELDA:datei/datei/originalName"),
                            SipId = dirMetadataAccess.GetExtendedPropertyValue(extensions, "ARELDA:datei/datei@id")
                        };

                        // Check if file should be ignored
                        if (!filePatternsToIgnore.Any(fp => Regex.IsMatch(document.Name, fp, RegexOptions.IgnoreCase)))
                        {
                            retVal.Add(repositoryFile);
                        }
                        else
                        {
                            ignoredFiles.Add(repositoryFile);
                        }
                    }
                    else if (document != null)
                    {
                        Log.Warning("Found an older version of a document (id: {Id}). Skipping document!", document.Id);
                    }
                }
            }

            return retVal;
        }

        public RepositoryFolder GetRepositoryRoot(string packageId)
        {
            Log.Information("Getting repository root for package with id {packageId}", packageId);
            var session = connectionFactory.ConnectToFirstRepository();

            // Do we have a document identifier? These have two @ signs in its packageId
            var isDocument = packageId.Split('@').Length == 3;
            if (isDocument)
            {
                Log.Debug("Have a package at the document level.");
                return GetDocumentRoot(packageId, session);
            }

            // We have a dossier or subdossier
            return GetDossierOrSubdossierRoot(packageId, session);
        }

        public IFolder GetCmisFolder(string folderId)
        {
            var session = connectionFactory.ConnectToFirstRepository();

            var folder = session.GetObject(folderId) as IFolder;
            return folder;
        }

        public Stream GetFileContent(string fileId)
        {
            var session = connectionFactory.ConnectToFirstRepository();

            var document = session.GetObject(fileId) as IDocument;
            if (document != null)
            {
                var cs = document.GetContentStream();
                return cs.Stream;
            }

            return Stream.Null;
        }

        public string GetRepositoryName()
        {
            var session = connectionFactory.ConnectToFirstRepository();
            return session.RepositoryInfo.ProductName;
        }

        private RepositoryFolder GetDossierOrSubdossierRoot(string packageId, ISession session)
        {
            var isAlfresco = session.RepositoryInfo.ProductName.Equals("Alfresco Community", StringComparison.InvariantCultureIgnoreCase);
            var select = $"Select * from cmis:folder where cmis:description = '{packageId}'";
            Log.Verbose("Executing cmis select query: {select}", select);
            var result = session.Query(select, false);
            foreach (var queryResult in result)
            {
                var description = queryResult["cmis:description"].FirstValue.ToString();
                var objectId = queryResult["cmis:objectId"].FirstValue.ToString();
                var folder = session.GetObject(objectId) as IFolder;
                if (folder != null)
                {
                    var extensions = folder.GetExtensions(ExtensionLevel.Object);
                    var aipAtDossierId = dirMetadataAccess.GetExtendedPropertyValue(extensions, "AIP-ID_Dossier-ID");

                    // First condition is used to check an AMA repository. Second condition only applies to CMI Alfresco Repro
                    if (aipAtDossierId != null && aipAtDossierId.Equals(packageId, StringComparison.InvariantCultureIgnoreCase) ||
                        description.Equals(packageId, StringComparison.InvariantCultureIgnoreCase))
                    {
                        Log.Verbose("Found correct folder object. Folder name is {Name}.", folder.Name);
                        var isDossier = !string.IsNullOrEmpty(dirMetadataAccess.GetExtendedPropertyValue(extensions, "ARELDA:dossier/dossier@id")) ||
                                        isAlfresco;
                        return GetRepositoryFolder(folder, extensions, isDossier);
                    }
                }
            }

            Log.Warning("Did not find any results with the passed query: {select}.", select);
            return null;
        }

        private RepositoryFolder GetDocumentRoot(string packageId, ISession session)
        {
            Log.Debug("Getting document root for packageId: {packageId}", packageId);
            var segments = packageId.Split('@');
            var isAlfresco = session.RepositoryInfo.ProductName.Equals("Alfresco Community", StringComparison.InvariantCultureIgnoreCase);
            Log.Debug("The repository product is {productName}.", session.RepositoryInfo.ProductName);
            Debug.Assert(segments.Length == 3, "Document Identifiert must contain 3 segments seperated by @.");

            // Get the query depending on the system
            var select = isAlfresco
                ? $"Select * from cmis:folder where cmis:description = '{packageId}'"
                : $"Select * from cmis:folder where cmis:description = '{segments[0]}' AND cmis:path = '{segments[2]}'";

            // This query should return exactly one match
            var result = session.Query(select, false).FirstOrDefault();
            if (result != null)
            {
                Log.Debug("Found root folder for packageId {packageId}. About the verify, that it is correct folder.", packageId);
                var objectId = result["cmis:objectId"].FirstValue.ToString();
                var folder = session.GetObject(objectId) as IFolder;
                if (folder != null)
                {
                    var description = result["cmis:description"].FirstValue.ToString();
                    var extensions = folder.GetExtensions(ExtensionLevel.Object);
                    var documentId = dirMetadataAccess.GetExtendedPropertyValue(extensions, "Catalogue Reference");

                    // For AMA repository: The last segment must match the catalogue reference value
                    // For Alfresco repository: Description must match the package id
                    if (!isAlfresco && documentId.Equals(segments[2], StringComparison.InvariantCultureIgnoreCase) ||
                        isAlfresco && description.Equals(packageId, StringComparison.InvariantCultureIgnoreCase))
                    {
                        Log.Verbose("Found correct folder object. Folder name is {Name}.", folder.Name);
                        var isDossier = !string.IsNullOrEmpty(dirMetadataAccess.GetExtendedPropertyValue(extensions, "ARELDA:dossier/dossier@id"));
                        return GetRepositoryFolder(folder, extensions, isDossier);
                    }
                }
            }

            Log.Warning("Did not find any results with the passed query: {select}.", select);
            return null;
        }

        private RepositoryFolder GetRepositoryFolder(IFolder folder, IList<ICmisExtensionElement> extensions, bool isDossier)
        {
            return new RepositoryFolder
            {
                Id = folder.Id,
                LogicalName = folder.Name,
                SipId = GetFolderSipId(extensions, isDossier),
                SipType = isDossier ? RepositoryFolderSipType.dossier : RepositoryFolderSipType.dokument
            };
        }

        private string GetFolderSipId(IList<ICmisExtensionElement> extensions, bool isDossier)
        {
            return isDossier
                ? dirMetadataAccess.GetExtendedPropertyValue(extensions, "ARELDA:dossier/dossier@id")
                : dirMetadataAccess.GetExtendedPropertyValue(extensions, "ARELDA:dokument/dokument@id");
        }
    }
}