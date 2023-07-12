using System;
using System.Collections.Generic;
using CMI.Contract.Common;
using static System.Net.Mime.MediaTypeNames;

namespace CMI.Access.Harvest.CMIAIS.Mapping.ElementMappings;

public class TextMapping : BaseMapping
{
    public override DataElement CreateElement(string name, object value)
    {
        var element = new DataElement
        {
            ElementValue = new List<DataElementElementValue>(),
            ElementType = DataElementElementType.text,
            ElementName = name
        };

        if (value is IEnumerable<string> arrayValues)
        {
            foreach (var arrayValue in arrayValues)
            {
                element.ElementValue.Add(new DataElementElementValue
                {
                    TextValues = new List<DataElementElementValueTextValue>
                    {
                        new()
                        {
                            Value = arrayValue,
                            Lang = "de-CH",
                            IsDefaultLang = true
                        }
                    }
                });
            }
        }
        else
        {
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
        }

        return element;
    }
}