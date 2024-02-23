using System;
using Autofac;
using CMI.Contract.Parameter;
using CMI.Contract.Repository;
using MassTransit;
using System.Reflection;
using CMI.Access.Repository.Systems.Dir;
using CMI.Access.Repository.Systems.Rosetta;
using CMI.Contract.Monitoring;
using CMI.Engine.PackageMetadata.Systems.Dir;
using CMI.Engine.PackageMetadata.Systems.Rosetta;
using CMI.Manager.Repository.Systems;
using CMI.Manager.Repository.Systems.Bar;
using CMI.Manager.Repository.Systems.Mock;
using CMI.Manager.Repository.Systems.Rosetta;

namespace CMI.Manager.Repository.Infrastructure
{
    /// <summary>
    ///     Helper class for configuring the IoC Container.
    /// </summary>
    internal class ContainerConfigurator
    {
        public static ContainerBuilder Configure()
        {
            var builder = new ContainerBuilder();

            // register the different consumers and classes
            builder.RegisterType<RepositoryManager>().As<IRepositoryManager>();

            var providerName = Properties.Settings.Default.RepositoryManager.ToLowerInvariant();
            switch (providerName)
            {
                case "mock":
                    builder.RegisterType<MockRepositoryProvider>().As<IRepositoryProvider>();
                    builder.RegisterType<MockPackageHandler>().As<IPackageHandler>();
                    builder.RegisterType<DirPackageValidator>().As<IDirPackageValidator>();

                    break;

                case "rosetta":
                    builder.RegisterType<RosettaRepositoryProvider>().As<IRepositoryProvider>();
                    builder.RegisterType<RosettaPackageHandler>().As<IPackageHandler>();
                    builder.RegisterType<RosettaDataAccess>().As<IRosettaDataAccess>();
                    builder.RegisterType<RosettaConnector>();
                    builder.RegisterType<RosettaRepositoryCheck>().As<IRepositoryCheck>();
                    break;
                case "dir":
                    builder.RegisterType<DirRepositoryProvider>().As<IRepositoryProvider>();
                    builder.RegisterType<DirRepositoryConnectionFactory>().As<IDirRepositoryConnectionFactory>();
                    builder.RegisterType<DirPackageValidator>().As<IDirPackageValidator>();
                    builder.RegisterType<DirPackageHandler>().As<IPackageHandler>();
                    builder.RegisterType<DirRepositoryDataAccess>().As<IDirRepositoryDataAccess>();
                    builder.RegisterType<DirMetadataDataAccess>().As<IDirMetadataDataAccess>();
                    builder.RegisterType<DirRepositoryCheck>().As<IRepositoryCheck>();
                    break;
                default:
                    throw new NotImplementedException($"Der Provider mit dem Namen {providerName} ist nicht implementiert.");
            }

            builder.RegisterType<ParameterHelper>().As<IParameterHelper>();

            // register all the consumers
            builder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
                .AssignableTo<IConsumer>()
                .AsSelf();
            return builder;
        }
    }

}