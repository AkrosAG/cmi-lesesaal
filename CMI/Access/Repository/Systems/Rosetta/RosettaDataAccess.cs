using CMI.Access.Repository.Properties;
using CMI.Access.Repository.Systems.Rosetta.Helper;
using Serilog;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace CMI.Access.Repository.Systems.Rosetta;

public class RosettaDataAccess : IRosettaDataAccess
{
    private readonly string userName = Settings.Default.RepositoryDirectoryUser;
    private readonly string password = Settings.Default.RepositoryDirectoryPassword;
    private readonly string repositoryDirectory = Settings.Default.RepositoryDirectory;
    private readonly string domain = Settings.Default.RepositoryDomain;

    private readonly RosettaConnector rosettaConnector;

    public RosettaDataAccess(RosettaConnector connector)
    {
        rosettaConnector = connector;
    }

    public async Task<bool> ExportIntellectualEntity(string defaultTempStoragePath, string entityId)
    {
        var success = await rosettaConnector.StartExportAsync(entityId); 
        if(success)
        {
            using (new ConnectToSharedFolder(repositoryDirectory, new NetworkCredential(userName, password, domain)))
            {
                try
                {
                    CopyNecessaryExtractIntellectualEntityFiles(defaultTempStoragePath, entityId);
                }
                catch (Exception e)
                {
                    success = false;
                    Log.Error(e, $"An error occurred when copying the Intellectual Entity {entityId}");
                }
            }
            Log.Information($"Intellectual Entity {entityId} exported successfully to {Path.Combine(defaultTempStoragePath, entityId)}");
        }
        
        return success;
    }

    private void CopyNecessaryExtractIntellectualEntityFiles(string defaultTempStoragePath, string entityId)
    {
        var directoryEntity = Directory.GetDirectories(repositoryDirectory).First(f => f.EndsWith(entityId));
        var copyPath = Path.Combine(defaultTempStoragePath, entityId);
        if (Directory.Exists(copyPath))
        {
            Directory.Delete(copyPath, true);
        }

        var files =  Directory.GetFiles(directoryEntity, "*.tar", SearchOption.AllDirectories);
        foreach (var file in files)
        {
            var fileInfo = new FileInfo(file);
            var tarFileName = fileInfo.Name.Substring(0, fileInfo.Name.Length - fileInfo.Extension.Length);
            if (Directory.GetDirectories(directoryEntity).Any(dic => dic.EndsWith(tarFileName)))
            {
                // Die tar Datei wird nicht benötigt und wird vor dem kopieren gelöscht
                File.Delete(fileInfo.FullName);
            }
        }

        CopyDirectory(directoryEntity, copyPath);
        Directory.Delete(directoryEntity, true);
    }

    private static void CopyDirectory(string sourceDir, string destinationDir)
    {
        var dir = new DirectoryInfo(sourceDir);

        // Check if the source directory exists
        if (!dir.Exists)
        {
            throw new DirectoryNotFoundException($"Source repositoryDirectory not found: {dir.FullName}");
        }

        // Cache directories before we start copying
        var dirs = dir.GetDirectories();
        Directory.CreateDirectory(destinationDir);

        // Get the files in the repository directory and copy to the local directory
        foreach (var file in dir.GetFiles())
        {
            var targetFilePath = Path.Combine(destinationDir, file.Name);
            file.CopyTo(targetFilePath);
        }

        foreach (DirectoryInfo subDir in dirs)
        {
            var newDestinationDir = Path.Combine(destinationDir, subDir.Name);
            CopyDirectory(subDir.FullName, newDestinationDir);
        }
    }
}