using LuminaryVisuals.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LuminaryVisuals.Services;

public class UserManagementService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<UserManagementService> _logger;

    public UserManagementService(
        UserManager<ApplicationUser> userManager,
        ILogger<UserManagementService> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<List<UserRoleViewModel>> GetAllUsersWithRolesAsync()
    {
        _logger.LogInformation("Getting all users with roles");
        var users = await _userManager.Users.ToListAsync();
        var userRoles = new List<UserRoleViewModel>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            _logger.LogInformation($"User {user.UserName} has roles: {string.Join(", ", roles)}");

            userRoles.Add(new UserRoleViewModel
            {
                UserId = user.Id,
                UserName = user.UserName,
                Roles = roles.ToList()
            });
        }

        return userRoles;
    }

    public async Task<bool> ChangeUserRoleAsync(string userId, string newRole)
    {
        _logger.LogInformation($"Changing role for user {userId} to {newRole}");

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            _logger.LogWarning($"User {userId} not found");
            return false;
        }

        var currentRoles = await _userManager.GetRolesAsync(user);
        _logger.LogInformation($"Current roles for user {user.UserName}: {string.Join(", ", currentRoles)}");

        var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
        if (!removeResult.Succeeded)
        {
            _logger.LogError($"Failed to remove current roles: {string.Join(", ", removeResult.Errors)}");
            return false;
        }

        var addResult = await _userManager.AddToRoleAsync(user, newRole);
        if (!addResult.Succeeded)
        {
            _logger.LogError($"Failed to add new role: {string.Join(", ", addResult.Errors)}");
            return false;
        }

        _logger.LogInformation($"Successfully changed role for user {user.UserName} to {newRole}");
        return true;
    }
}

public class UserRoleViewModel
{
    public string UserId { get; set; }
    public string UserName { get; set; }
    public List<string> Roles { get; set; }
    public string SelectedRole { get; set; }
}