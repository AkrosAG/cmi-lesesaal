using System;

namespace CMI.Access.Harvest
{
    public class AISDataProviderFactory : IAISDataProviderFactory
    {
        public IAISDataProvider Create()
        {
            IAISDataProvider dataProvider;
            switch (Properties.Settings.Default.AisProvider.ToLowerInvariant())
            {
                case "cmiais":
                    dataProvider = new CMIAIS.CMIAISDataProvider();
                    break;
                case "scopeais":
                    dataProvider = new ScopeArchiv.ScopeAISDataProvider();
                    break;
                default:
                    throw new NotImplementedException($"{Properties.Settings.Default.AisProvider} is not a supported AisProvider");
            }

            return dataProvider;
        }
    }
}