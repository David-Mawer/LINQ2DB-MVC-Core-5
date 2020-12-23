using Microsoft.Extensions.Configuration;

namespace LINQ2DB_MVC_Core_5.Models
{
    public class SSOConfigModel
    {
        public string ID { get; }
        public string Secret { get; }
        public bool HasSettings => (ID.Length != 0) && (Secret.Length != 0);

        public SSOConfigModel(IConfiguration oConfig, string sAuthenticationConfigSection, string sIdKey, string sSecretKey)
        {
            var oGoogleSSO = oConfig.GetSection("Authentication").GetSection(sAuthenticationConfigSection);
            ID = oGoogleSSO.GetSection(sIdKey).Value ?? "";
            Secret = oGoogleSSO.GetSection(sSecretKey).Value ?? "";
        }
    }
}
