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
            // Facetten
            elasticArchiveRecord.Facetten.Text01 = elasticArchiveRecord.Permission;

            // All Text Values Field all keywords Value Fields

        }
    }
}
