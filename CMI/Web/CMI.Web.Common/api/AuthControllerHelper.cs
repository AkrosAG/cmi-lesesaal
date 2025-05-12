using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Authentication;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Web;
using CMI.Access.Sql.Lesesaal;
using CMI.Contract.Common;
using CMI.Web.Common.Auth;
using CMI.Web.Common.Helpers;
using Microsoft.AspNet.Identity;
using Microsoft.Owin;
using Newtonsoft.Json.Linq;
using Serilog;
using SameSiteMode = Microsoft.Owin.SameSiteMode;

namespace CMI.Web.Common.api
{
    public class AuthControllerHelper
    {
        private const string roleIdentifier = "Standard";
        private const string loginSystem = "System-Login";
        private readonly IApplicationRoleUserDataAccess applicationRoleUserDataAccess;
        private readonly IAuthenticationHelper authenticationHelper;
        private readonly IControllerHelper controllerHelper;
        private readonly IUserDataAccess userDataAccess;
        private readonly IWebCmiConfigProvider webCmiConfigProvider;

        public AuthControllerHelper(IApplicationRoleUserDataAccess applicationRoleUserDataAccess,
            IUserDataAccess userDataAccess,
            IControllerHelper controllerHelper,
            IAuthenticationHelper authenticationHelper,
            IWebCmiConfigProvider webCmiConfigProvider)
        {
            this.applicationRoleUserDataAccess = applicationRoleUserDataAccess;
            this.userDataAccess = userDataAccess;
            this.controllerHelper = controllerHelper;
            this.webCmiConfigProvider = webCmiConfigProvider;
            this.authenticationHelper = authenticationHelper;
        }

        /// <summary>
        /// Wird aufgerufen, wenn das EIAM / SAML2-Login durchgeführt wurde.
        /// Erstellt die Session innerhalb der Lesesaal Applikation
        /// </summary>
        /// <param name="owinContext"></param>
        /// <returns></returns>
        public async Task OnExternalSignIn(IOwinContext owinContext)
        {
            var authManager = owinContext.Authentication;
            var authResult = await authManager.AuthenticateAsync(DefaultAuthenticationTypes.ExternalCookie);
            if (authResult == null)
            {
                return;
            }

            var identity = authResult.Identity;
            Log.Information("Found identity for user {Name}", identity.Name);

            authManager.SignOut(DefaultAuthenticationTypes.ExternalCookie);

            Log.Information("Getting claims");
            var claims = identity.Claims.ToList();
            var appCookieKey = WebHelper.CookiePcAppliationCookieKey;
            var ci = new ClaimsIdentity(claims, appCookieKey);
            authManager.SignIn(ci);

            var aspSessionIdCookieyKey = WebHelper.CookiePcAspNetSessionIdKey;
            var sessionId = owinContext.Request.Cookies[aspSessionIdCookieyKey];
            var userId = GetUserId(ci.Claims);
            Log.Information("Got userId from claims: {userId}", userId);

            // Die SessionId wird zusätzlich in einem eigenem Cookie gespeichert, damit diese nach dem Logout kurzzeitig verwendet werden kann.
            var cookieUserIdKey = WebHelper.CookiePcUserIdKey;
            AddLesesaalSessionCookie(owinContext, userId, cookieUserIdKey);

            // Wir merken uns die aktive SessionId um sie bei einem Logout zurückzusetzen. 
            // Dies wird beim Überprüfen der Identity genutzt um zu verhindern, dass vor einem Logout
            // das Session-Cookie abgegriffen - und nach dem Logout weiter verwendet werden kann.
            userDataAccess.UpdateActiveSessionId(userId, sessionId);
        }

        /// <summary>
        /// Wird nach dem Abmelden vom EIAM / SAMl2 aufgerufen.
        /// Die Methode entfernt die notwendigen Cookies und setzt die aktive SessionId des Benutzers auf der DB zurück.
        /// </summary>
        /// <param name="owinContext"></param>
        public void OnExternalSignOut(IOwinContext owinContext)
        {
            var cookieUserIdKey = WebHelper.CookiePcUserIdKey;
            var appCookieKey = WebHelper.CookiePcAppliationCookieKey;

            var userId = owinContext.Request.Cookies[cookieUserIdKey];
            userDataAccess.UpdateActiveSessionId(userId, null);

            var authManager = owinContext.Authentication;
            authManager.SignOut(appCookieKey);
            owinContext.Response.Cookies.Delete(cookieUserIdKey, new CookieOptions
            {
                HttpOnly = true, Secure = true, SameSite = SameSiteMode.Strict
            });
        }

        private static void AddLesesaalSessionCookie(IOwinContext owinContext, string userId, string cookieUserIdKey)
        {
            owinContext.Response.Cookies.Append(cookieUserIdKey, userId,
                new CookieOptions { HttpOnly = true, Secure = true, SameSite = SameSiteMode.Strict });
        }

