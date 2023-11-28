
using CMI.Access.Repository;
using DotCMIS.Client;

namespace CMI.Manager.Repository.Mock
{
    public class MockRepositoryConnectionFactory : IRepositoryConnectionFactory
    {
        public ISession ConnectToFirstRepository()
        {
            return null;
        }
    }
}
