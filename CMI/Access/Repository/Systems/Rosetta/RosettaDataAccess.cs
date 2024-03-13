using System.Threading.Tasks;
using CMI.Access.Repository.Properties;
using System.IO;
using System.Net;
using System;
using System.Linq;
using Serilog;
using CMI.Access.Repository.Systems.Rosetta.Helper;

namespace CMI.Access.Repository.Systems.Rosetta;

public class RosettaDataAccess : IRosettaDataAccess
{
    private readonly string userName = Settings.Default.RepositoryDirectoryUser;
    private readonly string password = Settings.Default.RepositoryDirectoryPassword;
    private readonly string directory = Settings.Default.RepositoryDirectory;
    private readonly string domain = Settings.Default.RepositoryDomain;

    private readonly RosettaConnector rosettaConnector;

    public RosettaDataAccess(RosettaConnector connector)
    {
        rosettaConnector = connector;
    }

    public async Task<bool> ExportIntellectualEntity(string defaultTempStoragePath, string entityId)
    {
        entityId = "IE444295";
        var success = await rosettaConnector.StartExportAsync(entityId); 
        if(success)
        {
            using (new ConnectToSharedFolder(directory, new NetworkCredential(userName, password, domain)))
            {
                var directoryEntity = Directory.GetDirectories(directory).First(f => f.EndsWith(entityId));
                var copyPath = Path.Combine(defaultTempStoragePath, entityId);
                if (Directory.Exists(copyPath))
                {
                    Directory.Delete(copyPath, true);
                }
                CopyDirectory(directoryEntity, copyPath);
                Directory.Delete(directoryEntity, true);
            }
            Log.Information($"Intellectual Entity {entityId} exported successfully to {Path.Combine(defaultTempStoragePath, entityId)}");
        }
        
        return success;
    }

    private static void CopyDirectory(string sourceDir, string destinationDir)
    {
        // Get information about the source directory
        var dir = new DirectoryInfo(sourceDir);

        // Check if the source directory exists
        if (!dir.Exists)
        {
            throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");
        }

        // Cache directories before we start copying
        var dirs = dir.GetDirectories();

        // Create the destination directory
        Directory.CreateDirectory(destinationDir);

        // Get the files in the source directory and copy to the destination directory
        foreach (var file in dir.GetFiles())
        {
            string targetFilePath = Path.Combine(destinationDir, file.Name);
            file.CopyTo(targetFilePath);
        }

        foreach (DirectoryInfo subDir in dirs)
        {
            string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
            CopyDirectory(subDir.FullName, newDestinationDir);
        }
    }
}