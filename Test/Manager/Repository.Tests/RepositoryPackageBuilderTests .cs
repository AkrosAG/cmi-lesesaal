using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Threading.Tasks;
using CMI.Access.Repository.Systems.Rosetta;
using CMI.Contract.Common;
using CMI.Contract.Common.Gebrauchskopie;
using CMI.Engine.PackageMetadata.Systems.Dir;
using CMI.Manager.Repository.Systems.Rosetta;
using FluentAssertions;
using NUnit.Framework;

namespace CMI.Manager.Repository.Tests
{
    [TestFixture]
    public class RepositoryPackageBuilderTests
    {
        [Test]
        public async Task Build_Repository_Package_returns_correct_item()
        {
            // Arrange
            var fileshare = @"C:\Temp\Repository";
            var fileUrl = $@"{fileshare}\IE444295\ie.xml";
            var archiveRecord = new ElasticArchiveRecord
            {
                ArchiveRecordId = "IE444295",
                DetailData = new List<ElasticDetailData>()
            };

            // Act
            var builder = new RepositoryPackageBuilder(null, null);
            await builder.BuildRepositoryPackageAsync(fileUrl, archiveRecord);

            // Assert
        }
    }
}