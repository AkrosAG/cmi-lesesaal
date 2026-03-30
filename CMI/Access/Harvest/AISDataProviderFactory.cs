using CMI.Access.Sql.Lesesaal.EF;
using System;
using System.Net.Http;
using System.Runtime.Caching;

namespace CMI.Access.Harvest
{
    public class AISDataProviderFactory : IAISDataProviderFactory
    {
        private readonly LesesaalDb dbContext;
        private readonly MemoryCache cache;
        private readonly HttpClient cdwsRequestClient;

        public AISDataProviderFactory(LesesaalDb dbContext, MemoryCache cache, HttpClient cdwsRequestClient)
        {
            this.cdwsRequestClient = cdwsRequestClient;
            this.dbContext = dbContext;
            this.cache = cache;
        }

        public IAISDataProvider Create()
        {
            IAISDataProvider dataProvider;
            switch (Properties.Settings.Default.AisProvider.ToLowerInvariant())
            {
                case "cmiais":
                    dataProvider = new CMIAIS.CMIAISDataProvider(dbContext, cache, cdwsRequestClient);
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