using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CMI.Access.Repository.Systems.Rosetta;
using CMI.Contract.Common;
using CMI.Contract.Messaging;
using CMI.Contract.Parameter;
using CMI.Contract.Repository;
using MassTransit;

namespace CMI.Manager.Repository.Systems.Rosetta
{
    public class RosettaRepositoryProvider : IRepositoryProvider
    {
        private readonly IRosettaDataAccess rosettaDataAccess;
        private readonly IPackageHandler handler;
        private readonly IParameterHelper parameterHelper;
        private readonly IBus bus;

        public RosettaRepositoryProvider(IRosettaDataAccess rosettaDataAccess, IPackageHandler handler,
            IParameterHelper parameterHelper, IBus bus)
        {
            this.rosettaDataAccess = rosettaDataAccess;
            this.handler = handler;
            this.parameterHelper = parameterHelper;
            this.bus = bus;
        }

        public async Task<RepositoryPackageResult> GetPackage(string packageId, string archiveRecordId, bool createMetadataXml, List<string> fileTypesToIgnore, int primaerdatenAuftragId)
        {
            // ToDo: DLS-333 Rosetta-Anbindung (Export einer IntellectualEntity)
            var requestClient = bus.CreateRequestClient<FindArchiveRecordRequest>(new Uri(bus.Address, BusConstants.IndexManagerFindArchiveRecordMessageQueue), TimeSpan.FromSeconds(10));
            var response = await requestClient.GetResponse<FindArchiveRecordResponse>(new FindArchiveRecordRequest { ArchiveRecordId = archiveRecordId });
            if(response.Message.ElasticArchiveRecord == null)
            {
                return new RepositoryPackageResult
                {
                    Success = false,
                    ErrorMessage = "ArchiveRecord not found."
                };
            }

            var exportPath =  await rosettaDataAccess.ExportIntellectualEntity(packageId);
            if(exportPath == null)
            {
                return new RepositoryPackageResult
                {
                    Success = false,
                    ErrorMessage = "Export has failed."
                };
            }

            var repositoryPackage = BuildRepositoryPackage(exportPath, response.Message.ElasticArchiveRecord, createMetadataXml, fileTypesToIgnore, primaerdatenAuftragId);
            return new RepositoryPackageResult
            {
                Success = true,
                PackageDetails = repositoryPackage
            };
        }

        private RepositoryPackage BuildRepositoryPackage(string exportPath, ElasticArchiveRecord elasticArchiveRecord, bool createMetadataXml, List<string> fileTypesToIgnore, int primaerdatenAuftragId)
        {
            return new RepositoryPackage
            {
                ArchiveRecordId = elasticArchiveRecord.ArchiveRecordId,
                SizeInBytes = 0,
            };
        }

        public async Task<RepositoryPackageInfoResult> ReadPackageMetadata(string packageId, string archiveRecordId)
        {
            // andere Methode verwenden
            await rosettaDataAccess.ExportIntellectualEntity(packageId);
            return null;
        }
    }
}
