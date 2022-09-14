using System;
using System.Runtime.Caching;
using CMI.Access.Sql.Lesesaal.EF;

namespace CMI.Access.Harvest
{
    public class AISDataProviderFactory : IAISDataProviderFactory
    {
        private readonly LesesaalDb dbContext;
        private readonly MemoryCache cache;

        public AISDataProviderFactory(LesesaalDb dbContext, MemoryCache cache)
        {
            this.dbContext = dbContext;
            this.cache = cache;
        }

        public IAISDataProvider Create()
        {
            IAISDataProvider dataProvider;
            switch (Properties.Settings.Default.AisProvider.ToLowerInvariant())
            {
                case "cmiais":
                    dataProvider = new CMIAIS.CMIAISDataProvider(dbContext, cache);
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