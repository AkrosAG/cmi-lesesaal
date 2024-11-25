using System.Collections.Generic;

namespace CMI.Contract.Common
{
    public partial class ArchiveRecord
    {
        public List<ElasticArchiveRecordPackage> ElasticPrimaryData { get; set; }
        public bool HasArchiveRecordBuildError { get; set; }
        public string ArchiveRecordBuildErrorMessage { get; set; }
    }
}