using System.Security.Cryptography.X509Certificates;
using System.Security;
using PnP.Core.Services;
using Microsoft.Identity.Web;
using Microsoft.AspNetCore.Hosting;
using PnP.Core.Auth;
using PnP.Framework;
using Microsoft.SharePoint.Client;
using Microsoft.Extensions.Options;
using MHA.ECLAIM.Framework.JSONConstants;

namespace MHA.ECLAIM.Framework.Helpers
{
    public class TokenHelper
    {
        private readonly IPnPContextFactory _pnpContextFactory;
        private readonly ITokenAcquisition _tokenAcquisition;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly JSONAppSettings appSettings;

        private readonly string[] scopes = new string[] { "https://mha.sharepoint.com/.default" };

        public TokenHelper(IPnPContextFactory pnpContextFactory, ITokenAcquisition tokenAcquisition, IWebHostEnvironment webHostEnvironment)
        {
            _pnpContextFactory = pnpContextFactory;
            _tokenAcquisition = tokenAcquisition;
            _webHostEnvironment = webHostEnvironment;
            appSettings = ConfigurationManager.GetAppSetting();
        }

        public async Task<PnPContext> CreateSiteContext(string spHostUrl)
        {
            var siteUrl = new Uri(spHostUrl);
            var scopes = new[] { "https://mha.sharepoint.com/.default" };
            return await _pnpContextFactory.CreateAsync(siteUrl,
                            new ExternalAuthenticationProvider((resourceUri, scopes) =>
                            {
                                return _tokenAcquisition.GetAccessTokenForUserAsync(scopes);
                            }
                            ));
        }

        public async Task<string> GetUserAccessToken()
        {
            return await _tokenAcquisition.GetAccessTokenForUserAsync(scopes);
        }

        public async Task<string> GetAccessTokenFromHybridApp(string SPHostURL)
        {
            string accessToken = string.Empty;

            string certificatePath = Path.Combine(_webHostEnvironment.ContentRootPath, "wwwroot", "Certs", appSettings.CertName);

            X509Certificate2 certificate2 = new X509Certificate2(certificatePath, appSettings.CertPassword);

            var am_1 = AuthenticationManager.CreateWithCertificate(appSettings.ClientId, certificate2, appSettings.TenantId);
            accessToken = await Task.Run(() => am_1.GetAccessToken(SPHostURL));

            return accessToken;
        }

        public static bool CheckValidAccessToken(string accessToken, string spHostUrl)
        {
            bool IsValid = false;
            try
            {
                if (!string.IsNullOrEmpty(accessToken))
                {
                    var authManager = AuthenticationManager.CreateWithAccessToken(ConvertAccessTokenToSecureString(accessToken));

                    using (ClientContext clientContext = authManager.GetContext(spHostUrl))
                    {
                        clientContext.ExecuteQueryWithIncrementalRetry();
                        IsValid = true;
                    }
                }
                else
                {
                    throw new Exception("No access token retrieved.");
                }
            }
            catch (System.Net.WebException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception("Check valid access token error. ", ex);
            }
            return IsValid;
        }

        public static SecureString ConvertAccessTokenToSecureString(string accessToken)
        {
            SecureString secureString = new SecureString();
            for (int i = 0; i < accessToken.Length; i++)
                secureString.AppendChar(accessToken[i]);

            return secureString;
        }

        public static ClientContext GetClientContextWithAccessToken(string spHostUrl, string accessToken)
        {
            if (string.IsNullOrWhiteSpace(spHostUrl))
                throw new ArgumentException("SharePoint host URL cannot be null or empty.", nameof(spHostUrl));
            if (string.IsNullOrWhiteSpace(accessToken))
                throw new ArgumentException("Access token cannot be null or empty.", nameof(accessToken));

            try
            {
                var authManager = AuthenticationManager.CreateWithAccessToken(ConvertAccessTokenToSecureString(accessToken));
                var clientContext = authManager.GetContext(spHostUrl);

                if (clientContext == null)
                    throw new InvalidOperationException("Failed to create SharePoint ClientContext.");

                return clientContext;
            }
            catch (Exception ex)
            {
                // Optionally log the exception here
                throw new ApplicationException("Error obtaining SharePoint ClientContext.", ex);
            }
        }

    }
}
