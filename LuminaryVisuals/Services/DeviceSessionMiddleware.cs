using LuminaryVisuals.Data.Entities;
using Microsoft.AspNetCore.Identity;
using System.Text.RegularExpressions;
using System.Security.Claims;

namespace LuminaryVisuals.Services
{
    public class DeviceSessionMiddleware
    {
        private readonly RequestDelegate _next;

        public DeviceSessionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context,
            DeviceSessionService deviceSessionService,
            UserManager<ApplicationUser> userManager)
        {
            // Check if user is authenticated
            if (context.User.Identity?.IsAuthenticated == true)
            {
                var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(userId))
                {
                    var userAgent = context.Request.Headers["User-Agent"].ToString();

                    // Extract device information
                    var deviceType = GetBrowserType(userAgent);
                    var browserVersion = GetBrowserVersion(userAgent);
                    var operatingSystem = GetOperatingSystem(userAgent);

                    // Register device session
                    deviceSessionService.RegisterDeviceSession(
                        userId,
                        deviceType,
                        browserVersion,
                        operatingSystem
                    );
                }
            }
            await _next(context);
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

    // Extension method for middleware
    public static class DeviceSessionMiddlewareExtensions
    {
        public static IApplicationBuilder UseDeviceSessionTracking(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<DeviceSessionMiddleware>();
        }
    }
}