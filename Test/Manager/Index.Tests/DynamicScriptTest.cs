using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Reflection;
using CMI.Access.Common;
using CMI.Access.Common.Compiler;
using CMI.Contract.Common;
using CMI.Manager.Index.Config;
using FluentAssertions;
using Microsoft.CSharp.RuntimeBinder;
using Moq;
using Newtonsoft.Json;
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
                    .LoadScriptByDefault())
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