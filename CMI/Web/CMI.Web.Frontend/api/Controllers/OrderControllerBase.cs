using System;
using CMI.Access.Sql.Lesesaal;
using CMI.Contract.Common;

namespace CMI.Web.Frontend.api.Controllers
{
    public class OrderControllerBase : ApiFrontendControllerBase
    {
        protected bool IsEinsichtsbewilligungNotwendig(ElasticArchiveRecord record, UserAccess access, bool hasBewilligungsDatum)
        {
            return false;
            /* Todo (record.Benutzbarkeit() == null 
                           || record.Benutzbarkeit().Equals("Gesuchspflichtig", StringComparison.InvariantCultureIgnoreCase))
                   && !hasBewilligungsDatum; */
        }
    }
}