using CMI.Access.Sql.Lesesaal;
using CMI.Contract.Common;
using CMI.Web.Common.api;
using CMI.Web.Common.Helpers;
using CMI.Web.Frontend.api.Configuration;
using System;

namespace CMI.Web.Frontend.api.Controllers
{
    [CamelCaseJson]
    public abstract class ApiFrontendControllerBase : ApiControllerBase
    {
        private UserAccessProvider userAccessProvider;

        protected FrontendSettings Settings => FrontendSettings.Instance;

        /// <summary>
        ///     Falls die Methode true zurückgibt, muss der der Benutzer
        ///     A) wählen das keine Personendaten vorhanden sind oder
        ///     B) ein Grund auswählen
        /// </summary>
        internal static bool CouldNeedAReason(ElasticArchiveRecord record, UserAccess access)
        {
            return access.RolePublicClient == AccessRoles.RoleAS
                   && access.HasAsTokenFor(record.PrimaryDataDownloadAccessTokens)
                   && (string.IsNullOrEmpty(record.Permission) || 
                       record.Permission.Equals("Gesuchspflichtig", StringComparison.InvariantCultureIgnoreCase));
        }

        protected UserAccess GetUserAccess(string language = null, string userId = null)
        {
            userAccessProvider ??= new UserAccessProvider(ControllerHelper.UserDataAccess);

            userId ??= ControllerHelper.GetCurrentUserId();
            language ??= WebHelper.GetClientLanguage(Request);

            return userAccessProvider.GetUserAccess(language, userId);
        }
    }
}