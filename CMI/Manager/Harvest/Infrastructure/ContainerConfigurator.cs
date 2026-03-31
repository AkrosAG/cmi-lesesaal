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
using System.Configuration;
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
            services.AddTransient<IHarvestManager, HarvestManager>();
            services.AddTransient<IArchiveRecordProcessHandler, CMIAISArchiveRecordProcessHandler>();
            services.AddTransient<IAISDataProviderFactory, AISDataProviderFactory>();
            services.AddTransient<IArchiveRecordBuilderFactory, ArchiveRecordBuilderFactory>();
            services.AddTransient<CSharpCodeProvider>();

            services.AddScoped<SipDateBuilder>();
            services.AddScoped<DigitizationOrderBuilder>();
            services.AddScoped<AISDataAccess>();
            services.AddScoped<IDbMutationQueueAccess>(sp => sp.GetRequiredService<AISDataAccess>());
            services.AddScoped<IDbMetadataAccess>(sp => sp.GetRequiredService<AISDataAccess>());
            services.AddScoped<IDbResyncAccess>(sp => sp.GetRequiredService<AISDataAccess>());
            services.AddScoped<IDbStatusAccess>(sp => sp.GetRequiredService<AISDataAccess>());
            services.AddScoped<IDbTestAccess>(sp => sp.GetRequiredService<AISDataAccess>());
            var connectionString = DbConnectionSetting.Default.ConnectionStringEF;
            services.AddScoped<LesesaalDb>(sp => new LesesaalDb(connectionString));


            services.AddSingleton<ICachedHarvesterSetting, CachedHarvesterSetting>();
            services.AddSingleton<IParameterHelper, ParameterHelper>();
            services.AddSingleton<MemoryCache>(MemoryCache.Default);
            services.AddSingleton<CachedLookupData>();
            services.AddSingleton<LanguageSettings>();
            services.AddSingleton<ApplicationSettings>();
            services.AddSingleton<IDynamicScriptProvider, DynamicScriptProvider>();

            services.AddHttpClient("default");
            services.AddSingleton<IDynamicScriptLocator>(sp =>
            {
                var path = Settings.Default.CustomScriptPath;
                return new CustomScriptLocator(path);
            });

            services.AddTransient<IAISDataProvider>(sp =>   
            {
                var dataProviderFactory = sp.GetRequiredService<IAISDataProviderFactory>();
                return dataProviderFactory.Create();
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