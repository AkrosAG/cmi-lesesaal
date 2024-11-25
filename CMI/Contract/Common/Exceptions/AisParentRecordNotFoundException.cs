using System;

namespace CMI.Contract.Common.Exceptions;

public class AisParentRecordNotFoundException : Exception
{
    public string ParentRecordId { get; set; }
}