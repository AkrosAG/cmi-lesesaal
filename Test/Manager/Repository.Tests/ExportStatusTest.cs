using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using CMI.Access.Repository.Systems.Rosetta;
using CMI.Contract.Common.Gebrauchskopie;
using CMI.Engine.PackageMetadata.Systems.Dir;
using FluentAssertions;
using NUnit.Framework;

namespace CMI.Manager.Repository.Tests
{
    [TestFixture]
    public class ExportStatusTest
    {
        [Test]
        public void Xml_responds_returns_correct_values()
        {
            // Arrange
            var xmlString = File.ReadAllText((Path.Combine(TestContext.CurrentContext.TestDirectory, "reponse.xml")));

            // Act
            var xmlDoc = XDocument.Parse(xmlString);
            var node = xmlDoc.Descendants("info")
                                  .FirstOrDefault(e => e.Attribute("desc")?.Value == "process_instance_id_link");

            var processInstanceIdLink = node?.Value;

            // Assert
            processInstanceIdLink.Should().Be("rest/v0/conf/processes/70218297/instances/70218298");
        }
    }
}
