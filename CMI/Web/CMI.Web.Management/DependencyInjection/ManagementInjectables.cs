using Autofac;
using CMI.Access.Sql.Lesesaal;
using CMI.Access.Sql.Lesesaal.AblieferndeStellen;
using CMI.Access.Sql.Lesesaal.EF;
using CMI.Access.Sql.Lesesaal.File;
using CMI.Contract.Common;
using CMI.Contract.Messaging;
using CMI.Contract.Order;
using CMI.Contract.Parameter;
using CMI.Utilities.Cache.Access;
using CMI.Utilities.ProxyClients.Order;
using CMI.Utilities.Template;
using CMI.Web.Common.Auth;
using CMI.Web.Common.Helpers;
using CMI.Web.Management.api.Configuration;
using CMI.Web.Management.api.Data;
using MassTransit;
using System.Reflection;


namespace CMI.Web.Management.DependencyInjection
{
    public static class ManagementInjectables
    {
        public static void RegisterManagementInjectables(this ContainerBuilder builder)
        {
            builder.RegisterType<OrderManagerClient>().AsSelf();
            builder.RegisterType<CollectionManagerClient>().As<ICollectionManager>();
            builder.RegisterType<ExcelExportHelper>().AsSelf();
            builder.RegisterType<CacheHelper>().As<ICacheHelper>().WithParameter("sftpLicenseKey", WebHelper.Settings["sftpLicenseKey"]);
            
            var connectionString = ManagementSettings.Instance.SqlConnectionString;
            var connectionStringEF = ManagementSettings.Instance.SqlConnectionStringEF;

            builder.RegisterType<LesesaalDb>().AsSelf().WithParameter(nameof(connectionString), connectionStringEF);
            builder.RegisterType<UserDataAccess>().As<IUserDataAccess>().InstancePerRequest().WithParameter(nameof(connectionString), connectionString);
            builder.RegisterType<ApplicationRoleDataAccess>().As<IApplicationRoleDataAccess>().InstancePerRequest().WithParameter(nameof(connectionString), connectionString);
            builder.RegisterType<ApplicationRoleUserDataAccess>().As<IApplicationRoleUserDataAccess>().InstancePerRequest().WithParameter(nameof(connectionString), connectionString);
            builder.RegisterType<AblieferndeStelleDataAccess>().As<IAblieferndeStelleDataAccess>().InstancePerRequest().WithParameter(nameof(connectionString), connectionString);
            builder.RegisterType<AblieferndeStelleTokenDataAccess>().As<IAblieferndeStelleTokenDataAccess>().InstancePerRequest().WithParameter(nameof(connectionString), connectionString);
            builder.RegisterType<DownloadTokenDataAccess>().As<IDownloadTokenDataAccess>().InstancePerRequest().WithParameter(nameof(connectionString), connectionString);
            builder.RegisterType<NewsDataAccess>().AsSelf().InstancePerRequest().WithParameter(nameof(connectionString), connectionString);

            builder.RegisterType<SynchronisationManagerClient>().As<ISynchronisationManager>();
            builder.RegisterType<SynchronisationAccess>().As<ISynchronisationAccess>();

            builder.Register(c => BusConfig.CreateGetElasticLogRecordsRequestClient()).As<IRequestClient<GetElasticLogRecordsRequest>>();
            builder.Register(c => BusConfig.RegisterDownloadAssetCallback()).As<IRequestClient<DownloadAssetRequest>>();
            builder.Register(c => BusConfig.CreateDoesExistInCacheClient()).As<IRequestClient<DoesExistInCacheRequest>>();

            builder.RegisterType<AuthenticationHelper>().As<IAuthenticationHelper>();
            builder.RegisterType<ParameterHelper>().As<IParameterHelper>();
            builder.RegisterType<MailHelper>().As<IMailHelper>();

            builder.RegisterType<CmiSettings>().As<ICmiSettings>();
            builder.RegisterType<WebCmiConfigProvider>().As<IWebCmiConfigProvider>();

            builder.RegisterType<OrderManagerClient>().As<IPublicOrder>();
            builder.RegisterType<VecteurActionsClient>().As<IVecteurActions>();
            builder.RegisterType<DownloadLogHelper>().As<IDownloadLogHelper>();
            builder.RegisterType<AbbyyProgressInfo>().SingleInstance().AsSelf();

            // register all the consumers
            builder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
                .AssignableTo<IConsumer>()
                .AsSelf();
        }
    }
}