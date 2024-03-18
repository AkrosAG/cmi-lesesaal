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
        private readonly IPackageHandler handler;
        private readonly IParameterHelper parameterHelper;
        private readonly RepositoryPackageBuilder builder;
        private readonly IBus bus;

        public RosettaRepositoryProvider(IRosettaDataAccess rosettaDataAccess, IPackageHandler handler,
            IParameterHelper parameterHelper, RepositoryPackageBuilder builder, IBus bus)
        {
            this.rosettaDataAccess = rosettaDataAccess;
            this.handler = handler;
            this.parameterHelper = parameterHelper;
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
            // Test IEs Must remove
            elasticArchiveRecord.PrimaryDataLink = new Random().Next(0, 1) == 1 ? "IE444295" : "IE607568";
            var success = await rosettaDataAccess.ExportIntellectualEntity(Settings.Default.TempStoragePath, elasticArchiveRecord.PrimaryDataLink);

            var fileUrl = $"{Path.Combine(Settings.Default.TempStoragePath, elasticArchiveRecord.PrimaryDataLink)}";


            var package = await builder.BuildRepositoryPackageAsync(fileUrl, elasticArchiveRecord);

            return new RepositoryPackageInfoResult
            {
                Success = success,
                PackageDetails = package,
                Valid = success 
            };
        }
    }
}
