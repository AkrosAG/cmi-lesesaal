using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CMI.Access.Repository.Systems.Dir;
using CMI.Contract.Monitoring;
using MassTransit;

namespace CMI.Manager.Repository.Systems.Bar
{
    public class DirRepositoryCheck: IRepositoryCheck
    {
        private readonly IDirRepositoryConnectionFactory connectionFactory;

        public DirRepositoryCheck(IDirRepositoryConnectionFactory connectionFactory)
        {
            this.connectionFactory = connectionFactory;
        }
        public RepositoryCheckResponse GetRepositoryResponse()
        {
            var response = new RepositoryCheckResponse();
            var session = connectionFactory.ConnectToFirstRepository();

            response.Ok = session != null;

            if (session != null)
            {
                var repositoryInfo = session.RepositoryInfo;

                response.RepositoryName = repositoryInfo.Name;
                response.ProductVersion = repositoryInfo.ProductVersion;
                response.ProductName = repositoryInfo.ProductName;
            }

            return response;
        }
    }
}
