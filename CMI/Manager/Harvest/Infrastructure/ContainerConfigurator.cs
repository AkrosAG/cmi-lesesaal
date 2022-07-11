using System.Configuration;
using System.Reflection;
using Autofac;

using CMI.Access.Harvest;
using CMI.Access.Harvest.CMIAIS;
using CMI.Access.Harvest.ScopeArchiv;
using CMI.Access.Sql.Lesesaal.EF;
using CMI.Contract.Common.Compiler;
using CMI.Contract.Harvest;
using CMI.Contract.Parameter;

using MassTransit;

namespace CMI.Manager.Harvest.Infrastructure
{
    /// <summary>
    ///     Helper class for configuring the IoC container.
    /// </summary>
    internal class ContainerConfigurator
    {
        public static ContainerBuilder Configure()
        {
            var builder = new ContainerBuilder();

            // register the different consumers and classes
            builder.RegisterType<LanguageSettings>().AsSelf();
            builder.RegisterType<ApplicationSettings>().AsSelf();
            builder.RegisterType<CachedLookupData>().AsSelf();
            
            builder.RegisterType<SipDateBuilder>().AsSelf();
            builder.RegisterType<DigitizationOrderBuilder>().AsSelf();
            
            builder.RegisterType<CMIAISArchiveRecordProcessHandler>().As<IArchiveRecordProcessHandler>();
            builder.RegisterType<AISDataAccess>().As<IDbMutationQueueAccess>();
            builder.RegisterType<AISDataProviderFactory>().As<IAISDataProviderFactory>();
            builder.RegisterType<ArchiveRecordBuilderFactory>().As<IArchiveRecordBuilderFactory>();
            var connectionString = ConfigurationManager.ConnectionStrings[nameof(LesesaalDb)].ConnectionString;
            builder.RegisterType<LesesaalDb>().AsSelf().WithParameter(nameof(connectionString), connectionString);

            builder.RegisterType<DynamicScriptProvider>().As<IDynamicScriptProvider>();
            builder.Register(ctx =>
            {
               var path = Settings.Default.CustomScriptRoot;
               return new CustomScriptLocator(path);
            }).AsImplementedInterfaces().AsSelf();


            builder.Register(ctx =>
            {
                var dataProviderFactory = ctx.Resolve<IAISDataProviderFactory>();
                return dataProviderFactory.Create();
            }).AsImplementedInterfaces().AsSelf();

            builder.Register(ctx =>
            {
                var builderFactory = ctx.Resolve<IArchiveRecordBuilderFactory>();
                return builderFactory.Create();
            }).As<IArchiveRecordBuilder>().AsSelf();

            builder.RegisterType<HarvestManager>().As<IHarvestManager>();
            builder.RegisterType<AISDataAccess>().AsImplementedInterfaces();
            builder.RegisterType<ParameterHelper>().As<IParameterHelper>();
            builder.RegisterType<CachedHarvesterSetting>().As<ICachedHarvesterSetting>().SingleInstance();
            
            // register all the consumers
            builder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
                .AssignableTo<IConsumer>()
                .AsSelf();

            return builder;
        }
    }
}