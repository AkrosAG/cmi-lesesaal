using CMI.Access.Repository.Systems.Rosetta;
using CMI.Contract.Common;
using CMI.Contract.Messaging;
using CMI.Contract.Repository;
using CMI.Manager.Repository.Properties;
using MassTransit;
using Serilog;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Diagnostics;
using CMI.Engine.PackageMetadata.Systems.Rosetta.Schema;
using System.IO.Compression;
using System.IO;
using System.Linq;
using File = System.IO.File;

namespace CMI.Manager.Repository.Systems.Rosetta
{
    public class RosettaRepositoryProvider : IRepositoryProvider
    {
        private readonly IRosettaDataAccess rosettaDataAccess;
        private readonly IRequestClient<FindArchiveRecordRequest> indexClient;
        private readonly IBus bus;
        private readonly IPackageHandler handler;

        public RosettaRepositoryProvider(IRosettaDataAccess rosettaDataAccess, IPackageHandler handler, IRequestClient<FindArchiveRecordRequest> indexClient, IBus bus)
        {
            this.handler = handler;
            this.rosettaDataAccess = rosettaDataAccess;       
            this.indexClient = indexClient;                              
            this.bus = bus;
        }

        public List<RepositoryPackage> PrimaryData { get; set; }

        /// <summary>
        /// We already have the package. Here the order is updated, the name comes because the interface is used on several repositories.
        /// </summary>
        /// <param name="packageId"></param>
        /// <param name="archiveRecordId"></param>
        /// <param name="createMetadataXml"></param>
        /// <param name="fileTypesToIgnore"></param>
        /// <param name="primaerdatenAuftragId"></param>
        /// <returns></returns>
        public async Task<RepositoryPackageResult> GetPackage(string packageId, string archiveRecordId, bool createMetadataXml, List<string> fileTypesToIgnore, int primaerdatenAuftragId)
        {
            Log.Information("RosettaRepositoryProvider GetPackage {packageId} for archiveRecord {archiveRecordId}", packageId, archiveRecordId);
            var currentStatus = AufbereitungsStatusEnum.AuftragGestartet;
            await UpdatePrimaerdatenAuftragStatus(primaerdatenAuftragId, currentStatus);
            var watch = new Stopwatch();
            watch.Start();
            var repositoryPackageResult = new RepositoryPackageResult
            {
                Success = false,
                Valid = false
            };
            
            var success = await rosettaDataAccess.ExportIntellectualEntity(Settings.Default.TempStoragePath, packageId);
            var zipFileName = Path.GetRandomFileName();
            if (success)
            {
                currentStatus = AufbereitungsStatusEnum.PrimaerdatenExtrahiert;
                await UpdatePrimaerdatenAuftragStatus(primaerdatenAuftragId, currentStatus);
                var archiveRecord = indexClient.GetResponse<FindArchiveRecordResponse>(new FindArchiveRecordRequest { ArchiveRecordId = archiveRecordId }).Result.Message.ElasticArchiveRecord;
               
                var repositoryPackage = await BuildRepositoryPackageAsync(archiveRecord, zipFileName);
                // ToDo: fileTypesToIgnore
                if (createMetadataXml)
                {
                    await handler.CreateMetadataXml(Settings.Default.TempStoragePath, repositoryPackage, new List<RepositoryFile>());
                }
                watch.Stop();
                repositoryPackage.RepositoryExtractionDuration = watch.ElapsedMilliseconds;
                repositoryPackageResult.Success = true;
                repositoryPackageResult.PackageDetails = repositoryPackage;
                repositoryPackageResult.Valid = true;

                BuildZipFile(archiveRecordId, packageId, zipFileName);
                currentStatus = AufbereitungsStatusEnum.ZipDateiErzeugt;
                await UpdatePrimaerdatenAuftragStatus(primaerdatenAuftragId, currentStatus);
            }

            // Make sure that the temporary folder is deleted

            DeleteTemporaryDirectory(Path.Combine(Settings.Default.TempStoragePath, packageId));
            DeleteTemporaryDirectory(Path.Combine(Settings.Default.TempStoragePath, zipFileName));

            return repositoryPackageResult;
        }

        public async Task<RepositoryPackageInfoResult> ReadPackageMetadata(ElasticArchiveRecord elasticArchiveRecord)
        {
            Log.Information("Read Package Metadata {PrimaryDataLink} for archiveRecord {archiveRecordId}", elasticArchiveRecord.PrimaryDataLink, elasticArchiveRecord.ArchiveRecordId);
            var success = await rosettaDataAccess.ExportIntellectualEntity(Settings.Default.TempStoragePath, elasticArchiveRecord.PrimaryDataLink);
            if(success)
            {
                // Zip file name not important here, because we are not creating a zip file
                var package = await BuildRepositoryPackageAsync(elasticArchiveRecord, string.Empty);
                return new RepositoryPackageInfoResult
                {
                    Success = true,
                    PackageDetails = package,
                    Valid = true
                };
            }

            return new RepositoryPackageInfoResult
            {
                Success = false,
                Valid = false
            };
            
        }
        
