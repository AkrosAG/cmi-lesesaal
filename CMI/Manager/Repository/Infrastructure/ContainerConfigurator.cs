using Autofac;
using CMI.Access.Repository;
using CMI.Contract.Parameter;
using CMI.Contract.Repository;
using CMI.Engine.PackageMetadata;
using CMI.Manager.Repository.Mock;
using MassTransit;
using System.Reflection;

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

            switch (Properties.Settings.Default.RepositoryManager.ToLowerInvariant())
            {
                case "mock":
                    builder.RegisterType<MockRepositoryManager>().As<IRepositoryManager>();
                    builder.RegisterType<MockRepositoryConnectionFactory>().As<IRepositoryConnectionFactory>();
                    builder.RegisterType<MockRepositoryDataAccess>().As<IRepositoryDataAccess>();
                    break;
              
                default:
                    builder.RegisterType<RepositoryManager>().As<IRepositoryManager>();
                    builder.RegisterType<RepositoryConnectionFactory>().As<IRepositoryConnectionFactory>();
                    builder.RegisterType<RepositoryDataAccess>().As<IRepositoryDataAccess>();
                    break;
            }
           
         
            builder.RegisterType<PackageValidator>().As<IPackageValidator>();
            builder.RegisterType<MetadataDataAccess>().As<IMetadataDataAccess>();
            builder.RegisterType<PackageHandler>().As<IPackageHandler>();
            builder.RegisterType<ParameterHelper>().As<IParameterHelper>();

            // register all the consumers
            builder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
                .AssignableTo<IConsumer>()
                .AsSelf();
            return builder;
        }
    }

}