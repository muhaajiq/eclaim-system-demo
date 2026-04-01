using MHA.ECLAIM.Framework.JSONConstants;
using Microsoft.Extensions.Configuration;

namespace MHA.ECLAIM.Framework.Helpers
{
    public static class ConfigurationManager
    {
        private static readonly IConfiguration config;

        static ConfigurationManager()
        {
            config = new ConfigurationBuilder()
               .SetBasePath(AppContext.BaseDirectory)
               .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
               .Build();
        }

        public static JSONAppSettings GetAppSetting()
        {
            JSONAppSettings jSONAppSettings = new JSONAppSettings();

            jSONAppSettings = config.GetSection("AppSettings").Get<JSONAppSettings>();

            return jSONAppSettings;
        }

        public static JSONAppSettings GetConnStrings()
        {
            JSONAppSettings jSONAppSettings = new JSONAppSettings();

            jSONAppSettings = config.GetSection("ConnectionStrings").Get<JSONAppSettings>();

            return jSONAppSettings;
        }
    }
}
