using System.Web.Http.Cors;
using CMI.Web.Common.api;
using CMI.Web.Management.api.Configuration;

namespace CMI.Web.Management.api.Controllers
{
    [EnableCors("*", "*", "*")]
    [CamelCaseJson]
    public abstract class ApiManagementControllerBase : ApiControllerBase
    {
        protected ManagementSettings Settings => ManagementSettings.Instance;

        public IManagementControllerHelper ManagementControllerHelper { get; set; }

        protected ApiManagementControllerBase()
        {
            ManagementControllerHelper = new ManagementControllerHelper(this, ControllerHelper);
        }
    }
}