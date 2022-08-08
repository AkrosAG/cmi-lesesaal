using System;
using System.Collections.Generic;
using System.Xml;
using CMI.Contract.Common;

namespace CMI.Access.Harvest.CMIAIS.Mapping.ElementMappings;

public abstract class BaseMapping
{
    public abstract DataElement CreateElement(string name, object value);
}

public class CustomFieldMapping : BaseMapping
{
    public override DataElement CreateElement(string name, object value)
    {
        if (value is not XmlElement xmlElement)
            return null;
        
        var element = new DataElement
        {
            ElementValue = new List<DataElementElementValue>(),
            ElementType = ToDataElementType(xmlElement.GetAttribute("type")),
            ElementName = name,
            ElementId = name,
        };

        var elementValue = xmlElement.Value;
        var elementElement = new DataElementElementValue
        {
            TextValues = new List<DataElementElementValueTextValue>{ new () { Value = elementValue } } 
        };
        
        element.ElementValue.Add(elementElement);
        return element;
    }
    
    private DataElementElementType ToDataElementType(string xsdType)
    {
        switch (xsdType.ToLowerInvariant())
        {
            case "xsd:string":
                return DataElementElementType.text;
            case "xsd:dateTime":
                return DataElementElementType.date;
            case "datetimefieldtype":
                return DataElementElementType.timespan;
            case "xsd:boolean":
                return DataElementElementType.boolean;
            case "xsd:decimal":
                return DataElementElementType.@float;
            case "documentfieldtype":
                return DataElementElementType.media; // ToDo: Check
            default:
                throw new NotImplementedException("unknown datatype " + xsdType);
        }
    }
}