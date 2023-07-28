using CMI.Contract.Common;

namespace CMI.Access.Harvest.CMIAIS.Mapping.ElementMappings;

public abstract class BaseMapping
{
    public abstract DataElement CreateElement(string name, object value);
}
