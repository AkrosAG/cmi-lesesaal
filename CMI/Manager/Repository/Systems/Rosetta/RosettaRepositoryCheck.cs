using System;
using CMI.Access.Repository.Systems.Rosetta;
using CMI.Contract.Monitoring;
using CMI.Manager.Repository.Properties;

namespace CMI.Manager.Repository.Systems.Rosetta
{
    public class RosettaRepositoryCheck: IRepositoryCheck
    {
        private readonly IRosettaDataAccess rosettaDataAccess;

        public RosettaRepositoryCheck(IRosettaDataAccess rosettaDataAccess)
        {
            this.rosettaDataAccess = rosettaDataAccess;
        }

        public RepositoryCheckResponse GetRepositoryResponse()
        {
           var result = rosettaDataAccess.PingRosetta().Result;
           var response = new RepositoryCheckResponse
           {
               Ok = result.Key,
               RepositoryName = result.Value,
               ProductName = Settings.Default.RepositoryManager
           };
           return response;
        }
    }
}