        private string GetUserId(IEnumerable<Claim> claims)
        {
            return claims?.FirstOrDefault(c => c.Type.Contains("/identity/claims/e-id/userExtId"))?.Value;
        }

        public Identity GetIdentity(HttpRequestMessage request, IPrincipal user, bool isPublicClient)
        {
            var userId = controllerHelper.GetCurrentUserId();
            var claims = authenticationHelper.GetClaimsForRequest(user, request);

            if (!HasValidMandant(claims))
            {
                Log.Warning("User hat noch keinen Antrag gestellt");
                throw new AuthenticationException("User hat noch keinen Antrag gestellt");
            }

            var isNewUser = !TryUpdateUser(userId, claims);


            if (isNewUser)
            {
                return new Identity
                {
                    IssuedClaims = claims.ToArray(),
                    Roles = new[]
                    {
                        isPublicClient
                            ? controllerHelper.GetInitialTokenFromClaims()
                            : controllerHelper.GetManagementRoleFromClaim()
                    },
                    IssuedAccessTokens = new string[] { },
                    AuthStatus = AuthStatus.NeuerBenutzer,
                    RedirectUrl = GetReturnUrl(AuthStatus.NeuerBenutzer, isPublicClient)
                };
            }

            var role = isPublicClient
                ? userDataAccess.GetRoleForClient(userId)
                : userDataAccess.GetEiamRoles(userId);

            var authStatus = IsValidAuthRole(role, isPublicClient);

            var accessTokens = userDataAccess.GetTokensDesUser(userId);
            var identity = new Identity
            {
                IssuedClaims = claims.ToArray(),
                Roles = new [] { role },
                IssuedAccessTokens = accessTokens,
                AuthStatus = authStatus,
                RedirectUrl = GetReturnUrl(authStatus, isPublicClient)
            };
            AddAppRolesAndFeatures(userId, identity);

            // Fehlerhafte Rolle oder Anmeldung
            if (authStatus == AuthStatus.KeineRolleDefiniert)
            {
                Log.Error("Es wurde für den Benutzer keine Rolle definiert in der Datenbank oder Authentifikation hat fehlgeschlagen UserId:={UserId}, AuthStatus={AuthStatus}",
                    userId, authStatus);
            }

            return identity;
        }

