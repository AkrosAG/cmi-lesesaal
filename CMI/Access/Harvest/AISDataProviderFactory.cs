using System;
using CMI.Access.Sql.Lesesaal.EF;

namespace CMI.Access.Harvest
{
    public class AISDataProviderFactory : IAISDataProviderFactory
    {
        private readonly LesesaalDb dbContext;

        public AISDataProviderFactory(LesesaalDb dbContext)
        {
            this.dbContext = dbContext;
        }

        public IAISDataProvider Create()
        {
            IAISDataProvider dataProvider;
            switch (Properties.Settings.Default.AisProvider.ToLowerInvariant())
            {
                case "cmiais":
                    dataProvider = new CMIAIS.CMIAISDataProvider(dbContext);
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