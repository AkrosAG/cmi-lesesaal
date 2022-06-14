using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Reflection;
using CMI.Access.Common;
using CMI.Contract.Common;
using CMI.Manager.Index.Compiler;
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
        public void Test_IndexManager_Should_Fill_CustomFields_Correctly()
        {
            var scriptCode = @"
            public class MyCustomClass : ICustomType
            {
                public void Execute(ArchiveRecord archiveRecord, ElasticArchiveRecord elasticArchiveRecord)
                {
                   elasticArchiveRecord.LastSyncDate = DateTime.UtcNow;
                }
            }
            ";

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

            script.Execute(archiveRecord, elasticRecord);
        }
    }
}