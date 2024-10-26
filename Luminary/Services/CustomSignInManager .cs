using Luminary.Data.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace Luminary.Services
{
    public class CustomSignInManager : SignInManager<ApplicationUser>
    {
        public CustomSignInManager(
            UserManager<ApplicationUser> userManager,
            IHttpContextAccessor contextAccessor,
            IUserClaimsPrincipalFactory<ApplicationUser> claimsFactory,
            IOptions<IdentityOptions> optionsAccessor,
            ILogger<SignInManager<ApplicationUser>> logger,
            IAuthenticationSchemeProvider schemes,
            IUserConfirmation<ApplicationUser> confirmation)
            : base(userManager, contextAccessor, claimsFactory, optionsAccessor, logger, schemes, confirmation)
        {
        }


        // Override password sign-in to prevent local authentication
        public override Task<SignInResult> PasswordSignInAsync(string userName, string password, bool isPersistent, bool lockoutOnFailure)
        {
            return Task.FromResult(SignInResult.Failed);
        }

        // Override to prevent any other external logins except Google
        public override async Task<SignInResult> ExternalLoginSignInAsync(
            string loginProvider,
            string providerKey,
            bool isPersistent,
            bool bypassTwoFactor)
        {
            // Only allow Google authentication
            if (loginProvider != "Google")
            {
                return SignInResult.Failed;
            }

            try
            {
                var result = await base.ExternalLoginSignInAsync(loginProvider, providerKey, isPersistent, bypassTwoFactor);

                // Only proceed if sign-in failed (meaning this might be a new user)
                if (!result.Succeeded)
                {
                    var info = await base.GetExternalLoginInfoAsync();
                    if (info == null)
                    {
                        return SignInResult.Failed;
                    }

                    // Check if user already exists
                    var user = await UserManager.FindByLoginAsync(loginProvider, providerKey);
                    if (user == null)
                    {
                        var email = info.Principal.FindFirstValue(ClaimTypes.Email);

                        // Create new user
                        user = new ApplicationUser
                        {
                            UserName = email, // Use email as username
                            Email = email,
                            EmailConfirmed = true // Google emails are already verified
                        };

                        var createResult = await UserManager.CreateAsync(user);
                        if (createResult.Succeeded)
                        {
                            // Add external login
                            var addLoginResult = await UserManager.AddLoginAsync(user, info);
                            if (addLoginResult.Succeeded)
                            {
                                // Assign Guest role only for new users
                                await UserManager.AddToRoleAsync(user, "Guest");

                                // Sign in the new user
                                await SignInAsync(user, isPersistent);
                                return SignInResult.Success;
                            }
                        }
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error during Google authentication: {ex.Message}");
                return SignInResult.Failed;
            }
        }
    }
}