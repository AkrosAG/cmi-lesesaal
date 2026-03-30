using CMI.Access.Harvest;
using CMI.Access.Harvest.CMIAIS;
using CMI.Access.Harvest.ScopeArchiv;
using CMI.Access.Sql.Lesesaal.EF;
using CMI.Contract.Common.Compiler;
using CMI.Contract.Harvest;
using CMI.Manager.DataFeed.Properties;
using Microsoft.CSharp;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Runtime.Caching;

namespace CMI.Manager.DataFeed.Infrastructure
{
    internal class ContainerConfigurator
    {
        public static IServiceCollection Configure()
        {
            var services = new ServiceCollection();

            services.AddScoped<LanguageSettings>();
            services.AddScoped<ApplicationSettings>();
            services.AddScoped<CachedLookupData>();
            services.AddScoped<SipDateBuilder>();
            services.AddScoped<DigitizationOrderBuilder>();
            services.AddSingleton<MemoryCache>(MemoryCache.Default);
            
            services.AddScoped<IDbMutationQueueAccess, AISDataAccess>();
            services.AddSingleton<CheckMutationQueueJob>();
            services.AddSingleton<RequeueMutationJob>();
            var connectionString = ConfigurationManager.ConnectionStrings[nameof(LesesaalDb)].ConnectionString;
            services.AddScoped<LesesaalDb>(sp => new LesesaalDb(connectionString));
            services.AddTransient<IAISDataProviderFactory, AISDataProviderFactory>();
            services.AddTransient<IArchiveRecordBuilderFactory, ArchiveRecordBuilderFactory>();
            services.AddSingleton<CSharpCodeProvider>(); services.AddScoped<IDynamicScriptProvider, DynamicScriptProvider>();
            services.AddSingleton<IDynamicScriptLocator>(sp =>
            {
                return new EmptyScriptLocator();
            });
            
            services.AddHttpClient("default");
            services.AddScoped<IArchiveRecordProcessHandler, CMIAISArchiveRecordProcessHandler>();
            services.AddScoped<IArchiveRecordBuilder, CMIAISArchiveRecordBuilder>();
            services.AddSingleton<CMIAISDataProvider>();
            services.AddSingleton<IAISDataProvider>(sp => sp.GetRequiredService<CMIAISDataProvider>());
            services.AddSingleton<IAISSpecificRecordAccess>(sp => sp.GetRequiredService<CMIAISDataProvider>());
            services.AddScoped<ICancelToken, JobCancelToken>();
           
            return services;
        }
    }
}