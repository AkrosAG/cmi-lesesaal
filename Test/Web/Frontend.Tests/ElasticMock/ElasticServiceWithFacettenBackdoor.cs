using System;
using CMI.Web.Frontend.api.Elastic;
using System.Collections.Generic;
using System.Text;
using CMI.Contract.Common;
using CMI.Web.Frontend.api.Interfaces;
using Elasticsearch.Net;
using Moq;
using Nest;
using Nest.JsonNetSerializer;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace CMI.Web.Frontend.API.Tests.ElasticMock
{
    internal class ElasticServiceWithFacettenBackdoor : ElasticService
    {
        private static object mockResponse = new
        {
            took = 1,
            timed_out = false,
            _shards = new
            {
                total = 2,
                successful = 2,
                failed = 0
            },
            hits = new
            {
                total = new { value = 1 },
                max_score = 1.0,
                hits = new[]
                {
                    new
                    {
                        _index = "project",
                        _type = "project",
                        _id = "Project",
                        _score = 1.0,
                        _source = new TreeRecord
                        {
                            ArchiveRecordId = "1",
                            PrimaryDataFulltextAccessTokens = new List<string> {AccessRoles.RoleBAR}
                        },
                        highlight = new
                        {
                            title = new[] {"<em>Fundstelle</em>"},
                            all_Metadata_Text = new[] {"<em>Fundstelle</em>", "Dies ist eine andere <em>Fundstelle</em>"},
                            all_Primarydata = new[] {"<em>Fundstelle</em> in den Primärdaten"}
                        }
                    }
                }
            }
        };

        public ElasticServiceWithFacettenBackdoor( IElasticSettings elasticSettings, string uniteTestConfig = null) : base(CreateClientProvider(mockResponse), elasticSettings, uniteTestConfig)
        {
        }

        internal List<Facette> Facetten
        {
            get
            {
                return facetten;


            }
        }


        private static IElasticClientProvider CreateClientProvider(object responseMock)
        {
            var providerMock = new Mock<IElasticClientProvider>();
            providerMock.Setup(m =>
                m.GetElasticClient(It.IsAny<IElasticSettings>(), It.IsAny<ElasticQueryResult<TreeRecord>>())).Returns(
                () =>
                {
                    var response = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(responseMock));
                    var connection = new InMemoryConnection(response);
                    var connectionPool = new SingleNodeConnectionPool(new Uri("http://localhost:9200"));
                    var settings = new ConnectionSettings(connectionPool, connection,
                        (serializer, values) => new JsonNetSerializer(
                            serializer, values, null, null,
                            new[] { new ExpandoObjectConverter() }));

                    return new ElasticClient(settings);
                });

            return providerMock.Object;
        }
    }
}
