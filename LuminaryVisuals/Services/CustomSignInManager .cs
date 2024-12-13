using LuminaryVisuals.Data.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace LuminaryVisuals.Services
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
        public override async Task<SignInResult> PasswordSignInAsync(string userName, string password, bool isPersistent, bool lockoutOnFailure)
        {
            // Return Task.FromResult(SignInResult.Failed);
            var user = await UserManager.FindByNameAsync(userName);
            if (user != null)
            {
                // Attempt to sign in with the provided password
                var result = await base.PasswordSignInAsync(userName, password, isPersistent, lockoutOnFailure);

                if (result.Succeeded)
                {
                    return result; // Return success if login is successful
                }
            }

            // If login failed, create a new user
            var newUser = new ApplicationUser
            {
                UserName = userName,
                Email = userName, // Or another way to assign email if you have it
                EmailConfirmed = true // Assuming this is a guest
            };

            var createResult = await UserManager.CreateAsync(newUser, password);
            if (createResult.Succeeded)
            {
                // Assign Guest role to the new user
                await UserManager.AddToRoleAsync(newUser, "Guest");
                // Sign in the new user
                await SignInAsync(newUser, isPersistent);
                return SignInResult.Success; // Return success after signing in
            }

            // Return failed if the user could not be created
            return SignInResult.Failed;
        }
    

    // Override to prevent any other external logins except Google
    public override async Task<SignInResult> ExternalLoginSignInAsync(
            string loginProvider,
            string providerKey,
            bool isPersistent,
            bool bypassTwoFactor)
        {
            ApplicationUser user = null;
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
                        throw new Exception($"Sign In Failed! There is no account found. Try contact us.");
                    }

                    // Check if user already exists
                    user = await UserManager.FindByLoginAsync(loginProvider, providerKey);
                    if (user == null)
                    {
                        var email = info.Principal.FindFirstValue(ClaimTypes.Email);

                        // Create new user
                        user = new ApplicationUser
                        {
                            UserName = info.Principal.FindFirstValue(ClaimTypes.Name),
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
                        else
                        {
                            throw new Exception($"Can't register this account, because of {createResult.Errors}");
                        }
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error during Google authentication: {ex.InnerException}, Date: {DateTime.UtcNow}, User: {user}");
                return SignInResult.Failed;
            }
        }
    }
    public class SignInResultWithMessage
    {
        public SignInResult Result { get; set; }
        public string ErrorMessage { get; set; }
    }
}