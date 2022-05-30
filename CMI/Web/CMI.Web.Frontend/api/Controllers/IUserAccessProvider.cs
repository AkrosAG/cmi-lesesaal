using CMI.Access.Sql.Lesesaal;

namespace CMI.Web.Frontend.api.Controllers
{
    public interface IUserAccessProvider
    {
        UserAccess GetUserAccess(string language, string userId);
    }
}