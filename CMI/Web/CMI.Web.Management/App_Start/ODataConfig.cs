using CMI.Access.Sql.Lesesaal;
using CMI.Access.Sql.Lesesaal.EF;
using CMI.Web.Management.api.Controllers;
using Microsoft.AspNet.OData.Batch;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using System.Web.Http;

namespace CMI.Web.Management
{
    public class ODataConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.MapHttpAttributeRoutes();

            var modelBuilder = new ODataConventionModelBuilder();
            modelBuilder.EnableLowerCamelCase();

            var orderingName = nameof(OrderingFlatItemsController).Replace("Controller", "");
            var userOverviewName = nameof(UserOverviewController).Replace("Controller", "");
            var synchronisationen = nameof(VSynchronisationenController).Replace("Controller", "");

            modelBuilder.EntitySet<OrderingFlatItem>(orderingName).EntityType.Count().Select().Filter().Expand().Page().OrderBy();
            modelBuilder.EntitySet<UserOverview>(userOverviewName).EntityType.Count().Select().Filter().Expand().Page().OrderBy();
            modelBuilder.EntitySet<VSyncAction>(synchronisationen).EntityType.Count().Select().Filter().Expand().Page().OrderBy();

            config.MapODataServiceRoute(
                "ODataRoute",
                "odata",
                modelBuilder.GetEdmModel(),
                new DefaultODataBatchHandler(GlobalConfiguration.DefaultServer));

            config.EnsureInitialized();
        }
    }
}