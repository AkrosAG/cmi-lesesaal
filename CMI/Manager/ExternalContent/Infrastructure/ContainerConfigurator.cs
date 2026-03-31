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

            // Settings (unveränderliche Konfiguration) als Singleton
            services.AddSingleton<LanguageSettings>();
            services.AddSingleton<ApplicationSettings>();
            services.AddSingleton<CachedLookupData>();

            // Builder als Scoped (werden pro Request verwendet)
            services.AddScoped<DigitizationOrderBuilder>();
            services.AddScoped<SipDateBuilder>();

            // Dynamic Script Services
            services.AddSingleton<IDynamicScriptProvider, DynamicScriptProvider>();
            services.AddSingleton<IDynamicScriptLocator, EmptyScriptLocator>();
            services.AddTransient<CSharpCodeProvider>(); // WICHTIG: Transient statt Singleton!

            services.AddHttpClient("default");

            // Manager und Handler als Scoped
            services.AddScoped<IExternalContentManager, ExternalContentManager>();
            services.AddScoped<IArchiveRecordProcessHandler, CMIAISArchiveRecordProcessHandler>();

            // AISDataAccess: Entweder separate Instanzen...
            services.AddScoped<IDbMutationQueueAccess, AISDataAccess>();
            services.AddScoped<IDbExternalContentAccess, AISDataAccess>();

            // Factories als Singleton (zustandslos)
            services.AddSingleton<IAISDataProviderFactory, AISDataProviderFactory>();
            services.AddSingleton<IArchiveRecordBuilderFactory, ArchiveRecordBuilderFactory>();

            // Provider und Builder aus Factories als Transient
            services.AddTransient<IAISDataProvider>(sp =>
            {
                var factory = sp.GetRequiredService<IAISDataProviderFactory>();
                return factory.Create();
            });

            services.AddTransient<IArchiveRecordBuilder>(sp =>
            {
                var factory = sp.GetRequiredService<IArchiveRecordBuilderFactory>();
                return factory.Create();
            });

            return services;
        }
    }
}