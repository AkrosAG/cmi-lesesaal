using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Autofac.Features.Metadata;
using System.Xml;
using CMI.Access.Repository.Systems.Rosetta;
using CMI.Contract.Common;
using CMI.Contract.Messaging;
using CMI.Contract.Parameter;
using CMI.Contract.Repository;
using MassTransit;
using Serilog;
using System.Xml.Linq;
using CMI.Manager.Repository.Properties;

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
            this.bus = bus;
            this.builder = builder;                              
        }
                                                                 
        public async Task<RepositoryPackageResult> GetPackage(string packageId, string archiveRecordId, bool createMetadataXml, List<string> fileTypesToIgnore, int primaerdatenAuftragId)
        {                                                        
            var requestClient = bus.CreateRequestClient<FindArchiveRecordRequest>(new Uri(bus.Address, BusConstants.IndexManagerFindArchiveRecordMessageQueue), TimeSpan.FromSeconds(10));
            var response = await requestClient.GetResponse<FindArchiveRecordResponse>(new FindArchiveRecordRequest { ArchiveRecordId = archiveRecordId });

            if (response.Message.ElasticArchiveRecord == null)
            {
                return new RepositoryPackageResult
                {
                    Success = false,
                    ErrorMessage = "ArchiveRecord in Elastic not found."
                };
            }

            // TODO: DLS-333 Rosetta-Anbindung (Export einer IntellectualEntity) - Logik implementieren

            return new RepositoryPackageResult
            {
                Success = false,
                ErrorMessage = "Export has failed."
            };
        }

        public async Task<RepositoryPackageInfoResult> ReadPackageMetadata(ElasticArchiveRecord elasticArchiveRecord)
        {
            // Test IEs Must remove, only for tests
            elasticArchiveRecord.PrimaryDataLink = iEs[new Random().Next(0, 10)] ;
            var success = await rosettaDataAccess.ExportIntellectualEntity(Settings.Default.TempStoragePath, elasticArchiveRecord.PrimaryDataLink);
            if (success)
            {
                var package = await builder.BuildRepositoryPackageAsync(elasticArchiveRecord);
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
    }
}
