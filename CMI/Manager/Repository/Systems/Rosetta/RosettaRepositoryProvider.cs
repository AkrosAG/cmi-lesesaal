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

namespace CMI.Manager.Repository.Systems.Rosetta
{
    public class RosettaRepositoryProvider : IRepositoryProvider
    {
        private readonly IRosettaDataAccess rosettaDataAccess;
        private readonly IPackageHandler handler;
        private readonly IParameterHelper parameterHelper;
        private readonly RepositoryPackageBuilder builder;
        private readonly IBus bus;

        public RosettaRepositoryProvider(IRosettaDataAccess rosettaDataAccess, IPackageHandler handler,
            IParameterHelper parameterHelper, RepositoryPackageBuilder builder,  IBus bus)
        {
            this.rosettaDataAccess = rosettaDataAccess;
            this.handler = handler;
            this.parameterHelper = parameterHelper;
            this.bus = bus;
            this.builder = builder;
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
                    ErrorMessage = "ArchiveRecord in Elastic not found."
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

            return new RepositoryPackageResult
            {
                Success = true
        };
        }

        public async Task<RepositoryPackageInfoResult> ReadPackageMetadata(ElasticArchiveRecord elasticArchiveRecord)
        {
            var fileshare = await rosettaDataAccess.ExportIntellectualEntity(elasticArchiveRecord.PrimaryDataLink);
            var fileUrl = $@"{fileshare}\ie.xml";
           
            if (!File.Exists(fileUrl))
            {
                return new RepositoryPackageInfoResult
                {
                    Success = false,
                    ErrorMessage = $"File {fileUrl} not found."
                };
            }

            builder.CreateMetadataXml( $@"{fileshare}", elasticArchiveRecord);

            return new RepositoryPackageInfoResult
            {
                Success = File.Exists($@"{fileshare}\metadata.xml"),
                Valid = true // ToDo
            };
        }
    }
}
