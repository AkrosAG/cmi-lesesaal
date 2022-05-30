using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Serilog;

namespace CMI.Access.Harvest
{
    public class CachedLookupData
    {
        private readonly IAISDataProvider dataProvider;

        public List<FondLink> fondsOverview;

        public CachedLookupData(IAISDataProvider dataProvider)
        {
            this.dataProvider = dataProvider;

            // Load Fonds overview data 
            // TODO: Review
            fondsOverview = LoadFondsOverview().GetAwaiter().GetResult();
        }

        private async Task<List<FondLink>> LoadFondsOverview()
        {
            try
            {
                return await dataProvider.LoadFondLinks();
            }
            catch (Exception e)
            {
                Log.Error(e, "Unexpected error while loading the fonds list.");
                return new List<FondLink>();
            }
        }
    }
}