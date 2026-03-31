using CMI.Access.Harvest;
using CMI.Access.Harvest.CMIAIS;
using CMI.Access.Harvest.ScopeArchiv;
using CMI.Access.Sql.Lesesaal.EF;
using CMI.Contract.Common.Compiler;
using CMI.Contract.Harvest;
using CMI.Contract.Messaging;
using CMI.Contract.Monitoring;
using CMI.Contract.Parameter;
using CMI.Manager.Harvest.Consumers;
using CMI.Manager.Harvest.Properties;
using MassTransit;
using Microsoft.CSharp;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Reflection;
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

            services.AddScoped<LanguageSettings>();
            services.AddScoped<ApplicationSettings>();
            services.AddScoped<SipDateBuilder>();
            services.AddScoped<DigitizationOrderBuilder>();
            services.AddSingleton<MemoryCache>(MemoryCache.Default);
            services.AddSingleton<CachedLookupData>();
            services.AddSingleton<CSharpCodeProvider>();
            services.AddSingleton<ICachedHarvesterSetting, CachedHarvesterSetting>();

            services.AddHttpClient("default");
            services.AddScoped<IArchiveRecordProcessHandler, CMIAISArchiveRecordProcessHandler>();
            services.AddScoped<IDbMutationQueueAccess, CMIAISDataAccess>();
            services.AddScoped<IAISDataProviderFactory, AISDataProviderFactory>();
            services.AddScoped<IArchiveRecordBuilderFactory, ArchiveRecordBuilderFactory>();


            services.AddScoped<IHarvestManager, HarvestManager>();
            services.AddScoped<IParameterHelper, ParameterHelper>();
            services.AddScoped<IDbMetadataAccess, AISDataAccess>();
            services.AddScoped<IDbResyncAccess, AISDataAccess>();
            services.AddScoped<IDbExternalContentAccess, AISDataAccess>();
            services.AddScoped<IDbTestAccess, AISDataAccess>();

            var connectionString = DbConnectionSetting.Default.ConnectionStringEF;
            services.AddScoped<LesesaalDb>(sp => new LesesaalDb(connectionString));
            services.AddScoped<IDynamicScriptProvider, DynamicScriptProvider>();
            services.AddSingleton<IDynamicScriptLocator>(sp =>
            {
                var path = Settings.Default.CustomScriptPath;
                return new CustomScriptLocator(path);
            });

            services.AddSingleton<CMIAISDataProvider>();
            services.AddSingleton<IAISDataProvider>(sp => sp.GetRequiredService<CMIAISDataProvider>());
            services.AddSingleton<IAISSpecificRecordAccess>(sp => sp.GetRequiredService<CMIAISDataProvider>());

            services.AddTransient<IArchiveRecordBuilder>(sp =>
            {
                var builderFactory = sp.GetRequiredService<IArchiveRecordBuilderFactory>();
                return builderFactory.Create();
            });
            services.AddScoped<DigitizationOrderBuilder>();

            return services;
        }
    }
}