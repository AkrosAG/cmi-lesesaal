using System;

namespace CMI.Access.Repository.Systems.Dir
{
    public class DirRepositoryConnectionException : Exception
    {
        public DirRepositoryConnectionException(Exception innerException) : base("Unable to connect to repository", innerException)
        {
        }
    }
}