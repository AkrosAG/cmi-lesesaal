using System;

namespace CMI.Contract.Common.Exceptions
{
    public class AisRecordNotFoundException: Exception
    {
        public string ArchiveRecordId { get; set; }
    }
}
