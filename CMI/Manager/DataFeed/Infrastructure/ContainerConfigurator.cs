using CMI.Access.Sql.Lesesaal.EF;
using CMI.Contract.Common.Compiler;
using CMI.Contract.Harvest;
using Microsoft.CSharp;
using Microsoft.Extensions.DependencyInjection;
using System.Configuration;
using System.Runtime.Caching;
using CMI.Access.Harvest;
using CMI.Access.Harvest.CMIAIS;
using CMI.Access.Harvest.ScopeArchiv;

namespace CMI.Manager.DataFeed.Infrastructure
{
    internal class ContainerConfigurator
    {
        public static IServiceCollection Configure()
        {
            var services = new ServiceCollection();

            // -------------------------
            // Application Settings (Singleton - statische Konfiguration)
            // -------------------------
            services.AddSingleton<LanguageSettings>();
            services.AddSingleton<ApplicationSettings>();
            services.AddSingleton<CachedLookupData>(); // WICHTIG: Singleton!
            services.AddSingleton<MemoryCache>(MemoryCache.Default);

            // -------------------------
            // Builders (Scoped - pro Request/Job)
            // -------------------------
            services.AddScoped<SipDateBuilder>();
            services.AddScoped<DigitizationOrderBuilder>();

            // -------------------------
            // Database / Data Access (Scoped - DbContext!)
            // -------------------------
            var connectionString = ConfigurationManager.ConnectionStrings[nameof(LesesaalDb)].ConnectionString;
            services.AddScoped<LesesaalDb>(sp => new LesesaalDb(connectionString)); 
            services.AddScoped<IDbMutationQueueAccess, AISDataAccess>();

            // -------------------------
            // Factories (Singleton - zustandslos)
            // -------------------------
            services.AddSingleton<IAISDataProviderFactory, AISDataProviderFactory>();
            services.AddSingleton<IArchiveRecordBuilderFactory, ArchiveRecordBuilderFactory>();

            // -------------------------
            // Provider & Builder aus Factories (Scoped wegen DbContext!)
            // -------------------------
            services.AddScoped<IAISDataProvider>(sp =>
            {
                var factory = sp.GetRequiredService<IAISDataProviderFactory>();
                return factory.Create();
            });

            services.AddScoped<IArchiveRecordBuilder>(sp =>
            {
                var factory = sp.GetRequiredService<IArchiveRecordBuilderFactory>();
                return factory.Create();
            });

            // WICHTIG: CMIAISDataProvider NICHT als Singleton!
            // Wird über Factory erstellt und ist Scoped wegen LesesaalDb

            // -------------------------
            // Dynamic Script Services
            // -------------------------
            services.AddSingleton<IDynamicScriptProvider, DynamicScriptProvider>();
            services.AddSingleton<IDynamicScriptLocator, EmptyScriptLocator>();
            services.AddTransient<CSharpCodeProvider>(); // WICHTIG: Transient!

            // -------------------------
            // Process Handler (Singleton - zustandslos)
            // -------------------------
            services.AddSingleton<IArchiveRecordProcessHandler, CMIAISArchiveRecordProcessHandler>();

            // -------------------------
            // Cancel Token (Singleton - shared state)
            // -------------------------
            services.AddSingleton<ICancelToken, JobCancelToken>();

            // -------------------------
            // Quartz Jobs (Scoped - wegen IDbMutationQueueAccess)
            // -------------------------
            services.AddScoped<CheckMutationQueueJob>();
            services.AddScoped<RequeueMutationJob>();

            // -------------------------
            // HttpClient
            // -------------------------
            services.AddHttpClient("default");

            return services;
        }
    }
}