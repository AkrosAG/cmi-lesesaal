using CMI.Access.Repository.Systems.Rosetta;
using CMI.Contract.Common;
using CMI.Contract.Messaging;
using CMI.Manager.Repository.Properties;
using MassTransit;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace CMI.Manager.Repository.Systems.Rosetta
{
    public class RosettaRepositoryProvider : IRepositoryProvider
    {
        private readonly IRosettaDataAccess rosettaDataAccess;
        private readonly RepositoryPackageBuilder builder;
        private readonly IBus bus;


        private readonly string[] iEs = { "IE609791", "IE610326", "IE611472", "IE611480", "IE611508"
            , "IE611531", "IE611662", "IE611671", "IE611682", "IE611691", "IE611696"};


        public RosettaRepositoryProvider(IRosettaDataAccess rosettaDataAccess, RepositoryPackageBuilder builder, IBus bus)
        {
            this.rosettaDataAccess = rosettaDataAccess;       
            this.builder = builder;                              
            this.bus = bus;
        }
                                                                 
        public async Task<RepositoryPackageResult> GetPackage(string packageId, string archiveRecordId, bool createMetadataXml, List<string> fileTypesToIgnore, int primaerdatenAuftragId)
        {
            Debug.Assert(fileTypesToIgnore != null, "fileTypesToIgnore must not be null");
            var currentStatus = AufbereitungsStatusEnum.AuftragGestartet;
            await UpdatePrimaerdatenAuftragStatus(primaerdatenAuftragId, currentStatus);
            var retVal = new RepositoryPackageResult
            {
                Success = true,
                Valid = true
            };

            retVal.Success = true;
            retVal.Valid = true;
            currentStatus = AufbereitungsStatusEnum.PrimaerdatenExtrahiert;
            await UpdatePrimaerdatenAuftragStatus(primaerdatenAuftragId, currentStatus);

            return retVal;
        }

        public async Task<RepositoryPackageInfoResult> ReadPackageMetadata(ElasticArchiveRecord elasticArchiveRecord)
        {
            // Test IEs Must remove, only for tests
            elasticArchiveRecord.PrimaryDataLink = iEs[new Random().Next(0, 10)] ;
            var success = await rosettaDataAccess.ExportIntellectualEntity(Settings.Default.TempStoragePath, elasticArchiveRecord.PrimaryDataLink);
            if (success)
            {
                var package = await builder.BuildRepositoryPackageAsync(elasticArchiveRecord);

                var list = new List<RepositoryPackage> { package };
                elasticArchiveRecord.PrimaryData = list.ToElasticArchiveRecordPackage(); 


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
