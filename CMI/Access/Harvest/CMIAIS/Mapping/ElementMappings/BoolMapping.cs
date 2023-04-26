using System.Collections.Generic;
using System.Resources;
using CMI.Access.Harvest.Properties;
using CMI.Contract.Common;

namespace CMI.Access.Harvest.CMIAIS.Mapping.ElementMappings;

public class BoolMapping : BaseMapping
{
    private readonly LanguageSettings languageSettings;
    private readonly ResourceManager resourceManager;
    
    public BoolMapping(LanguageSettings languageSettings)
    {
        this.languageSettings = languageSettings;
        resourceManager = new ResourceManager(typeof(Resources));
    }
    
    public override DataElement CreateElement(string name, object value)
    {
        var element = new DataElement
        {
            ElementType = DataElementElementType.boolean,
            ElementName = name
        };

        var boolVal = (bool?) value;
        if (boolVal == null)
            return element;
        
        var elementElement = new DataElementElementValue
        {
            BooleanValue = boolVal.Value,
        };

        elementElement.TextValues.Add(new DataElementElementValueTextValue
        {
            Value = boolVal.Value ? resourceManager.GetString("BooleanYes", languageSettings.DefaultLanguage) : resourceManager.GetString("BooleanNo", languageSettings.DefaultLanguage),
            IsDefaultLang = true,
            Lang = languageSettings.DefaultLanguage.TwoLetterISOLanguageName 
        });
        
        foreach (var language in languageSettings.SupportedLanguages)
        {
            elementElement.TextValues.Add(new DataElementElementValueTextValue
            {
                Value = boolVal.Value ? resourceManager.GetString("BooleanYes", language) : resourceManager.GetString("BooleanNo", language),
                Lang = language.TwoLetterISOLanguageName
            });
        }

        element.ElementValue.Add(elementElement);
        return element;
    }
}