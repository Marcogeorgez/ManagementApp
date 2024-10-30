using Microsoft.AspNetCore.Identity;

namespace LuminaryVisuals.Services;

public class RoleInitializer
{
    public static async Task InitializeRoles(IServiceProvider serviceProvider)
    {
        try
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            string[] roleNames = { "Admin", "Editor", "Client", "Guest" };

            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }
        }
        catch (Exception ex)
        {
            var logger = serviceProvider.GetService<ILogger<RoleInitializer>>();
            logger?.LogError($"Error initializing roles: {ex.Message}");
        }
    }
}