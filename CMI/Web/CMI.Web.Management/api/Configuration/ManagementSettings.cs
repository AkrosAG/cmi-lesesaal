using System.Reflection;
using CMI.Web.Common.api;
using CMI.Web.Common.Helpers;

namespace CMI.Web.Management.api.Configuration
{
    public class ManagementSettings : AppSettings
    {
        static ManagementSettings()
        {
            Instance = new ManagementSettings();
        }

        private ManagementSettings()
            : base(Assembly.GetExecutingAssembly())
        {
        }

        public static ManagementSettings Instance { get; }
        public string SqlConnectionString { get; } = WebHelper.Settings["sqlConnectionString"];
        public string SqlEFConnectionString { get; } = WebHelper.Settings["sqlefConnectionString"];
        public int CookieExpireTimeInMinutes { get; } = WebHelper.GetIntSetting("cookieExpireTimeInMinutes", 60);
    }
}
