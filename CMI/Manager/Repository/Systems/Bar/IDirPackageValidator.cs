using CMI.Contract.Common;

namespace CMI.Manager.Repository.Systems.Bar
{
    public interface IDirPackageValidator
    {
        void EnsureValidPhysicalFileAndFolderNames(RepositoryPackage package, string rootFolderName);
    }
}