using CMI.Access.Repository.Properties;
using CMI.Access.Repository.Systems.Rosetta.Helper;

using System.Threading.Tasks;
using System.IO;
using System.Net;
using System;
using System.IO.Compression;
using System.Linq;
using Serilog;


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
        // Test IE
        entityId = "IE444295";
        var success = await rosettaConnector.StartExportAsync(entityId); 
        if(success)
        {
            using (new ConnectToSharedFolder(directory, new NetworkCredential(userName, password, domain)))
            {
                try
                {
                    CopyAndExtractIntellectualEntity(defaultTempStoragePath, entityId);
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

    private void CopyAndExtractIntellectualEntity(string defaultTempStoragePath, string entityId)
    {
        var directoryEntity = Directory.GetDirectories(directory).First(f => f.EndsWith(entityId));
        var copyPath = Path.Combine(defaultTempStoragePath, entityId);
        if (Directory.Exists(copyPath))
        {
            Directory.Delete(copyPath, true);
        }

        CopyDirectory(directoryEntity, copyPath);
        var dir = Directory.GetDirectories(copyPath).Length == 1
            ? Directory.GetDirectories(copyPath).First()
            : throw new Exception("Too many directories, one was expected");
        var file = Directory.GetFiles(dir, "*.tar").Length == 1
            ? Directory.GetFiles(dir, "*.tar").First()
            : throw new Exception("Too many tar-Files, one was expected");
        
        var fileInfo = new FileInfo(file);
        var newDic = Directory.CreateDirectory(Path.Combine(dir,
            fileInfo.Name.Substring(0, fileInfo.Name.Length - fileInfo.Extension.Length)));

        // ToDo: Anderes entpackungstool verwenden
        ZipFile.ExtractToDirectory(fileInfo.FullName, newDic.FullName);
        Directory.Delete(directoryEntity, true);
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