using CMI.Access.Harvest;
using CMI.Access.Harvest.CMIAIS;
using CMI.Access.Harvest.ScopeArchiv;
using CMI.Contract.Common.Compiler;
using CMI.Contract.Harvest;
using Microsoft.CSharp;
using Microsoft.Extensions.DependencyInjection;

namespace CMI.Manager.ExternalContent.Infrastructure
{
    /// <summary>
    ///     Helper class for configuring the IoC container.
    /// </summary>
    internal class ContainerConfigurator
    {
        public static IServiceCollection Configure()
        {
            var services = new ServiceCollection();

            // register the different consumers and classes
           
            services.AddScoped<LanguageSettings>();
            services.AddScoped<ApplicationSettings>();
            services.AddSingleton<CachedLookupData>();
            services.AddScoped<DigitizationOrderBuilder>();
            services.AddScoped<SipDateBuilder>();
            // -------------------------
            // Dynamic Script Services
            // -------------------------
            services.AddSingleton<IDynamicScriptProvider, DynamicScriptProvider>(); 
            services.AddSingleton<IDynamicScriptLocator, EmptyScriptLocator>();
            services.AddSingleton<CSharpCodeProvider>();

            services.AddHttpClient("default");
            services.AddScoped<IExternalContentManager, ExternalContentManager>();
            services.AddScoped<IArchiveRecordProcessHandler, CMIAISArchiveRecordProcessHandler>();
            services.AddScoped<IDbMutationQueueAccess, AISDataAccess>();
            services.AddScoped<IAISDataProviderFactory, AISDataProviderFactory>();
            services.AddScoped<IArchiveRecordBuilderFactory, ArchiveRecordBuilderFactory>();
            services.AddScoped<IDbExternalContentAccess, AISDataAccess>();
            // Für den CMIAISDataProvider wird unter anderem LesesaalDb benötigt
            services.AddScoped<IAISDataProvider, ScopeAISDataProvider>();

            

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