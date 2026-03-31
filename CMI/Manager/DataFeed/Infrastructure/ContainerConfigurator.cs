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
using System;
using System.Configuration;
using System.Runtime.Caching;

namespace CMI.Manager.DataFeed.Infrastructure
{
    internal class ContainerConfigurator
    {
        public static IServiceCollection Configure()
        {
            var services = new ServiceCollection();

            // -------------------------
            // Application Settings
            // -------------------------
            services.AddScoped<LanguageSettings>();
            services.AddScoped<ApplicationSettings>();
            services.AddScoped<CachedLookupData>();
            services.AddScoped<SipDateBuilder>();
            services.AddScoped<DigitizationOrderBuilder>();
            services.AddSingleton<MemoryCache>(MemoryCache.Default);

            // -------------------------
            // Database / Data Access
            // -------------------------
            services.AddScoped<IDbMutationQueueAccess, AISDataAccess>();

            var connectionString = ConfigurationManager.ConnectionStrings[nameof(LesesaalDb)].ConnectionString;
            services.AddScoped<LesesaalDb>(sp => new LesesaalDb(connectionString));

            // -------------------------
            // Factories & Builders
            // -------------------------
            services.AddTransient<IAISDataProviderFactory, AISDataProviderFactory>();
            services.AddTransient<IArchiveRecordBuilderFactory, ArchiveRecordBuilderFactory>();

            services.AddTransient<IArchiveRecordBuilder>(sp =>
            {
                var factory = sp.GetRequiredService<IArchiveRecordBuilderFactory>();
                return factory.Create();
            });

            // -------------------------
            // Dynamic Script Services
            // -------------------------
            services.AddSingleton<IDynamicScriptProvider, DynamicScriptProvider>(); // Singleton für Quartz Jobs
            services.AddSingleton<IDynamicScriptLocator, EmptyScriptLocator>();
            services.AddSingleton<CSharpCodeProvider>();

            // -------------------------
            // AIS Data Provider
            // -------------------------
            services.AddSingleton<CMIAISDataProvider>();
            services.AddSingleton<IAISDataProvider>(sp => sp.GetRequiredService<CMIAISDataProvider>());
            services.AddSingleton<IAISSpecificRecordAccess>(sp => sp.GetRequiredService<CMIAISDataProvider>());

            // -------------------------
            // Quartz Jobs / Handlers
            // -------------------------
            services.AddSingleton<IArchiveRecordProcessHandler, CMIAISArchiveRecordProcessHandler>();
            services.AddSingleton<ICancelToken, JobCancelToken>();

            services.AddSingleton<CheckMutationQueueJob>();
            services.AddSingleton<RequeueMutationJob>();

            // -------------------------
            // HttpClient
            // -------------------------
            services.AddHttpClient("default");

            return services;
        }
    }
}