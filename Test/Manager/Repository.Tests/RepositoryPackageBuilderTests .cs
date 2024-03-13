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
            var fileshare = @"\\nas12.ethz.ch\ethbib_rosetta_test_vls_transfer_s1\vls";
            var fileUrl = $@"{fileshare}\IE268715\ie.xml";
            
            // Act
            var builder = new RepositoryPackageBuilder(null, null);
            await builder.BuildAsync(fileUrl,new ElasticArchiveRecord());

            // Assert
        }
    }
}