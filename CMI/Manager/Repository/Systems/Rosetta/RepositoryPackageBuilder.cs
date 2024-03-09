using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using CMI.Access.Repository.Systems.Rosetta;
using CMI.Contract.Common;
using CMI.Contract.Parameter;
using CMI.Contract.Repository;
using MassTransit;
using Serilog;

namespace CMI.Manager.Repository.Systems.Rosetta
{
    public class RepositoryPackageBuilder
    {
        private readonly IRosettaDataAccess rosettaDataAccess;
        private readonly IBus bus;

        public RepositoryPackageBuilder(IRosettaDataAccess rosettaDataAccess, IBus bus)
        {
            this.rosettaDataAccess = rosettaDataAccess;
            this.bus = bus;
        }

        public async Task<RepositoryPackage> BuildAsync(string fileUrl)
        {
            var doc = XDocument.Load(fileUrl);
            return await Task.FromResult<RepositoryPackage>(null);
        }
    }
}
