using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CMI.Access.Repository.Systems.Rosetta;
using CMI.Contract.Common;
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

        public Task<RepositoryPackageResult> GetPackage(string packageId, string archiveRecordId, bool createMetadataXml, List<string> fileTypesToIgnore, int primaerdatenAuftragId)
        {
            throw new NotImplementedException();
        }

        public RepositoryPackageInfoResult ReadPackageMetadata(string packageId, string archiveRecordId)
        {
            var result=rosettaDataAccess.ExportIntellectualEntity(packageId);
            // ToDo: DLS-333 Rosetta-Anbindung (Export einer IntellectualEntity)
            return null;
        }
    }
}
