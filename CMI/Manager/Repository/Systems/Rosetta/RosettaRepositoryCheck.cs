using System;
using CMI.Access.Repository.Systems.Rosetta;
using CMI.Contract.Monitoring;

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
            throw new NotImplementedException();
        }
    }
}
