using System.Reflection;
using CMI.Web.Common.api;
using CMI.Web.Common.Helpers;

namespace CMI.Web.Frontend.api.Configuration
{
    public class FrontendSettings : AppSettings
    {
        static FrontendSettings()
        {
            Instance = new FrontendSettings();
        }

        private FrontendSettings()
            : base(Assembly.GetExecutingAssembly())
        {
        }

        public static FrontendSettings Instance { get; }
        public string SqlConnectionString { get; } = WebHelper.Settings["sqlConnectionString"];
        public string SqlEFConnectionString { get; } = WebHelper.Settings["sqlefConnectionString"];
        public int CookieExpireTimeInMinutes { get; } = WebHelper.GetIntSetting("cookieExpireTimeInMinutes", 60);
    }
}
