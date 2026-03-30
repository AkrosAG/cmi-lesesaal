using Autofac;
using Autofac.Core;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Quartz.Impl.AdoJobStore.Common;
using Quartz.Spi;
using System;

namespace CMI.Manager.DataFeed.Infrastructure
{
    /// <summary>
    ///     JobFactory resolves dependencies for the Quartz scheduler
    /// </summary>
    internal class JobFactory : IJobFactory
    {
        private readonly IServiceProvider provider;

        public JobFactory(IServiceProvider provider)
        {
            this.provider = provider;
        }

        public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
        {
            var job = provider.GetRequiredService(bundle.JobDetail.JobType);
            // Job aus DI auflösen
            return (IJob)job;
        }

        public void ReturnJob(IJob job)
        {
            // In Microsoft DI musst du normalerweise nichts zurückgeben.
            // Optional: Wenn du IDisposable Jobs hast:
            if (job is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }
}