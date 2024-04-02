using System.Collections.Generic;
using System.Threading.Tasks;
using CMI.Contract.Common;
using CMI.Manager.Repository.Systems.Rosetta;
using NUnit.Framework;

namespace CMI.Manager.Repository.Tests
{
    [TestFixture]
    public class RepositoryPackageBuilderTests
    {
        [TestCase("IE610326")]
        [TestCase("IE611480")]
        [TestCase("IE611671")]
        [TestCase("IE611682")]
        [TestCase("IE611691")]
        [TestCase("IE611696")]
        public async Task Build_Repository_Package_returns_correct_item(string id)
        {
            // Arrange
            var fileshare = @"C:\Temp\Repository";
            var fileUrl = $@"{fileshare}\{id}\ie.xml";
            var archiveRecord = new ElasticArchiveRecord
            {
                ArchiveRecordId = id,
                DetailData = new List<ElasticDetailData>()
            };

            // Act
            var builder = new RepositoryPackageBuilder(null, null);
            await builder.BuildRepositoryPackageAsync(fileUrl, archiveRecord);

            // Assert
        }
    }
}