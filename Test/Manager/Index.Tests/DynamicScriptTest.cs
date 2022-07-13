using CMI.Contract.Common;
using CMI.Contract.Common.Compiler;
using Moq;
using NUnit.Framework;

namespace CMI.Manager.Index.Tests
{
    [TestFixture]
    public class DynamicScriptTest
    {
        [Test]
        [Ignore("incomplete")]
        public void Test_IndexManager_Should_Fill_CustomFields_Correctly()
        {
            var scriptCode = @"
            public class MyCustomClass : IDynamicScript
            {
                public void PostProcessArchiveRecord(ArchiveRecord archiveRecord)
                {
                }

                public void PostProcessElasticArchiveRecord(ElasticArchiveRecord elasticArchiveRecord, ArchiveRecord archiveRecord)
                {
                }
            }";

            // Arrange
            var mockDynamicScriptLocator  = new Mock<IDynamicScriptLocator>();
            
            mockDynamicScriptLocator.Setup(s => s
                    .GetCustomScript())
                .Returns(() => { return scriptCode; });

            // Act

            var archiveRecord = new ArchiveRecord();
            var elasticRecord = new ElasticArchiveRecord();
            
            var provider = new DynamicScriptProvider(mockDynamicScriptLocator.Object);
            var script = provider.GetInstanceByType<IDynamicScript>();

            script.PostProcessElasticArchiveRecord(elasticRecord, archiveRecord);
        }
    }
}