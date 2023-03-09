using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Security.Authentication;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.ModelBinding;
using System.Web.SessionState;
using System.Web.UI.WebControls;
using CMI.Access.Sql.Lesesaal;
using CMI.Web.Common.api;
using CMI.Web.Common.api.Attributes;
using CMI.Web.Common.Auth;
using CMI.Web.Common.Helpers;
using System.Xml;
using Serilog;
using System.IO;

namespace CMI.Web.Frontend.api.Controllers
{
    [Authorize]
    [NoCache]
    public sealed class AuthController : ApiFrontendControllerBase
    {
        private readonly AuthControllerHelper authControllerHelper;

        public AuthController(
            IUserDataAccess userDataAccess,
            IAuthenticationHelper authenticationHelper,
            IWebCmiConfigProvider webCmiConfigProvider,
            IApplicationRoleUserDataAccess applicationRoleUserDataAccess)
        {
            Log.Information("AuthController");
            authControllerHelper = new AuthControllerHelper(applicationRoleUserDataAccess, userDataAccess, ControllerHelper, authenticationHelper,
                webCmiConfigProvider);
        }

        [AllowAnonymous]
        [Route("Auth/ExternalSignIn")]
        [HttpGet]
        public async Task<IHttpActionResult> OnExternalSignIn()
        {
            try
            {
                Log.Information("OnExternalSignIn");
                await authControllerHelper.OnExternalSignIn(Request.GetOwinContext(), true);
            }
            catch (AuthenticationException e)
            {
                Log.Error(e, "Fehler beim Anmelden");
            }

            return Redirect(WebHelper.FrontendAuthReturnUrl);
        }

        [AllowAnonymous]
        [Route("AuthServices/SignIn")]
        [HttpGet]
        public async Task<IHttpActionResult> SignIn()
        {
            try
            {
                Log.Information("SignIn");
                await authControllerHelper.OnExternalSignIn(Request.GetOwinContext(), true);
            }
            catch (AuthenticationException e)
            {
                Log.Error(e, "Fehler beim Anmelden");
            }

            return Redirect(WebHelper.FrontendAuthReturnUrl);
        }


        [AllowAnonymous]
        [Route("Auth/ExternalSignOut")]
        [HttpGet]
        public IHttpActionResult OnExternalSignOut()
        {
            try
            {
                Log.Information("ExternalSignOut");
                authControllerHelper.OnExternalSignOut(Request.GetOwinContext(), true);
            }
            catch (AuthenticationException e)
            {
                Log.Error(e, "Fehler beim Abmelden");
            }

            return Redirect(WebHelper.FrontendLogoutReturnUrl);
        }


        // This method is called when IAM-authentication was successful
        [AllowAnonymous]
        [HttpGet]
        public IHttpActionResult GetIdentity()
        {
            try
            {
                string URLString = "https://recherche-dev.library.ethz.ch/Shibboleth.sso/Session";
                string html = File.ReadAllText(URLString);

                Log.Information($"GetIdentity {html} " );
                Log.Information("Server Variables ");


                var items = HttpContext.Current.Items.Keys;
                foreach (var key in items)
                {
                    if(key.ToString()== "AspSession")
                    {
                        Log.Information((HttpContext.Current.Items[key] as HttpSessionState).CookieMode.ToString());
                    }
                    else if (key.ToString() == "owin.Environment")
                    {
                        Log.Information($"owin.Environment");
                        var owin = HttpContext.Current.Items[key] as IDictionary<string, object>;
                            foreach (var kw in owin.Keys)
                        {
                            Log.Information($"{kw} {owin[kw]}");
                        }

                     
                    }
                    Log.Information($"Item: {key}: {HttpContext.Current.Items[key]}");
                }               
                foreach (var key in HttpContext.Current.Request.ServerVariables.AllKeys)
                {
                    // Log.Debug($"variable: {variable}");
                    Log.Information($"ServerVariable: {key}: {HttpContext.Current.Request.ServerVariables[key]}");
                }

                var identity = authControllerHelper.GetIdentity(Request, User, true);
                return Ok(identity);
            }
            catch (AuthenticationException e)
            {
                return Content(HttpStatusCode.Forbidden, e.Message);
            }
        }
    }
}