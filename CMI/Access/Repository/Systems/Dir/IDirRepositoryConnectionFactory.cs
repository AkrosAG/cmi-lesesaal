using DotCMIS.Client;

namespace CMI.Access.Repository.Systems.Dir
{
    public interface IDirRepositoryConnectionFactory
    {
        ISession ConnectToFirstRepository();
    }
}