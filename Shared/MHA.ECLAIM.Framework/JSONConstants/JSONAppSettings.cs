namespace MHA.ECLAIM.Framework.JSONConstants
{
    public class JSONAppSettings
    {
        #region Azure AD Configuration
        public string TenantId { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string CertName { get; set; }
        public string CertPassword { get; set; }
        #endregion

        #region Date & Time Settings
        public string CentralTimeZone { get; set; }
        public string DefaultDateFormat { get; set; }
        public string DefaultDateTimeFormat { get; set; }
        public string DefaultDateTimeWithSecondFormat { get; set; }
        public string DefaultReportDateFormat { get; set; }
        #endregion

        #region Retry & Throttling Settings
        public string NoOfRetry { get; set; }
        public string EmailNoOfRetry { get; set; }
        public string ThrottlingSleeptime { get; set; }
        public string EmailThrottlingSleeptime { get; set; }
        public string ThrottlingTimeIncrease { get; set; }
        #endregion

        #region Other Settings
        public string DebugMode { get; set; }
        public string DynamicDUeDate { get; set; }
        public string AG_UseSPTimeZoneforDBDateTimeColumn { get; set; }
        #endregion

        #region Connection Strings

        public string AG_ENCKEY { get; set; }
        public string dbLogConnString { get; set; }
        public string dbWFConnString { get; set; }
        #endregion

        #region SMTP
        public string AG_EMAILHOST { get; set; }
        public string AG_EMAILPORT { get; set; }
        public string AG_EMAILDEFAULTCREDENTIAL { get; set; }
        public string AG_EMAILUSESSL { get; set; }
        public string AG_EMAILUSEDEFAULTCREDENTIAL { get; set; }
        public string AG_EMAILFROM { get; set; }
        public string AG_EMAILUSERNAME { get; set; }
        public string AG_EMAILPASSWORD { get; set; }
        public string AG_SECONDEMAILFROM { get; set; }
        public string AG_SECONDEMAILUSERNAME { get; set; }
        public string AG_SECONDEMAILPASSWORD { get; set; }
        public string rerAccountEmail { get; set; }
        public string rerAccountPassword { get; set; }
        #endregion

        #region URLs
        public string RemoteAppURL { get; set; }
        public string WebAPIUrl { get; set; }
        public static string TravelAppUrl { get; set; }

        public string DefaultSPSiteLogoURL { get; set; }
        #endregion

        #region Emails
        public string MaximumEmailRecipients { get; set; }
        #endregion

        #region Row Limitation
        public static string HomePagingShowLimit { get; set; }
        #endregion

    }
}
