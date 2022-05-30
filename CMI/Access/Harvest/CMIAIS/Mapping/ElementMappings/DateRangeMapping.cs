using System;
using System.Collections.Generic;
using CMI.Contract.Common;

namespace CMI.Access.Harvest.CMIAIS.Mapping.ElementMappings;

public class DateRangeMapping: BaseMapping
{
    public override DataElement CreateElement(string name, object value)
    {
        var element = new DataElement
        {
            ElementValue = new List<DataElementElementValue>(),
            ElementType = DataElementElementType.dateRange,
            ElementName = name,
        };

        var datetime = value as DateTimeFieldType;

        if (datetime == null)
            return element;

        var elementElement = new DataElementElementValue
        {
            DateRange = new DateRange
            {
                FromDate = datetime.Start ?? DateTime.MinValue,
                ToDate = datetime.End ?? DateTime.MaxValue,
                SearchFromDate = datetime.Start ?? DateTime.MinValue,
                SearchToDate = datetime.End ?? DateTime.MaxValue,
                DateOperator = DateRangeDateOperator.fromTo,
                From = datetime.Start != null ? datetime.Start.Value.ToString("+yyyyMMdd") : "+0", // ToDo: Check
                To = datetime.End != null ? datetime.End.Value.ToString("+yyyyMMdd") : "+0",
                FromApproxIndicator = false,
                ToApproxIndicator = false
            }
        };

        element.ElementValue.Add(elementElement);
        return element;
    }


}