        internal bool TryUpdateUser(string userId, IList<ClaimInfo> claims)
        {
            var user = userDataAccess.GetUser(userId);
            if (user == null)
            {
                return false;
            }

            var isInternal = controllerHelper.IsInternalUser();
            var mgntRole = controllerHelper.GetManagementRoleFromClaim(user.RolePublicClient);
            try
            {
                var userDataOnLogin = new User
                {
                    Id = userId,
                    IsInternalUser = isInternal,
                    EiamRoles = mgntRole,
                    UserExtId = controllerHelper.GetFromClaim("/identity/claims/e-id/userExtId"),
                    Claims = new JObject { { "claims", JArray.FromObject(claims) } },
                    FamilyName = isInternal ? controllerHelper.GetFromClaim(ClaimTypes.Surname) : user.FamilyName,
                    FirstName = isInternal ? controllerHelper.GetFromClaim(ClaimTypes.GivenName) : user.FirstName,
                    EmailAddress = isInternal ? controllerHelper.GetFromClaim(ClaimTypes.Email) : user.EmailAddress
                };

                // Prüfen User Änderung enthält, falls ja Daten aktualisieren 
                if (HasUserChanges(userDataOnLogin, user))
                {
                    userDataAccess.UpdateUserOnLogin(userDataOnLogin, userId, loginSystem);
                }

                // Prüfen ob ein automatische hoch- oder runterstufen stattfinden soll
                // Es gibt nur eine Runterstufung zu Oe3 falls ein früherer EMA/AS/AMA Benutzer nicht mehr 
                // die Voraussetzungen erfüllt und als initiale Rolle Ö3 oder Ö2 erhält
                var initialRole = controllerHelper.GetInitialTokenFromClaims();
                if (initialRole != user.RolePublicClient)
                {
                    // runterstufen auf Ö3, von EMA, AMA und AS
                    if ((initialRole == AccessRoles.RoleOe3 || initialRole == AccessRoles.RoleOe2) && (user.RolePublicClient.Equals(AccessRoles.RoleAMA) ||
                                 user.RolePublicClient.Equals(AccessRoles.RoleAS) || user.RolePublicClient.Equals(AccessRoles.RoleEMA)))
                    {
                        userDataAccess.UpdatePublicClientRole(user.UserExtId, AccessRoles.RoleOe3, loginSystem);
                    }
                }

                // Falls der Benutzer für M-C berechtigt ist, soll die Standardrolle zugewiesen werden
                if (!string.IsNullOrWhiteSpace(mgntRole) && mgntRole.Equals(AccessRoles.RoleMgntAllow))
                {
                    applicationRoleUserDataAccess.InsertRoleUser(roleIdentifier, userId);
                }
                else if (string.IsNullOrWhiteSpace(mgntRole))
                {
                    applicationRoleUserDataAccess.RemoveRolesUser(userId, roleIdentifier);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Could not insert or update user {userId} on signin", userId);
            }

            return true;
        }

        private bool HasUserChanges(User newUser, User originalUser)
        {
            if (newUser.EiamRoles != originalUser.EiamRoles)
            {
                return true;
            }

            if (newUser.FamilyName != originalUser.FamilyName)
            {
                return true;
            }

            if (newUser.FirstName != originalUser.FirstName)
            {
                return true;
            }

            if (newUser.EmailAddress != originalUser.EmailAddress)
            {
                return true;
            }

            return false;
        }

        internal AuthStatus IsValidAuthRole(string role, bool isPublicClient)
        {
            if (string.IsNullOrWhiteSpace(role))
            {
                return AuthStatus.KeineRolleDefiniert;
            }

            /*
             Die folgende Tabelle enthält die Regeln für die Überprüfung

             Rolle	Affiliation	HomeOrganization	Bemerkung
             Ö2	    egal	    egal	            Wenn jemand von Shibboleth autorisiert wurde kann er in jedem Fall die Rolle Ö2 haben.
             Ö3	    egal	    egal	            Wenn jemand von Shibboleth autorisiert wurde, darf er auch die Rolle Ö3 haben. Es wird keine 2-Faktor Authentifizierung gefordert (auch nicht für andere User-Stufen).
             EMA    staff	    ethz.ch	            Damit jemand die EMA-Rolle haben darf, muss er an der ETH arbeiten.
             AS	    staff	    ethz.ch	            Damit jemand die AS-Rolle haben darf, muss er an der ETH arbeiten.
             AMA    staff	    ethz.ch	            Damit jemand die AMA-Rolle haben darf, muss er an der ETH arbeiten.

             * */

            // Public-Client
            if (isPublicClient)
            {
                switch (role.GetRolePublicClientEnum())
                {
                    // Keine spezial Behandlung
                    case AccessRolesEnum.Ö2:
                    case AccessRolesEnum.Ö3:
                        return AuthStatus.Ok;

                    case AccessRolesEnum.EMA:
                    case AccessRolesEnum.AS:
                    case AccessRolesEnum.AMA:
                        if (controllerHelper.IsInternalUser())
                        {
                            return AuthStatus.Ok;
                        } 
                        throw new AuthenticationException("Interne Benutzerrollen (EMA, AS und AMA) müssen als Staff der ETH Zürich deklariert sein");

                    default:
                        throw new InvalidOperationException("Nicht definiertes Rollen handling");
                }
            }

            // Management-Client
            switch (role)
            {
                // Kerberos Pflicht
                case AccessRoles.RoleMgntAllow:
                case AccessRoles.RoleMgntAppo:
                    return controllerHelper.IsStaff()
                        ? AuthStatus.Ok
                        : AuthStatus.KeineKerberosAuthentication;
                default:
                    return AuthStatus.KeineRolleDefiniert;
            }
        }

        internal void AddAppRolesAndFeatures(string userId, Identity identity)
        {
            try
            {
                var user = userDataAccess.GetUser(userId);
                if (user != null && user.Roles.Any())
                {
                    identity.ApplicationRoles = user.Roles;
                    identity.ApplicationFeatures = user.Features.ToInfos();
                }

                if (HttpContext.Current?.Session != null )
                {
                    HttpContext.Current.Session.SetApplicationUser(user);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Could not fetch user info for user {userId}", userId);
            }
        }

        private bool HasValidMandant(IList<ClaimInfo> claims)
        {
            var claimsRoles = claims.FirstOrDefault(c => c.Type.EndsWith("affiliation"))?.Value;
            Log.Information($"Claim Rolle {claimsRoles}");
            return !string.IsNullOrWhiteSpace(claimsRoles);
        }

        private string GetOeDreiKeineMobilenummerErfasst()
        {
            return webCmiConfigProvider.GetStringSetting("oeDreiKeineMobilenummerErfasst",
                "");
        }

        private string GetErrorPermissionUrl()
        {
            return webCmiConfigProvider.GetStringSetting("errorpermissionURl",
                "https://recherche.library.ethz.ch/management//errorpermission");
        }

        private string GetPublicClientUrl()
        {
            return webCmiConfigProvider.GetStringSetting("publicClientUrl",
                "https://recherche.library.ethz.ch/client");
        }

        private string GetReturnUrl(AuthStatus authStatus, bool isPublicClient)
        {
            if (authStatus == AuthStatus.KeineMTanAuthentication)
            {
                return GetOeDreiKeineMobilenummerErfasst();
            }

            if (!isPublicClient && authStatus == AuthStatus.NeuerBenutzer)
            {
                return GetPublicClientUrl();
            }

            if (!isPublicClient && (authStatus == AuthStatus.KeineRolleDefiniert || authStatus == AuthStatus.KeineKerberosAuthentication))
            {
                return GetErrorPermissionUrl();
            }

            return string.Empty;
        }
    }
}