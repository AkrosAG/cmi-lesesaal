using CMI.Web.Common.Auth.Sibboleth.Types;
using System.Collections.Generic;
using System.Web;

namespace CMI.Web.Common.Auth.Sibboleth
{
    /// <summary>
    /// Local development claims - must be overridden in each app to manually specify a user/claims
    /// </summary>
    public abstract class LocalDevClaimsAuthenticationHttpModule : ShibbolethClaimsAuthenticationHttpModule
    {
        protected abstract override ShibbolethAttributeValueCollection GetAttributesFromRequest(HttpRequest request, ShibbolethSessionType sessionType);

        protected override ShibbolethSessionType IsShibbolethSession(HttpRequest request)
        {
            // return something other than None
            return ShibbolethSessionType.Variable;
        }
    }

    public class LocalEthDevClaimsAuthenticationHttpModule : LocalDevClaimsAuthenticationHttpModule
    {
        protected override ShibbolethAttributeValueCollection GetAttributesFromRequest(HttpRequest request, ShibbolethSessionType sessionType)
        {
            var retVal = new Dictionary<string, ShibbolethAttributeValueCollection>
            {
                {
                    "Ö3 Benutzer",
                    new ShibbolethAttributeValueCollection
                    {
                        new ShibbolethAttributeValue("givenName", "Jörg"),
                        new ShibbolethAttributeValue("persistent-id", "https://aai-logon.ethz.ch/idp/shibboleth!https://recherche.library.ethz.ch/shibboleth!XpsCjWeL4TVehzLeqTOo/vhfluA="),
                        new ShibbolethAttributeValue("affiliation", "member"),
                        new ShibbolethAttributeValue("mail", "jlang@evelix.ch"),
                        new ShibbolethAttributeValue("homeOrganization", "ethz.ch"),
                        new ShibbolethAttributeValue("homeOrganizationType", "university"),
                        new ShibbolethAttributeValue("scoped-affiliation", "member@ethz.ch"),
                        new ShibbolethAttributeValue("surname", "Lang"),
                        new ShibbolethAttributeValue("uniqueID", "4043690@ethz.ch"),
                    }
                },

                {
                    "Interner Benutzer",
                    new ShibbolethAttributeValueCollection
                    {
                        new ShibbolethAttributeValue("givenName", "Alec"),
                        new ShibbolethAttributeValue("persistent-id", "https://aai-logon.ethz.ch/idp/shibboleth!https://recherche.library.ethz.ch/shibboleth!XpsCjWeL4TVehzLeqTOo/vhfluA="),
                        new ShibbolethAttributeValue("affiliation", "staff"),
                        new ShibbolethAttributeValue("mail", "alang@evelix.ch"),
                        new ShibbolethAttributeValue("homeOrganization", "ethz.ch"),
                        new ShibbolethAttributeValue("homeOrganizationType", "university"),
                        new ShibbolethAttributeValue("scoped-affiliation", "staff@ethz.ch"),
                        new ShibbolethAttributeValue("surname", "Lang-Intern"),
                        new ShibbolethAttributeValue("uniqueID", "1043690@ethz.ch"),
                    }
                },
                {
                    "Ö2 Benutzer",
                    new ShibbolethAttributeValueCollection
                    {
                        new ShibbolethAttributeValue("givenName", "Ulrich"),
                        new ShibbolethAttributeValue("persistent-id", "https://aai-logon.ethz.ch/idp/shibboleth!https://recherche.library.ethz.ch/shibboleth!XpsCjWeL4TVehzLeqTOo/vhfluA="),
                        new ShibbolethAttributeValue("affiliation", "member"),
                        new ShibbolethAttributeValue("mail", "uguenther@ensocon.net"),
                        new ShibbolethAttributeValue("homeOrganization", "eduid.ch"),
                        new ShibbolethAttributeValue("homeOrganizationType", "eduid"),
                        new ShibbolethAttributeValue("scoped-affiliation", "office@ensocon.net"),
                        new ShibbolethAttributeValue("surname", "Guenther-Intern"),
                        new ShibbolethAttributeValue("uniqueID", "2043690@ethz.ch"),
                    }
                }
            };

            // Hier anpassen, welcher Benutzer zurückgeliefert werden soll
            return retVal["Ö3 Benutzerr"];
        }
    }
}
