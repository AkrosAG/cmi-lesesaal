using System;
using System.Linq;
using System.Security.Claims;
using System.Web.Http;
using CMI.Access.Sql.Lesesaal;
using CMI.Contract.Common;
using CMI.Web.Common.Helpers;

namespace CMI.Web.Common.api
{
    public class ControllerHelper : IControllerHelper
    {
        private readonly ApiController apiController;

        private IApplicationRoleUserDataAccess applicationRoleUserDataAccess;
        private IUserDataAccess userDataAccess;
        private string additionalInternalUsers;

        public ControllerHelper(ApiController apiController)
        {
            this.apiController = apiController;
            additionalInternalUsers = WebHelper.AdditionalInternalUsers;
        }

        public IUserDataAccess UserDataAccess
        {
            get
            {
                if (userDataAccess == null)
                {
                    userDataAccess = new UserDataAccess(WebHelper.Settings["sqlConnectionString"]);
                }

                return userDataAccess;
            }
        }

        public IApplicationRoleUserDataAccess ApplicationRoleUserDataAccess => applicationRoleUserDataAccess ??
                                                                               (applicationRoleUserDataAccess =
                                                                                   new ApplicationRoleUserDataAccess(
                                                                                       WebHelper.Settings["sqlConnectionString"]));

        public string GetCurrentUserId()
        {
            var identity = apiController.User?.Identity as ClaimsIdentity;
            var uidClaim = identity != null && identity.Claims != null
                ? identity.Claims.FirstOrDefault(c => c.Type.Contains("/identity/claims/e-id/userExtId"))
                : null;
            return uidClaim?.Value;
        }

        public bool IsStaff()
        {
            var isStaff = GetFromClaim("affiliation")?.ToLowerInvariant().Contains("staff".ToLowerInvariant());
            var internalEmail = GetFromClaim("emailaddress")?.ToLowerInvariant();
            if(!string.IsNullOrEmpty(additionalInternalUsers) && !string.IsNullOrEmpty(internalEmail))
            {
                return additionalInternalUsers.Split(',', ';', '|').Contains(internalEmail);
            }

            return isStaff.GetValueOrDefault(false);
        }

        public bool IsHomeOrganizationEth()
        {
            var isEthEmployee = GetFromClaim("homeOrganization")?.ToLowerInvariant().Contains("ethz.ch".ToLowerInvariant());
            return isEthEmployee.GetValueOrDefault(false);
        }

        public bool NoHomeOrganization()
        {
            var homeOrganization = GetFromClaim("homeOrganization")?.ToLowerInvariant();
            return string.IsNullOrEmpty(homeOrganization);
        }

        public bool IsMTanAuthentication()
        {
            var isMTan = GetFromClaim("/identity/claims/authenticationmethod")?.ToLowerInvariant().Contains("nomadtelephony".ToLowerInvariant());
            return isMTan.GetValueOrDefault(false);
        }

        public bool IsInternalUser()
        {
            // Es handelt sich um einen internen User wenn er Staff von der ETH ZH ist.
            return IsStaff() && IsHomeOrganizationEth();
        }

        public string GetInitialTokenFromClaims()
        {
            if (IsInternalUser())
            {
                return AccessRoles.RoleBVW;
            }

            // Hat jemand eine ganz normale edu-id, ist dieser Ö2 Benutzer
            var homeOrganization = GetFromClaim("homeOrganization")?.ToLowerInvariant();
            if (homeOrganization == "eduid.ch")
            {
                return AccessRoles.RoleOe2;
            }

            // Alle anderen erhalten die Ö3 Rolle
            return AccessRoles.RoleOe3;

        }

        public string GetFromClaim(string field)
        {
            var identity = apiController.User?.Identity as ClaimsIdentity;
            var claims = identity != null && identity.Claims != null ? identity.Claims.Where(c => c.Type.Contains(field)) : null;
            // If we have names that are very similar, we need to make a distinction. For example homeOrganization and homeOrganizationType
            var uidClaim = claims != null && claims.Count() == 1
                ? claims.FirstOrDefault()
                : claims.FirstOrDefault(c => c.Type.EndsWith(field));
            return uidClaim?.Value;
        }

        public string GetManagementRoleFromClaim(string rolePublicClient = null)
        {
            // Nur wer im M-C als BAAR Benutzer hochgestuft wurde, erhält dieses Prädikat
            if (IsInternalUser() && (rolePublicClient ?? "").Equals(AccessRoles.RoleBAR))
            {
                return "APPO";
            }
            else
            {
                return string.Empty;
            }
        }

        public bool HasClaims()
        {
            var identity = apiController.User?.Identity as ClaimsIdentity;
            return identity?.Claims != null && identity.Claims.Any();
        }
    }
}