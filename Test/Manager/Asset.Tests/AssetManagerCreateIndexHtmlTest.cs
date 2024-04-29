using System.Collections.Generic;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CMI.Access.Sql.Lesesaal;
using CMI.Contract.Common;
using CMI.Contract.Messaging;
using CMI.Contract.Parameter;
using CMI.Engine.Asset;
using CMI.Engine.Security;
using CMI.Manager.Asset.ParameterSettings;
using FluentAssertions;
using MassTransit;
using Moq;
using NUnit.Framework;

namespace CMI.Manager.Asset.Tests
{
    [TestFixture]
    public class AssetManagerCreateIndexHtmlTest
    {
        [TestCase(@"C:\Temp\Final\f21510358da444888e9a6a7adb251300", "IE611508")]
        public void Create_IndexHtml_with_content(string sourcefolder, string packageId)
        {
            var targetFilename = Path.Combine(sourcefolder, "index.html");
            // Arrange
            var assetManager = CreateAssetManager();

            // Act
            File.Delete(Path.Combine(targetFilename));
            assetManager.CreateIndexHtml(sourcefolder, packageId);

            // Assert
            FileAssert.Exists(targetFilename);
        }

        private static AssetManager CreateAssetManager()
        {
            var textEngineMock = new Mock<ITextEngine>();
            var renderEngineMock = new Mock<IRenderEngine>();
            var transformEngineMock = new Mock<ITransformEngine>();
            var passwordHelper = new PasswordHelper("just a test");
            var paramHelperMock = new Mock<IParameterHelper>();
            paramHelperMock.Setup(s => s.GetSetting<SchaetzungAufbereitungszeitSettings>()).Returns(new SchaetzungAufbereitungszeitSettings
            { KonvertierungsgeschwindigkeitVideo = 1, KonvertierungsgeschwindigkeitAudio = 1 });
            paramHelperMock.Setup(s => s.GetSetting<AssetPriorisierungSettings>()).Returns(new AssetPriorisierungSettings
            {
                PackageSizes = @"
                        {
	                        ""MaxSmallSizeInMB"": 250,
	                        ""MaxMediumSizeInMB"": 1000,
	                        ""MaxLargeSizeInMB"": 4000,
	                        ""ExtraLargeSizeInMB"": 2147483647
                        }"
            });
            var pdfManipulatorMock = new Mock<IPdfManipulator>();
            var preparationTimeCalculator = new Mock<IPreparationTimeCalculator>();
            var auftragAccess = new Mock<IPrimaerdatenAuftragAccess>();
            auftragAccess.Setup(e => e.CreateOrUpdateAuftrag(It.IsAny<PrimaerdatenAuftrag>())).Returns(Task.FromResult(1));
            auftragAccess.Setup(e => e.GetLaufendenAuftrag("1", AufbereitungsArtEnum.Download)).Returns(Task.FromResult(
                new PrimaerdatenAuftragStatusInfo
                { PrimaerdatenAuftragId = 1, AufbereitungsArt = AufbereitungsArtEnum.Download }));
            auftragAccess.Setup(e => e.GetLaufendenAuftrag("2", AufbereitungsArtEnum.Download))
                .Returns(Task.FromResult<PrimaerdatenAuftragStatusInfo>(null));
            auftragAccess.Setup(e => e.UpdateStatus(It.IsAny<PrimaerdatenAuftragLog>(), 0)).Returns(Task.FromResult(1));
            var indexClient = new Mock<IRequestClient<FindArchiveRecordRequest>>();
            var response = new Mock<Response<FindArchiveRecordResponse>>();
            response.Setup(r => r.Message).Returns(new FindArchiveRecordResponse
            { ArchiveRecordId = "1", ElasticArchiveRecord = new ElasticArchiveRecord() });
            indexClient.Setup(e => e.GetResponse<FindArchiveRecordResponse>(It.IsAny<FindArchiveRecordRequest>(), It.IsAny<CancellationToken>(), It.IsAny<RequestTimeout>())).Returns(
                Task.FromResult(response.Object));

            preparationTimeCalculator
                .Setup(s => s.EstimatePreparationDuration(It.IsAny<List<ElasticArchiveRecordPackage>>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(TimeSpan.FromMinutes(1));

            var transformEngine = new TransformEngine(new Xsl2Processor()); ;

            var assetManager = new AssetManager(textEngineMock.Object, renderEngineMock.Object, transformEngine, passwordHelper,
                paramHelperMock.Object, pdfManipulatorMock.Object, preparationTimeCalculator.Object, auftragAccess.Object, indexClient.Object,
                null, null);
            return assetManager;
        }
    }
}