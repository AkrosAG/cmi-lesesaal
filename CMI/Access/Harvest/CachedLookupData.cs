using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Serilog;

namespace CMI.Access.Harvest
{
    public class CachedLookupData
    {
        private readonly IAISDataProvider dataProvider;

        private List<FondLink> fondsOverview;
        private readonly SemaphoreSlim semaphore = new SemaphoreSlim(1);
        private DateTime timeStamp = DateTime.MinValue;

        public CachedLookupData(IAISDataProvider dataProvider)
        {
            this.dataProvider = dataProvider;
        }

        public async Task<List<FondLink>> LoadFondsOverviewCached()
        {
            try
            {
                if (ShouldReloadData())
                {
                    await semaphore.WaitAsync();

                    try
                    {
                        if (ShouldReloadData())
                        {
                            fondsOverview = await dataProvider.LoadFondLinks();
                            timeStamp = DateTime.Now;
                        }
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                }

                return fondsOverview;
            }
            catch (Exception e)
            {
                Log.Error(e, "Unexpected error while loading the fonds list.");
                return new List<FondLink>();
            }
        }

        private bool ShouldReloadData()
        {
            return fondsOverview == null || DateTime.Now - timeStamp > TimeSpan.FromHours(2);
        }
    }
}