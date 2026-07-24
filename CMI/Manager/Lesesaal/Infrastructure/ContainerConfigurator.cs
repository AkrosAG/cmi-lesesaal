using CMI.Access.Sql.Lesesaal.EF;
using CMI.Contract.Common;
using CMI.Contract.Parameter;
using CMI.Manager.Lesesaal.Properties;
using Microsoft.Extensions.DependencyInjection;

namespace CMI.Manager.Lesesaal.Infrastructure
{
    /// <summary>
    ///     Helper class for configuring the IoC container.
    /// </summary>
    internal class ContainerConfigurator
    {
        public static IServiceCollection Configure()
        {
            var services = new ServiceCollection();

            var connectionString = DbConnectionSetting.Default.ConnectionStringEF;
            services.AddScoped<LesesaalDb>(_ => new LesesaalDb(connectionString)); // ctor parameter

            services.AddScoped<IParameterHelper, ParameterHelper>();
            services.AddScoped<ICollectionAccess, CollectionAccess>();
            services.AddScoped<ICollectionManager, CollectionManager>();
            services.AddScoped<ISynchronisationManager, SynchronisationManager>();
            services.AddScoped<ISynchronisationAccess, SynchronisationAccess>();
            services.AddScoped<ILesesaalDataProvider, LesesaalDataProvider>();

            return services;
        }
    }
}