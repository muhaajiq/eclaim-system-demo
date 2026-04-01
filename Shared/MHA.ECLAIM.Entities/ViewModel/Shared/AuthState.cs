using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Web;

namespace MHA.ECLAIM.Entities.ViewModel.Shared
{
    public class AuthState
    {
        private readonly ITokenAcquisition _tokenAcquisition;
        private readonly IConfiguration _config;
        private readonly NavigationManager _nav;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private string? _accessToken;

        public AuthState(ITokenAcquisition tokenAcquisition, IConfiguration config, NavigationManager nav, IHttpContextAccessor httpContextAccessor)
        {
            _tokenAcquisition = tokenAcquisition;
            _config = config;
            _nav = nav;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<string?> EnsureTokenAsync()
        {
            if (!string.IsNullOrEmpty(_accessToken))
                return _accessToken;

            var scopes = _config.GetSection("DownstreamApi:Scopes").Get<string[]>() ?? Array.Empty<string>();

            try
            {
                _accessToken = await _tokenAcquisition.GetAccessTokenForUserAsync(scopes);
                return _accessToken;
            }
            catch
            {
                _accessToken = null;

                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext != null)
                {
                    await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

                    var properties = new AuthenticationProperties
                    {
                        RedirectUri = _nav.Uri
                    };

                    await httpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, httpContext.User, properties);
                }

                return null;
            }
        }
    }
}
