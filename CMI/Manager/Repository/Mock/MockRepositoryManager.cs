using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using CMI.Contract.Common;
using CMI.Contract.Messaging;
using CMI.Contract.Repository;
using CMI.Manager.Repository.Properties;
using MassTransit;
using Newtonsoft.Json;
using Serilog;

namespace CMI.Manager.Repository.Mock;

public class MockRepositoryManager : IRepositoryManager
{
    private const string contentFolderName = "content";
    private const string headerFolderName = "header";
    private readonly RepositoryPackageInfoResult repositoryPackageInfoResult;
    private readonly RepositoryPackageResult repositoryPackageResult;
    private readonly IBus bus;
    private readonly IPackageValidator packageValidator; 
    private readonly IPackageHandler handler;

    public MockRepositoryManager(IBus bus, IPackageValidator packageValidator, IPackageHandler handler)
    {
        this.bus = bus;
        this.packageValidator = packageValidator;
        this.handler = handler;

        repositoryPackageInfoResult = JsonConvert.DeserializeObject<RepositoryPackageInfoResult>(File.ReadAllText(@"Data\RepositoryPackageInfoResult.json"));
        repositoryPackageResult = JsonConvert.DeserializeObject<RepositoryPackageResult>(File.ReadAllText(@"Data\RepositoryPackageResult.json"));
    }

  
    public async Task<RepositoryPackageResult> GetPackage(string packageId, string archiveRecordId, int primaerdatenAuftragId)
    {
        return await GetPackageInternal(primaerdatenAuftragId);
    }

    public async Task<RepositoryPackageResult> AppendPackageToArchiveRecord(ArchiveRecord archiveRecord, long mutationId, int primaerdatenId)
    {
        await GetPackageInternal(primaerdatenId);
        archiveRecord.PrimaryData.Add(repositoryPackageResult.PackageDetails);
        return repositoryPackageResult;
    }

    private async Task<RepositoryPackageResult> GetPackageInternal(int primaerdatenId)
    {
        var tempRootName = Path.GetRandomFileName();
        var storagePath = Settings.Default.TempStoragePath;
        if (!Directory.Exists(storagePath))
        {
            Directory.CreateDirectory(storagePath);
        }

        var tempRootFolder = Path.Combine(storagePath, tempRootName);
        Directory.CreateDirectory(tempRootFolder);
        await UpdatePrimaerdatenAuftragStatus(primaerdatenId, AufbereitungsStatusEnum.PrimaerdatenExtrahiert);
        // Ensure valid file names and prevent too long paths and file names
        packageValidator.EnsureValidPhysicalFileAndFolderNames(repositoryPackageResult.PackageDetails,
            Path.Combine(tempRootFolder, contentFolderName));
        await UpdatePrimaerdatenAuftragStatus(primaerdatenId, AufbereitungsStatusEnum.ZipDateiErzeugt);
     
        await handler.CreateMetadataXml(Path.Combine(tempRootFolder, headerFolderName), repositoryPackageResult.PackageDetails,
            new List<RepositoryFile>());
        await UpdatePrimaerdatenAuftragStatus(primaerdatenId, AufbereitungsStatusEnum.PaketTransferiert);

        CopayFileToDestination(new FileInfo(@"Mock\test.zip"), tempRootName);
        repositoryPackageResult.PackageDetails.PackageFileName = tempRootName + ".zip";
        return repositoryPackageResult;
    }

    public RepositoryPackageInfoResult ReadPackageMetadata(string packageId, string archiveRecordId)
    {
        return repositoryPackageInfoResult;

    }


    private async Task UpdatePrimaerdatenAuftragStatus(int primaerdatenAuftragId, AufbereitungsStatusEnum status, string errorText = null)
    {
        if (primaerdatenAuftragId > 0)
        {
            Log.Information("Auftrag mit Id {PrimaerdatenAuftragId} wurde im Repository-Service auf Status {Status} gesetzt.",
            primaerdatenAuftragId, status.ToString());

            var ep = await bus.GetSendEndpoint(new Uri(bus.Address, BusConstants.AssetManagerUpdatePrimaerdatenAuftragStatusMessageQueue));
            await ep.Send<IUpdatePrimaerdatenAuftragStatus>(new UpdatePrimaerdatenAuftragStatus
            {
                PrimaerdatenAuftragId = primaerdatenAuftragId,
                Service = AufbereitungsServices.RepositoryService,
                Status = status,
                ErrorText = errorText
            });
        }
    }

    private static void CopayFileToDestination(FileInfo zipFile, string zipName)
    {
        // If we have a final destination folder, move the zip file there
        var finalDestinationPath = Settings.Default.FileCopyDestinationPath;
        if (!string.IsNullOrEmpty(finalDestinationPath) && Directory.Exists(finalDestinationPath))
        {
            Log.Information("Moving zip file to the final destination.");
            // Set the new final name
            var destFileName = Path.Combine(Settings.Default.FileCopyDestinationPath, zipName + ".zip");
            File.Copy(zipFile.FullName, destFileName);
        }
        else
        {
            Log.Warning("Final destination path could not be found. Make sure the path {finalDestinationPath} exits and is accessible.",
                finalDestinationPath);
        }
    }
}