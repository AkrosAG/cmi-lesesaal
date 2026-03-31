using CMI.Access.Harvest;
using CMI.Access.Harvest.CMIAIS;
using CMI.Access.Harvest.ScopeArchiv;
using CMI.Access.Sql.Lesesaal.EF;
using CMI.Contract.Common.Compiler;
using CMI.Contract.Harvest;
using CMI.Contract.Parameter;
using CMI.Manager.Harvest.Properties;
using Microsoft.CSharp;
using Microsoft.Extensions.DependencyInjection;
using System.Runtime.Caching;


namespace CMI.Manager.Harvest.Infrastructure
{

    /// <summary>
    ///     Helper class for configuring the IoC container.
    /// </summary>
    internal class ContainerConfigurator
    {
        public static IServiceCollection Configure()
        {
            var services = new ServiceCollection();


            services.AddTransient<LanguageSettings>();
            services.AddTransient<ApplicationSettings>();
            services.AddTransient<SipDateBuilder>();
            services.AddTransient<DigitizationOrderBuilder>();
            services.AddTransient<IHarvestManager, HarvestManager>();
            services.AddTransient<IParameterHelper, ParameterHelper>();
            services.AddTransient<IDbMutationQueueAccess, AISDataAccess>();
            services.AddTransient<IDbMetadataAccess, AISDataAccess>();
            services.AddTransient<IDbResyncAccess, AISDataAccess>();
            services.AddTransient<IDbStatusAccess, AISDataAccess>();
            services.AddTransient<IArchiveRecordProcessHandler, CMIAISArchiveRecordProcessHandler>();
            services.AddTransient<IAISDataProviderFactory, AISDataProviderFactory>();
            services.AddTransient<IArchiveRecordBuilderFactory, ArchiveRecordBuilderFactory>();

            var connectionString = DbConnectionSetting.Default.ConnectionStringEF;
            services.AddTransient<LesesaalDb>(sp => new LesesaalDb(connectionString));
            services.AddSingleton<ICachedHarvesterSetting, CachedHarvesterSetting>();
            services.AddSingleton<MemoryCache>(MemoryCache.Default);
            services.AddSingleton<CachedLookupData>();
            services.AddSingleton<CSharpCodeProvider>();
            services.AddSingleton<IDynamicScriptProvider, DynamicScriptProvider>();

            services.AddHttpClient("default");
            services.AddSingleton<IDynamicScriptLocator>(sp =>
            {
                var path = Settings.Default.CustomScriptPath;
                return new CustomScriptLocator(path);
            });

            services.AddTransient(sp =>
            {
                var dataProviderFactory = sp.GetRequiredService<IAISDataProviderFactory>();
                return dataProviderFactory.Create();

            });

            services.AddTransient<CMIAISDataProvider>(sp =>
            {
                var dataProviderFactory = sp.GetRequiredService<IAISDataProviderFactory>();
                return dataProviderFactory.Create() as CMIAISDataProvider;
            });
            
            services.AddTransient<IArchiveRecordBuilder>(sp =>
            {
                var builderFactory = sp.GetRequiredService<IArchiveRecordBuilderFactory>();
                return builderFactory.Create();

            });
            return services;

        }
    }
}