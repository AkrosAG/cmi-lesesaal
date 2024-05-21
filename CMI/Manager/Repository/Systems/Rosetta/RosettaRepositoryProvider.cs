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
using CMI.Engine.PackageMetadata.Systems.Rosetta;

namespace CMI.Manager.Repository.Systems.Rosetta
{
    public class RosettaRepositoryProvider : IRepositoryProvider
    {
        private readonly IRosettaDataAccess rosettaDataAccess;
        private readonly IRepositoryPackageBuilder builder;
        private readonly IBus bus;
        private readonly IPackageHandler handler;

        public RosettaRepositoryProvider(IRosettaDataAccess rosettaDataAccess, IPackageHandler handler, IRepositoryPackageBuilder builder, IBus bus)
        {
            this.handler = handler;
            this.rosettaDataAccess = rosettaDataAccess;       
            this.builder = builder;                              
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
            var currentStatus = AufbereitungsStatusEnum.AuftragGestartet;
            await UpdatePrimaerdatenAuftragStatus(primaerdatenAuftragId, currentStatus);
            var success = await rosettaDataAccess.ExportIntellectualEntity(Settings.Default.TempStoragePath, packageId);
            currentStatus = AufbereitungsStatusEnum.PrimaerdatenExtrahiert;
            await UpdatePrimaerdatenAuftragStatus(primaerdatenAuftragId, currentStatus);


            if (success)
            {
                var repositoryPackage = await builder.BuildRepositoryPackageAsync(archiveRecordId, packageId);
                // ToDo: fileTypesToIgnore
                if (createMetadataXml)
                {
                    await handler.CreateMetadataXml("Braucht man bei Rosetta nicht", repositoryPackage, new List<RepositoryFile>());
                }
                

                return new RepositoryPackageResult
                {
                    Success = true,
                    PackageDetails = repositoryPackage,
                    Valid = true
                };
            }

            currentStatus = AufbereitungsStatusEnum.PrimaerdatenExtrahiert;
            await UpdatePrimaerdatenAuftragStatus(primaerdatenAuftragId, currentStatus);

            var retVal = new RepositoryPackageResult
            {
                Success = true,
                Valid = true
            };

            retVal.Success = true;
            retVal.Valid = true;


            builder.BuildZipFile(archiveRecordId, packageId);
            currentStatus = AufbereitungsStatusEnum.ZipDateiErzeugt;
            await UpdatePrimaerdatenAuftragStatus(primaerdatenAuftragId, currentStatus);
            return retVal;
        }

        public async Task<RepositoryPackageInfoResult> ReadPackageMetadata(ElasticArchiveRecord elasticArchiveRecord)
        {
            var success = await rosettaDataAccess.ExportIntellectualEntity(Settings.Default.TempStoragePath, elasticArchiveRecord.PrimaryDataLink);
            if (success)
            {
                var package = await builder.BuildRepositoryPackageAsync(elasticArchiveRecord.ArchiveRecordId, elasticArchiveRecord.PrimaryDataLink);
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

    }
}
