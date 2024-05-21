using System.Collections.Generic;
using System.Threading.Tasks;
using CMI.Contract.Common;
using CMI.Engine.PackageMetadata.Systems.Rosetta;
using CMI.Manager.Repository.Systems.Rosetta;
using NUnit.Framework;

namespace CMI.Manager.Repository.Tests
{
    [TestFixture]
    public class RepositoryPackageBuilderTests
    {
        //[TestCase("IE610326")]
        //[TestCase("IE611480")]
        //[TestCase("IE611671")]
        //[TestCase("IE611682")]
        //[TestCase("IE611691")]
        //[TestCase("IE611696")]
        //[Ignore("Diese Tests muss bewusst bzw.Bedarf ausgeführt werden")]
        //public async Task Build_Repository_Package_returns_correct_item(string id)
        //{
        //    // Arrange
        //    var archiveRecord = new ElasticArchiveRecord
        //    {
        //        ArchiveRecordId = id,
        //        PrimaryDataLink = id,
        //        DetailData = new List<ElasticDetailData>()
        //    };

        //    // Act
        //    //var builder = new RepositoryPackageBuilder();
        //    //var package = await builder.BuildRepositoryPackageAsync(archiveRecord);

        //    // Assert
        //    //Assert.IsNotNull(package);
        //}
    } 
}