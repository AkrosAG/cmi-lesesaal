using System.Collections.Generic;
using CMI.Contract.Common;
using Serilog;

namespace CMI.Access.Harvest.CMIAIS.Mapping.ElementMappings
{
    public class NumericMapping : BaseMapping
    {
        public override DataElement CreateElement(string name, object value)
        {
            var element = new DataElement
            {
                ElementValue = new List<DataElementElementValue>(),
                ElementName = name
            };
            switch (value)
            {
                case float:
                case decimal:
                case double:
                case long:
                {
                    element.ElementType = DataElementElementType.@float;

                    if (!float.TryParse(value.ToString(), out var floValue))
                        return element;

                    element.ElementValue.Add(new DataElementElementValue
                    {
                        FloatValue = new DataElementElementValueFloatValue
                        {
                            Value = floValue
                        }
                    });
                    break;
                }
                case int:
                {
                    element.ElementType = DataElementElementType.integer;

                    if (!int.TryParse(value.ToString(), out var intValue))
                        return element;

                    element.ElementValue.Add(new DataElementElementValue
                    {
                        IntValue = intValue
                    });
                    break;
                }
                default:
                    Log.Warning($"The value is not a number: {value} {value.GetType()} ");
                    break;
            }

            return element;
        }
    } 
}