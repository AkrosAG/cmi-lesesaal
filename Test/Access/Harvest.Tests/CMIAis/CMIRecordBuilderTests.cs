using CMI.Access.Harvest.CMIAIS;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using System.Threading.Tasks;

namespace CMI.Access.Harvest.Tests.CMIAis
{
    [TestFixture]
    public class CMIRecordBuilderTests
    {
        private LanguageSettings languageSettings = null;
        private Mock<IAISDataProvider> mockAisDataProvider = null;
        private Mock<IAISSpecificRecordAccess<Verzeichnungseinheit>> aisSpecificRecordAccess = null;

        [SetUp]
        public void Setup()
        {
            languageSettings = new LanguageSettings { DefaultLanguage = System.Globalization.CultureInfo.CreateSpecificCulture("de-CH") };
            mockAisDataProvider = new Mock<IAISDataProvider>();
            aisSpecificRecordAccess = new Mock<IAISSpecificRecordAccess<Verzeichnungseinheit>>();
        }

        [Test]
        public async Task Titel_And_Id_Should_Get_Mapped()
        {
            var cmiRecord = new Verzeichnungseinheit
            {
                OBJ_GUID = "123",
                Titel = "Test"
            };

            aisSpecificRecordAccess.Setup(m => m.GetAisSpecificRecord(It.IsAny<string>()).Result).Returns(cmiRecord);
            var sut = new CMIAISArchiveRecordBuilder(mockAisDataProvider.Object, aisSpecificRecordAccess.Object, languageSettings);

            var record = await sut.Build("123");

            Assert.AreEqual(cmiRecord.OBJ_GUID, record.ArchiveRecordId);
            Assert.AreEqual(nameof(cmiRecord.Titel), record.Metadata.DetailData[0].ElementName);
            Assert.AreEqual(cmiRecord.Titel, record.Metadata.DetailData[0].ElementValue[0].TextValues[0].Value);
        }

        [Test]
        public async Task NodeData_Should_Get_Mapped()
        {
            
            var cmiRecord = new Verzeichnungseinheit
            {
                DisplayName = "Me",
                OBJ_GUID = "123",
                Children = new Child[]
                {
                    new Child
                    {
                        OBJ_GUID = "1231",
                        DisplayName = "Child 1",
                        Sortierung = "1"
                    },
                    new Child
                    {
                        OBJ_GUID = "1232",
                        DisplayName = "Child 2",
                        Sortierung = "2"
                    }
                },
                Ancestors = new ParentFieldType[]
                {
                    new ParentFieldType
                    {
                        Depth = 0,
                        OBJ_GUID = "12"
                    },
                    new ParentFieldType
                    {
                        Depth = 1,
                        OBJ_GUID = "1"
                    }
                },
                Tektonikpfad = "1 / 12 / 123"
            };

            var parent = new Verzeichnungseinheit
            {
                OBJ_GUID = "12",
                Children = new Child[]
                {
                    new Child
                    {
                        OBJ_GUID = "122",
                        DisplayName = "brother",
                        Sortierung = "1"
                    },
                    new Child
                    {
                        OBJ_GUID = "123",
                        DisplayName = "Me",
                        Sortierung = "2"
                    },
                    new Child
                    {
                        OBJ_GUID = "124",
                        DisplayName = "sister",
                        Sortierung = "3"
                    }
                }
            };

            aisSpecificRecordAccess.Setup(m => m.GetAisSpecificRecord("123").Result).Returns(cmiRecord);
            aisSpecificRecordAccess.Setup(m => m.GetAisSpecificRecord("12").Result).Returns(parent);

            var sut = new CMIAISArchiveRecordBuilder(mockAisDataProvider.Object, aisSpecificRecordAccess.Object, languageSettings);

            var record = await sut.Build("123");

            var nodeInfo = record.Metadata.NodeInfo;
            nodeInfo.ChildCount.Should().Be(cmiRecord.Children.Length);
            nodeInfo.IsLeaf.Should().BeFalse();
            nodeInfo.IsRoot.Should().BeFalse();
            nodeInfo.ParentArchiveRecordId.Should().Be("12");
            nodeInfo.Level.Should().Be(2);
            nodeInfo.Path.Should().Be(cmiRecord.Tektonikpfad);
            nodeInfo.Sequence.Should().Be(2);
        }
    }
}
