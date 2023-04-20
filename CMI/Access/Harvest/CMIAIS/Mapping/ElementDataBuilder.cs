using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using CMI.Access.Harvest.CMIAIS.Mapping.ElementMappings;
using CMI.Contract.Common;

namespace CMI.Access.Harvest.CMIAIS.Mapping;

public class ElementDataBuilder
{
    private readonly Verzeichnungseinheit cmiRecord;
    private readonly List<DataElement> detailData;
    private readonly Dictionary<Type, BaseMapping> mappingsByType;
    public ElementDataBuilder(Verzeichnungseinheit cmiRecord, List<DataElement> detailData, LanguageSettings languageSettings)
    {
        this.cmiRecord = cmiRecord;
        this.detailData = detailData;
        mappingsByType = new Dictionary<Type, BaseMapping>
        {
            { typeof(string), new TextMapping() },
            { typeof(bool?), new BoolMapping(languageSettings) },
            { typeof(bool), new BoolMapping(languageSettings) },
            { typeof(DateTimeFieldType), new DateRangeMapping() },
            { typeof(XmlElement), new CustomFieldMapping() }
        };
    }

    public ElementDataBuilder From(string name, Func<Verzeichnungseinheit, string> func)
    {
        var value = func(cmiRecord);
        CreateValueInternal(name, value);
        return this;
    }

    public ElementDataBuilder FromCollection<T>(string name, Func<Verzeichnungseinheit, IEnumerable<T>> func)
    {
        var value = func(cmiRecord);
        CreateValueListInternal(name, value);
        return this;
    }

    public ElementDataBuilder From(string name, Func<Verzeichnungseinheit, bool?> func)
    {
        var value = func(cmiRecord);
        CreateValueInternal(name, value);
        return this;
    }
    
    public ElementDataBuilder From(string name, Func<Verzeichnungseinheit, DateTimeFieldType> func)
    {
        var value = func(cmiRecord);
        CreateValueInternal(name, value);
        return this;
    }

    public ElementDataBuilder FromCustomFields()
    {
        if (cmiRecord.CustomFields == null || cmiRecord.CustomFields.ChildNodes.Count == 0)
            return this;

        foreach (var element in cmiRecord.CustomFields.ChildNodes.OfType<XmlElement>())
        {
            CreateValueInternal(element.GetAttribute("name"), element);
        }

        return this;
    }
    
    private void CreateValueInternal(string name, object value)
    {
        if (value == null)
            return;

        if (!mappingsByType.TryGetValue(value.GetType(), out var mapping))
            throw new NotImplementedException($"No Builder is implemented for Type {value.GetType()}");

        var data = mapping.CreateElement(name, value);
        detailData.Add(data);
    }

    private void CreateValueListInternal<T>(string name, IEnumerable<T> values)
    {
        if (values == null)
            return;

        if (!mappingsByType.TryGetValue(typeof(T), out var mapping))
            throw new NotImplementedException($"No Builder is implemented for Type {typeof(T)}");

        foreach (var value in values)
        {
            var data = mapping.CreateElement(name, value);
            detailData.Add(data);
        }
    }
}