using CMI.Contract.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMI.Access.Harvest.CMIAIS.Mapping
{
    public class ArchiveRecordMapperBuilder
    {
        private readonly ArchiveRecord archiveRecord;
        private readonly Verzeichnungseinheit cmiRecord;
        internal readonly LanguageSettings languageSettings;

        public ArchiveRecordMapperBuilder(Verzeichnungseinheit cmiRecord, LanguageSettings languageSettings)
        {
            archiveRecord = new ArchiveRecord
            {
                ArchiveRecordId = cmiRecord.OBJ_GUID,
            };
            this.cmiRecord = cmiRecord;
            this.languageSettings = languageSettings;
        }

        public ArchiveRecord Build()
        {
            return archiveRecord;
        }

        public MetaDataBuilder BeginMetaData()
        {
            archiveRecord.Metadata = new ArchiveRecordMetadata
            {
                AccessionDate = 0, // ToDo: Mapping
                PrimaryDataLink = "", // ToDo: Mapping
                References = null, // ToDo: CDWS Field
                Descriptors = null, // ToDo: Mapping
                Containers = null,
            };

            return new MetaDataBuilder(cmiRecord, archiveRecord, this);
        }

    }
}
