using System.Collections.Generic;

namespace CMI.Contract.Common.Compiler
{
    public class MyCustomClass : IDynamicScript
    {
        public void PostProcessArchiveRecord(ArchiveRecord archiveRecord)
        {
            archiveRecord.Security.MetadataAccessToken = new List<string>(new[] { "BAR" });
            archiveRecord.Security.PrimaryDataDownloadAccessToken = new List<string>();
            archiveRecord.Security.PrimaryDataFulltextAccessToken = new List<string>();
        }

        public void PostProcessElasticArchiveRecord(ElasticArchiveRecord elasticArchiveRecord, ArchiveRecord archiveRecord)
        {
        }
    }
}
