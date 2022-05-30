using System.Collections.Generic;
using System.Threading.Tasks;
using CMI.Access.Sql.Lesesaal;

namespace CMI.Web.Frontend.Helpers
{
    public interface IKontrollstellenInformer
    {
        Task InformIfNecessary(UserAccess userAccess, IList<VeInfo> veInfoList);
    }
}