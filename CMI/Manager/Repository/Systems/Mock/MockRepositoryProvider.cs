using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using CMI.Contract.Common;
using CMI.Contract.Messaging;
using CMI.Contract.Repository;
using CMI.Manager.Repository.Properties;
using CMI.Manager.Repository.Systems.Bar;
using MassTransit;
using Newtonsoft.Json;
using Serilog;

namespace CMI.Manager.Repository.Systems.Mock;

public class MockRepositoryProvider : IRepositoryProvider
{
    private const string contentFolderName = "content";
    private const string headerFolderName = "header";
    private readonly RepositoryPackageInfoResult repositoryPackageInfoResult;
    private readonly RepositoryPackageResult repositoryPackageResult;
    private readonly IBus bus;
    private readonly IDirPackageValidator dirPackageValidator;
    private readonly IPackageHandler handler;

    public MockRepositoryProvider(IBus bus, IDirPackageValidator dirPackageValidator, IPackageHandler handler)
    {
        this.bus = bus;
        this.dirPackageValidator = dirPackageValidator;
        this.handler = handler;

        repositoryPackageInfoResult = JsonConvert.DeserializeObject<RepositoryPackageInfoResult>(File.ReadAllText(@"Mock\Data\RepositoryPackageInfoResult.json"));
        repositoryPackageResult = JsonConvert.DeserializeObject<RepositoryPackageResult>(File.ReadAllText(@"Mock\Data\RepositoryPackageResult.json"));
    }

    public async Task<RepositoryPackageResult> GetPackage(string packageId, string archiveRecordId, bool createMetadataXml, List<string> fileTypesToIgnore, int primaerdatenAuftragId)
    {
        var tempRootName = Path.GetRandomFileName();
        var storagePath = Settings.Default.TempStoragePath;
        if (!Directory.Exists(storagePath))
        {
            Directory.CreateDirectory(storagePath);
        }

        var tempRootFolder = Path.Combine(storagePath, tempRootName);
        Directory.CreateDirectory(tempRootFolder);
        await UpdatePrimaerdatenAuftragStatus(primaerdatenAuftragId, AufbereitungsStatusEnum.PrimaerdatenExtrahiert);
        // Ensure valid file names and prevent too long paths and file names
        dirPackageValidator.EnsureValidPhysicalFileAndFolderNames(repositoryPackageResult.PackageDetails,
            Path.Combine(tempRootFolder, contentFolderName));
        await UpdatePrimaerdatenAuftragStatus(primaerdatenAuftragId, AufbereitungsStatusEnum.ZipDateiErzeugt);

        await handler.CreateMetadataXml(Path.Combine(tempRootFolder, headerFolderName), repositoryPackageResult.PackageDetails,
            new List<RepositoryFile>());
        await UpdatePrimaerdatenAuftragStatus(primaerdatenAuftragId, AufbereitungsStatusEnum.PaketTransferiert);

        CopayFileToDestination(new FileInfo(@"Mock\Data\test.zip"), tempRootName);
        repositoryPackageResult.PackageDetails.PackageFileName = tempRootName + ".zip";
        repositoryPackageResult.PackageDetails.ArchiveRecordId = archiveRecordId;
        return repositoryPackageResult;
    }

    public RepositoryPackageInfoResult ReadPackageMetadata(string packageId, string archiveRecordId)
    {
        repositoryPackageInfoResult.PackageDetails.ArchiveRecordId = archiveRecordId;
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