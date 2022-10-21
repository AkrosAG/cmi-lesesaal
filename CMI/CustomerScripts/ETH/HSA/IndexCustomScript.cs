using System.Collections.Generic;

namespace CMI.Contract.Common.Compiler
{
    public class IndexCustomScript : IDynamicScript
    {
        public void PostProcessArchiveRecord(ArchiveRecord archiveRecord)
        {
        }

        public void PostProcessElasticArchiveRecord(ElasticArchiveRecord elasticArchiveRecord, ArchiveRecord archiveRecord)
        {
            elasticArchiveRecord.Title = string.Format("{0} - CustomScript", elasticArchiveRecord.Title);
        }
    }
}
