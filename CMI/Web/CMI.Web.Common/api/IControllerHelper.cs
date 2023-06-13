namespace CMI.Web.Common.api
{
    public interface IControllerHelper
    {
        string GetCurrentUserId();
        bool IsStaff();
        bool IsEthEmployee();
        bool NoHomeOrganization();

        /// <summary>
        ///     Kerberos-/Smartcard Anmeldung
        /// </summary>
        /// <returns></returns>
        bool IsInternalUser();

        string GetFromClaim(string field);
        string GetManagementRoleFromClaim();
        bool HasClaims();
    }
}