namespace CMI.Web.Common.api
{
    public interface IControllerHelper
    {
        string GetCurrentUserId();
        bool IsStaff();
        bool IsHomeOrganizationEth();
        bool NoHomeOrganization();
        bool IsMTanAuthentication();

        /// <summary>
        ///     Kerberos-/Smartcard Anmeldung
        /// </summary>
        /// <returns></returns>
        bool IsInternalUser();

        string GetInitialTokenFromClaims();

        string GetFromClaim(string field);
        string GetManagementRoleFromClaim();
        bool HasClaims();
    }
}