        public async Task<RepositoryPackage> BuildRepositoryPackageAsync(ElasticArchiveRecord archiveRecord, string zipFileName)
        {
            var files = new List<RepositoryFile>();
            var rootFolder = new List<RepositoryFolder>();

            var fileUrl = $@"{Path.Combine(Settings.Default.TempStoragePath, archiveRecord.PrimaryDataLink)}\ie.xml";
            var mets = Mets.LoadFromFile(fileUrl);

            var folder = mets.GetImportFolderName();

            var sourcePath = Path.Combine(Path.GetDirectoryName(fileUrl), folder);
            Log.Information($"Package Source Path: {sourcePath}");
            var totalFileSize = 0L;
            CalculateFolderAndFileStructures(sourcePath,  rootFolder, files, ref totalFileSize);

            var preZip = DateTime.Now;
            var result = new RepositoryPackage
            {
                PackageFileName = zipFileName + ".zip",
                PackageId = archiveRecord.PrimaryDataLink,
                ArchiveRecordId = archiveRecord.ArchiveRecordId,
                SizeInBytes = totalFileSize,
                FileCount = files.Count,
                RepositoryExtractionDuration = DateTime.Now.Ticks - preZip.Ticks,
                FulltextExtractionDuration = 0,
                Files = files,
                Folders = rootFolder
            };

            return await Task.FromResult(result);
        }


        private void CalculateFolderAndFileStructures(string rootPath, List<RepositoryFolder> folders, List<RepositoryFile> files, ref long totalFileSize)
        {
            var subFolders = new List<RepositoryFolder>();
            var subdirs = Directory.GetDirectories(rootPath);
            var subFiles = new List<RepositoryFile>();
            // Get the files in the repository directory and copy to the local directory
            foreach (var file in Directory.GetFiles(rootPath))
            {
                var fileInfo = new FileInfo(file);
                if (fileInfo.Exists)
                {
                    totalFileSize += fileInfo.Length;
                    var id = fileInfo.Name.Contains('_')
                        ? fileInfo.Name.Substring(0, fileInfo.Name.IndexOf('_'))
                        : fileInfo.Name.Substring(0, fileInfo.Name.Length - fileInfo.Extension.Length);
                    
                    subFiles.Add(new RepositoryFile
                    {
                        PhysicalName = fileInfo.Name,
                        Exported = true,
                        MimeType = fileInfo.Extension,
                        Id = id
                    });
                }
            }

            files.AddRange(subFiles);

            foreach (var subDir in subdirs)
            {

                subFolders.Add(new RepositoryFolder()
                {
                    LogicalName = subDir,
                    PhysicalName = subDir,
                    Id = subDir
                });
                CalculateFolderAndFileStructures(subDir, subFolders, files, ref totalFileSize);
            }

            if (subFolders.Count > 0)
            {
                folders.Add(new RepositoryFolder
                {
                    Files = subFiles,
                    Folders = subFolders
                });
            }
        }
       
        private void BuildZipFile(string archiveRecordId, string primaryDataLink, string zipFilename)
        {
            var fileUrl = $@"{Path.Combine(Settings.Default.TempStoragePath, primaryDataLink)}\ie.xml";
            var mets = Mets.LoadFromFile(fileUrl);

            var folder = mets.GetImportFolderName();

            var sourcePath = Path.Combine(Path.GetDirectoryName(fileUrl), folder);
            var targetFile = Path.Combine(Settings.Default.FileCopyDestinationPath, zipFilename + ".zip");
            var zipBaseDir = Path.Combine(Settings.Default.TempStoragePath, zipFilename);

            var contentDir = Path.Combine(zipBaseDir, "content");
            var headerDir = Path.Combine(zipBaseDir, "header");
            Directory.CreateDirectory(headerDir);

            RosettaDataAccess.CopyDirectory(sourcePath, contentDir);

            if (File.Exists(targetFile))
            {
                File.Delete(targetFile);
            }

            ZipFile.CreateFromDirectory(zipBaseDir, targetFile);
            Log.Information("Created zip file {0}. ArchiveRecord Id: {1}", targetFile, archiveRecordId);
            Directory.Delete(Path.Combine(Settings.Default.TempStoragePath, primaryDataLink), true);
            Directory.Delete(zipBaseDir, true);
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

        private static void DeleteTemporaryDirectory(string tempDir)
        {
            if (Directory.Exists(tempDir))
            {
                try
                {
                    Directory.Delete(tempDir, true);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Unexpected error while deleting directory {tempDir}", tempDir);
                }
            }
        }

    }
}
