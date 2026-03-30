using CMI.Access.Harvest;
using CMI.Access.Harvest.CMIAIS;
using CMI.Access.Harvest.ScopeArchiv;
using CMI.Contract.Harvest;
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

            services.AddHttpClient("default");
            services.AddScoped<IExternalContentManager, ExternalContentManager>();
            services.AddScoped<IArchiveRecordProcessHandler, CMIAISArchiveRecordProcessHandler>();
            services.AddScoped<IDbMutationQueueAccess, AISDataAccess>();
            services.AddScoped<IAISDataProviderFactory, AISDataProviderFactory>();
            services.AddScoped<IArchiveRecordBuilderFactory, ArchiveRecordBuilderFactory>();


            return services;
        }
    }
}