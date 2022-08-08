using System.Collections.Generic;
using CMI.Contract.Common;

namespace CMI.Access.Harvest.CMIAIS.Mapping.ElementMappings;

public class TextMapping : BaseMapping
{
    public override DataElement CreateElement(string name, object value)
    {
        var element = new DataElement
        {
            ElementValue = new List<DataElementElementValue>(),
            ElementType = DataElementElementType.text,
            ElementName = name,
            ElementId = name
        };

        var text = value as string;
        if (value == null)
            return element;

        element.ElementValue.Add(new DataElementElementValue
        {
            TextValues = new List<DataElementElementValueTextValue>
            {
                new()
                {
                    Value = text,
                    Lang = "de-CH",
                    IsDefaultLang = true
                }
            }
        });

        return element;
    }
}