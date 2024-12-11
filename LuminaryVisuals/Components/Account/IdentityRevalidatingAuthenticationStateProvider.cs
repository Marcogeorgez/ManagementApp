using LuminaryVisuals.Data.Entities;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace LuminaryVisuals.Components.Account
{
    internal sealed class IdentityRevalidatingAuthenticationStateProvider(
            ILoggerFactory loggerFactory,
            IServiceScopeFactory scopeFactory,
            IOptions<IdentityOptions> options,
            DeviceSessionService deviceSessionService,
            IHttpContextAccessor httpContextAccessor)
        : RevalidatingServerAuthenticationStateProvider(loggerFactory)
    {
        private readonly DeviceSessionService _deviceSessionService = deviceSessionService;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

        protected override TimeSpan RevalidationInterval => TimeSpan.FromSeconds(5);

        protected override async Task<bool> ValidateAuthenticationStateAsync(
            AuthenticationState authenticationState, CancellationToken cancellationToken)
        {
            // Get the user manager from a new scope to ensure it fetches fresh data
            await using var scope = scopeFactory.CreateAsyncScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            return await ValidateSecurityStampAsync(userManager, authenticationState.User);
        }

        private async Task<bool> ValidateSecurityStampAsync(UserManager<ApplicationUser> userManager, ClaimsPrincipal principal)
        {
            var user = await userManager.GetUserAsync(principal);
            if (user is null)
            {
                return false;
            }
            else if (!userManager.SupportsUserSecurityStamp)
            {
                return true;
            }
            else
            {
                var principalStamp = principal.FindFirstValue(options.Value.ClaimsIdentity.SecurityStampClaimType);
                var userStamp = await userManager.GetSecurityStampAsync(user);
                return principalStamp == userStamp;
            }
        }

        // New method to handle login with device session tracking
        // New method to handle login with device session tracking
        public async Task ExternalLoginWithDeviceTracking(ClaimsPrincipal principal)
        {
            // Find the user based on the external login principal
            await using var scope = scopeFactory.CreateAsyncScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            var user = await userManager.GetUserAsync(principal);
            if (user == null)
            {
                return;
            }

            // Extract device information
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
            {
                return;
            }

            var userAgent = httpContext.Request.Headers["User-Agent"].ToString();

            string deviceType = GetBrowserType(userAgent);
            string browserVersion = GetBrowserVersion(userAgent);
            string operatingSystem = GetOperatingSystem(userAgent);

            // Register the device session
            _deviceSessionService.RegisterDeviceSession(
                user.Id,
                deviceType,
                browserVersion,
                operatingSystem
            );

            // Create a new claims identity with the device details
            var claims = principal.Claims.ToList();
            claims.Add(new Claim("DeviceType", deviceType));
            claims.Add(new Claim("BrowserVersion", browserVersion));
            claims.Add(new Claim("OperatingSystem", operatingSystem));

            var newIdentity = new ClaimsIdentity(claims, principal.Identity.AuthenticationType);
            var newPrincipal = new ClaimsPrincipal(newIdentity);

            // Notify authentication state change
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(newPrincipal)));
        }

        private string GetBrowserType(string userAgent)
        {
            userAgent = userAgent.ToLower();
            if (userAgent.Contains("edg/"))
                return "Edge";
            if (userAgent.Contains("chrome/"))
                return "Chrome";
            if (userAgent.Contains("firefox/"))
                return "Firefox";
            if (userAgent.Contains("safari/") && !userAgent.Contains("chrome/"))
                return "Safari";
            if (userAgent.Contains("trident/") || userAgent.Contains("msie"))
                return "Internet Explorer";
            return "Unknown";
        }

        private string GetBrowserVersion(string userAgent)
        {
            // Implement version extraction logic
            var browserType = GetBrowserType(userAgent);
            var versionRegex = new Regex($@"{browserType.ToLower()}/(\d+)");
            var match = versionRegex.Match(userAgent.ToLower());
            return match.Success ? match.Groups[1].Value : "Unknown";
        }

        private string GetOperatingSystem(string userAgent)
        {
            userAgent = userAgent.ToLower();
            if (userAgent.Contains("windows"))
                return "Windows";
            if (userAgent.Contains("mac os x"))
                return "macOS";
            if (userAgent.Contains("linux"))
                return "Linux";
            if (userAgent.Contains("android"))
                return "Android";
            if (userAgent.Contains("ios"))
                return "iOS";
            return "Unknown";
        }
    }
}