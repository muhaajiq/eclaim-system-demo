using MHA.Framework.Core.General;
using MHA.ECLAIM.Framework.JSONConstants;

namespace MHA.ECLAIM.Framework.Helpers
{
    public class ConnectionStringHelper
    {
        private static readonly JSONAppSettings appSettings;
        private static readonly JSONAppSettings connStrings;

        static ConnectionStringHelper()
        {
            connStrings = ConfigurationManager.GetConnStrings();
            appSettings = ConfigurationManager.GetAppSetting();
        }

        public static string GetDBLogConnString()
        {
            string decryptKey = appSettings.AG_ENCKEY;
            string decryptedKey = EncryptionHelper.Decrypt(decryptKey);

            string logConnString = connStrings.dbLogConnString;
            logConnString = EncryptionHelper.Decrypt(logConnString, decryptedKey);
            return logConnString;
        }

        public static string GetGenericWFConnString()
        {
            string decryptKey = appSettings.AG_ENCKEY;
            string decryptedKey = EncryptionHelper.Decrypt(decryptKey);

            string WFConnString = connStrings.dbWFConnString;
            WFConnString = EncryptionHelper.Decrypt(WFConnString, decryptedKey);
            return WFConnString;
        }
    }
}
