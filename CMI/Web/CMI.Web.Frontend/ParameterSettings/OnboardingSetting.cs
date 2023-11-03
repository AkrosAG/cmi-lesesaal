using System.ComponentModel;
using CMI.Contract.Parameter;

namespace CMI.Web.Frontend.ParameterSettings
{
    public class OnboardingSetting : ISetting
    {
        [Description("URI, an welche die Benutzer weitergeleitet werden für das Onboarding")]
        [DefaultValue("")]
        public string UriTemplate { get; set; }
    }
}