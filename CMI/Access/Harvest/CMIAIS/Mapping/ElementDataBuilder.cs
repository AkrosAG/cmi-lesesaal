using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using CMI.Access.Harvest.CMIAIS.Mapping.ElementMappings;
using CMI.Contract.Common;
using Newtonsoft.Json.Linq;

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
            { typeof(int?), new NumericMapping() },
            { typeof(int), new NumericMapping() },
            { typeof(float?), new NumericMapping() },
            { typeof(float), new NumericMapping() },
            { typeof(long?), new NumericMapping() },
            { typeof(long), new NumericMapping() },
            { typeof(double?), new NumericMapping() },
            { typeof(double), new NumericMapping() },
            { typeof(decimal?), new NumericMapping() },
            { typeof(decimal), new NumericMapping() },
            { typeof(DateTimeFieldType), new DateRangeMapping() }
        };
    }

    public ElementDataBuilder From(string name, Func<Verzeichnungseinheit, string> func)
    {
        var value = func(cmiRecord);
        CreateValueInternal(name, value);
        return this;
    }

    public ElementDataBuilder From(string name, Func<Verzeichnungseinheit, decimal?> func)
    {
        var value = func(cmiRecord);
        CreateValueInternal(name, value);
        return this;
    }

    public ElementDataBuilder From(string name, Func<Verzeichnungseinheit, int?> func)
    {
        var value = func(cmiRecord);
        CreateValueInternal(name, value);
        return this;
    }

    public ElementDataBuilder From(string name, Func<Verzeichnungseinheit, float?> func)
    {
        var value = func(cmiRecord);
        CreateValueInternal(name, value);
        return this;
    }

    public ElementDataBuilder From(string name, Func<Verzeichnungseinheit, long?> func)
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
        if (values == null || !values.Any())
            return;

        if (!mappingsByType.TryGetValue(typeof(T), out var mapping))
            throw new NotImplementedException($"No Builder is implemented for Type {typeof(T)}");

        detailData.Add(values.Count() == 1 ? mapping.CreateElement(name, values.FirstOrDefault()) : mapping.CreateElement(name, values));
    }
}