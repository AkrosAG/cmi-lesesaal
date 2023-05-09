using System;
using CMI.Web.Frontend.api.Elastic;
using FluentAssertions;
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
    }
}