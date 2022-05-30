using System;
using System.Collections.Generic;
using CMI.Access.Harvest.CMIAIS.Mapping.ElementMappings;
using CMI.Contract.Common;

namespace CMI.Access.Harvest.CMIAIS.Mapping;

public class DetailDataBuilder
{
    private readonly Verzeichnungseinheit cmiRecord;
    private readonly ArchiveRecord archiveRecord;
    private readonly MetaDataBuilder metaDataBuilder;
    private readonly DetailData detailData;

    public DetailDataBuilder(Verzeichnungseinheit cmiRecord, ArchiveRecord archiveRecord, MetaDataBuilder metaDataBuilder)
    {
        this.cmiRecord = cmiRecord;
        this.archiveRecord = archiveRecord;
        this.metaDataBuilder = metaDataBuilder;
        detailData = new DetailData
        {
            DataElement = new List<DataElement>()
        };
    }

    public ElementDataBuilder WithMappings()
    {
        return new ElementDataBuilder(cmiRecord, detailData, this, metaDataBuilder.archiveRecordMapperBuilder.languageSettings);
    }

    public MetaDataBuilder EndDetailData()
    {
        archiveRecord.Metadata.DetailData = detailData.DataElement;
        return metaDataBuilder;
    }
}

public class ElementDataBuilder
{
    private readonly Verzeichnungseinheit cmiRecord;
    private readonly DetailData detailData;
    private readonly DetailDataBuilder detailDataBuilder;
    private readonly Dictionary<Type, BaseMapping> mappingsByType;
    public ElementDataBuilder(Verzeichnungseinheit cmiRecord, DetailData detailData, DetailDataBuilder detailDataBuilder, LanguageSettings languageSettings)
    {
        this.cmiRecord = cmiRecord;
        this.detailData = detailData;
        this.detailDataBuilder = detailDataBuilder;
        mappingsByType = new Dictionary<Type, BaseMapping>
        {
            { typeof(string), new TextMapping() },
            { typeof(bool?), new BoolMapping(languageSettings) },
            { typeof(DateTimeFieldType), new DateRangeMapping() },
            { typeof(CustomField), new CustomFieldMapping() }
        };
    }

    public ElementDataBuilder From(string name, Func<Verzeichnungseinheit, string> func)
    {
        var value = func(cmiRecord);
        CreateValueInternal(name, value);
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
        foreach (var customField in cmiRecord.CustomFields)
        {
            foreach (var element in customField.Any)
            {
                CreateValueInternal(element.GetAttribute("name"), element);
            }
        }

        return this;
    }

    public DetailDataBuilder EndMappings()
    {
        return detailDataBuilder;
    }
    
    private void CreateValueInternal(string name, object value)
    {
        if (!mappingsByType.TryGetValue(value.GetType(), out var mapping))
            throw new NotImplementedException($"No Builder is implemented for Type {value.GetType()}");

        var data = mapping.CreateElement(name, value);
        detailData.DataElement.Add(data);
    }

}