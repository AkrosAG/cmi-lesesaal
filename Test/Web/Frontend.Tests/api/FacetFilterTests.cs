using System;
using System.Linq;
using System.Text;
using CMI.Contract.Common;
using CMI.Web.Frontend.api.Configuration;
using CMI.Web.Frontend.api.Elastic;
using CMI.Web.Frontend.api.Interfaces;
using CMI.Web.Frontend.API.Tests.ElasticMock;
using Elasticsearch.Net;
using FluentAssertions;
using Moq;
using Nest;
using Nest.JsonNetSerializer;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NUnit.Framework;

namespace CMI.Web.Frontend.API.Tests.api
{
    [TestFixture]
    internal class FacetFilterTests
    {
        
        [Test]
        public void Vergleichswert_wird_Escaped()
        {
            var secured = ElasticService.GetSecuredFacetFilters(new[] {"level:Dossier:123"});

            secured.Should().BeEquivalentTo("level:Dossier\\:123");
        }

        [Test]
        public void Load_Facetten_Test()
        { 
            // arrange
            var elasticSettings = new Mock<IElasticSettings>();

            // act
            var service = new ElasticServiceWithFacettenBackdoor(elasticSettings.Object, TestResources.Facetten);

            // assert
            service.Should().NotBeNull();
            service.Facetten.Should().NotBeNull();
            service.Facetten.Count.Should().Be(10);
        }

        [Test]
        public void Load_Facetten_SmallList_Test()
        {
            // arrange
            var elasticSettings = new Mock<IElasticSettings>();

            // act
            var service = new ElasticServiceWithFacettenBackdoor(elasticSettings.Object, TestResources.Facetten_SmallList);

            // assert
            service.Should().NotBeNull();
            service.Facetten.Should().NotBeNull();
            service.Facetten.Count.Should().Be(8);
        }

        [Test]
        public void Create_Aggregation_Test()
        {
            // arrange
            var elasticSettings = new Mock<IElasticSettings>();
            var service = new ElasticServiceWithFacettenBackdoor(elasticSettings.Object, TestResources.Facetten);

            // act
            var result = service.TestAggregationCreation(null);

            // assert
            service.Should().NotBeNull();
            service.Facetten.Should().NotBeNull();
            service.Facetten.Count.Should().Be(result.Count());
        }
    }
}