using LuminaryVisuals.Data;
using LuminaryVisuals.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using static LuminaryVisuals.Services.Core.UserRoleViewModel;

namespace LuminaryVisuals.Services.Core;

public class UserServices
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ILogger<UserServices> _logger;
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
    public UserServices(
        UserManager<ApplicationUser> userManager,
        IDbContextFactory<ApplicationDbContext> context,
        RoleManager<IdentityRole> roleManager,
        ILogger<UserServices> logger)
    {
        _contextFactory = context;
        _userManager = userManager;
        _roleManager = roleManager;
        _logger = logger;
    }
    public async Task<List<string>> GetAllUsersForRoleAsync(string? role)
    {
        using var context = _contextFactory.CreateDbContext();

        if (role == null)
        {
            return await _userManager.Users.Select(u => u.Id).ToListAsync();
        }

        var usersInRole = role switch
        {
            "Client" => await _userManager.GetUsersInRoleAsync("Client"),
            "Editor" => await _userManager.GetUsersInRoleAsync("Editor"),
            "Admin" => await _userManager.GetUsersInRoleAsync("Admin"),
            _ => new List<ApplicationUser>()
        };

        return usersInRole.Select(u => u.Id).ToList(); 
    }



    public async Task<List<UserRoleViewModel>> GetAllUsersForAdminDashboardAsync(decimal? storedValue)
    {
        using (var context = _contextFactory.CreateDbContext())
        {
            // Query: Get all users
            var users = await _userManager.Users.ToListAsync();

            // Query: Get all role assignments
            var userRoles = await ( from userRole in context.UserRoles
                                    join role in context.Roles
                                        on userRole.RoleId equals role.Id
                                    select new
                                    {
                                        userRole.UserId,
                                        RoleName = role.Name
                                    } ).ToListAsync();

            // Query: Get all notes 
            var notes = await context.UserNote.ToListAsync();

            // Combine everything in memory and removing n+1 queries 
            var result = users.Select(user => new UserRoleViewModel
            {
                UserId = user.Id,
                UserName = user.UserName,
                UserEmail = user.Email,
                HourlyRate = user.HourlyRate,
                HourlyRateInLek = user.HourlyRate.HasValue ? user.HourlyRate.Value * storedValue : null,
                WeeksToDueDateDefault = user.WeeksToDueDateDefault,
                Roles = userRoles.Where(ur => ur.UserId == user.Id)
                                .Select(ur => ur.RoleName)
                                .ToList(),
                SelectedRole = userRoles.FirstOrDefault(ur => ur.UserId == user.Id)?.RoleName ?? "",
                Notes = notes.Where(n => n.TargetUserId == user.Id)
                            .ToDictionary(n => n.TargetUserId, n => n)

            }).ToList();

            return result;
        }
    }
    // Fetch a single user by userId
    public async Task<ApplicationUser> GetUserByIdAsync(string userId)
    {
        using (var context = _contextFactory.CreateDbContext())
        {

            var user = await _userManager.Users
                                      .AsTracking()
                                      .Where(u => u.Id == userId)
                                      .FirstOrDefaultAsync();

            return user;
        }
    }
    // Delete current user and remove his roles, then reassign all of his projects to the admin which will be selected from UI
    public async Task<bool> DeleteUserAsync(string userId, string AdminId)
    {
        using (var context = _contextFactory.CreateDbContext())
        {
            // Check if the new client exists
            var newClient = await context.Users.FindAsync(AdminId);
            if (newClient == null)
            {
                _logger.LogError("The new client (admin) does not exist.");
                return false;
            }

            // Fetch projects where the user is the client
            var projectsAsClient = await context.Projects
                .AsTracking()
                .Where(p => p.ClientId == userId || p.PrimaryEditorId == userId || p.SecondaryEditorId == userId)
                .ToListAsync();

            // Reassign projects to the new client
            foreach (var project in projectsAsClient)
            {
                if (project.ClientId == userId)
                {
                    // Update ClientId to AdminId
                    project.ClientId = AdminId;

                    // Update the ExternalOrder for the new client
                    var highestOrderForClient = await context.Projects
                        .Where(p => p.ClientId == AdminId && !p.IsArchived)
                        .MaxAsync(p =>  p.ExternalOrder) ?? 0;

                    project.ExternalOrder = highestOrderForClient + 1;
                }
                if (project.PrimaryEditorId == userId)
                {
                    // Update PrimaryEditorId to AdminId
                    project.PrimaryEditorId = AdminId;
                }
                if (project.SecondaryEditorId == userId)
                {
                    // Update SecondaryEditorId to AdminId
                    project.SecondaryEditorId = AdminId;
                }

            }

            // Remove the user and his related notes and role
            var user = await context.Users.FindAsync(userId);
            if (user != null)
            {
                context.Users.Remove(user);
            }
            else
            {
                _logger.LogError("User not found.");
                return false;
            }

            // Save changes
            await context.SaveChangesAsync();
            return true;
        }
    }
    // Get all users who have admin roles
    public async Task<List<ApplicationUser>> GetAllAdminsAsync()
    {
        // Get the role by name
        var adminRole = await _roleManager.FindByNameAsync("Admin");
        if (adminRole == null)
        {
            throw new InvalidOperationException("Role 'Admin' does not exist.");
        }

        // Get all users in the role
        var admins = await _userManager.GetUsersInRoleAsync("Admin");
        return admins.ToList();
    }
    public async Task ChangeUserRoleAsync(string userId, string newRole)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                var currentRoles = await _userManager.GetRolesAsync(user);

                // Remove existing roles if they exist
                if (currentRoles.Count > 0)
                {
                    var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
                    if (!removeResult.Succeeded)
                    {
                        throw new Exception($"Failed to remove user roles: {string.Join(", ", removeResult.Errors.Select(e => e.Description))}");
                    }
                }

                // Add new role
                var addResult = await _userManager.AddToRoleAsync(user, newRole);
                if (!addResult.Succeeded)
                {
                    throw new Exception($"Failed to add user to role: {string.Join(", ", addResult.Errors.Select(e => e.Description))}");
                }

                await _userManager.UpdateSecurityStampAsync(user);
            }
            else
            {
                throw new Exception($"User with ID {userId} not found");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to change user role for user {userId} to {newRole}");
            throw; // Rethrow the exception so the caller can handle it
        }
    }

    public async Task<string> UpdateUser(string userId, UserRoleViewModel _user, string newUsername)
    {
        try
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                // Get user by ID
                var user = await GetUserByIdAsync(userId);
                if (user != null)
                {
                    // Update user fields
                    user.WeeksToDueDateDefault = _user.WeeksToDueDateDefault;
                    user.HourlyRate = _user.HourlyRate;
                    // Update the user in the database
                    await _userManager.UpdateAsync(user);
                    await context.SaveChangesAsync();
                    // Check if new username is provided and if it's already taken
                    if (!string.IsNullOrEmpty(newUsername) && user.UserName != newUsername)
                    {
                        var userWithNewUsername = await _userManager.FindByNameAsync(newUsername);
                        if (userWithNewUsername != null && userWithNewUsername.Id != user.Id)
                        {
                            return "Username is already taken!"; // Return error message
                        }

                        // Update username
                        var result = await _userManager.SetUserNameAsync(user, newUsername);
                        if (result.Succeeded)
                        {
                            user.UserName = newUsername;
                            user.NormalizedUserName = newUsername.ToUpperInvariant();
                            await _userManager.UpdateNormalizedUserNameAsync(user);

                            return "User details updated successfully!"; // Return success message
                        }
                        else
                        {
                            return "There is an error, please contact us."; // Return error
                        }
                    }
                    return "User details updated successfully!"; // Return success message
                }
                else
                {
                    // User not found
                    return "User not found."; // Return error message if user doesn't exist
                }


            }
        }
        catch (Exception ex)
        {
            // Log error and return the error message
            _logger.LogError($"Failed to update user {userId}. Error: {ex.Message}");
            return $"Error: {ex.Message}"; // Return error message
        }
    }


    // Method to get all users with the "Editor" role and their associated projects
    public async Task<List<UserProjectViewModel>> GetEditorsWithProjectsAsync()
    {
        using (var context = _contextFactory.CreateDbContext())
        {

            var editors = await _userManager.GetUsersInRoleAsync("Editor");
            var result = await context.Users
                .Where(u => editors.Select(e => e.Id).Contains(u.Id))
                .Select(u => new UserProjectViewModel
                {
                    UserId = u.Id,
                    UserName = u.UserName,
                    HourlyRate = u.HourlyRate,
                    UserRole = "Editor",
                })
                .ToListAsync();

            return result;
        }
    }

    // Method to get all users with the "Client" role and their associated projects
    public async Task<List<UserProjectViewModel>> GetClientsWithProjectsAsync()
    {
        using (var context = _contextFactory.CreateDbContext())
        {

            var clients = await _userManager.GetUsersInRoleAsync("Client");

            var result = await context.Users
                .Where(u => clients.Select(c => c.Id).Contains(u.Id))
                .Select(u => new UserProjectViewModel
                {
                    UserId = u.Id,
                    UserName = u.UserName,
                    HourlyRate = u.HourlyRate,
                    UserRole = "Client",
                })
                .ToListAsync();

            return result;
        }
    }
    public async Task<List<ApplicationUser>> GetAllUsersAssociatedWithProjectAsync(Project project)
    {
        var AllUsers = await GetAllAdminsAsync();
        using (var context = _contextFactory.CreateDbContext())
        {
            var _project = await context.Projects
                .Include(p => p.PrimaryEditor)
                .Include(p => p.SecondaryEditor)
                .FirstOrDefaultAsync(p => p.ProjectId == project.ProjectId);

            if (_project != null)
            {
                if (_project.PrimaryEditorId != null)
                {
                    var primaryEditor = await _userManager.FindByIdAsync(_project.PrimaryEditorId);
                    if (primaryEditor != null)
                    {
                        AllUsers.Add(primaryEditor);
                    }
                }
                if (_project.SecondaryEditorId != null)
                {
                    var secondaryEditor = await _userManager.FindByIdAsync(_project.SecondaryEditorId);
                    if (secondaryEditor != null)
                    {
                        AllUsers.Add(secondaryEditor);
                    }
                }
                if (_project.ClientId != null)
                {
                    var client = await _userManager.FindByIdAsync(_project.ClientId);
                    if (client != null)
                    {
                        AllUsers.Add(client);
                    }
                }
            }
            return AllUsers;
        }
    }

    public async Task<List<string>> GetAssociatedEmailsAsync(string userId)
    {
        using (var context = _contextFactory.CreateDbContext())
        {
            var emails = await context.UserLogins
            .Where(ul => ul.UserId == userId)
            .Join(
                context.MigratedUsers,
                userLogin => userLogin.ProviderKey,
                migratedUser => migratedUser.GoogleProviderKey,
                (userLogin, migratedUser) => migratedUser.Email
            )
            .ToListAsync();

            return emails;
        }
    }
}

public class UserRoleViewModel
{
    public string UserId { get; set; }
    public string UserName { get; set; }
    public string UserEmail { get; set; }
    public List<string> Roles { get; set; }
    public string SelectedRole { get; set; }
    public Dictionary<string, UserNote> Notes { get; set; }
    public string Note
    {
        get => Notes.ContainsKey(UserId) ? Notes[UserId]?.Note ?? string.Empty : string.Empty;
        set
        {
            if (Notes.ContainsKey(UserId))
            {
                Notes[UserId].Note = value;
            }
        }
    }
    public decimal? HourlyRate { get; set; }
    public decimal? HourlyRateInLek { get; set; }
    public int? WeeksToDueDateDefault { get; set; } = 8;
    public string GetNoteValue(string targetUserId)
    {
        // Check if the targetUserId is null or empty
        if (string.IsNullOrEmpty(targetUserId))
        {
            return string.Empty;
        }

        // Try to get the note from the dictionary
        return Notes.TryGetValue(targetUserId, out var note) ? note?.Note ?? string.Empty : string.Empty;
    }
    public static string StripHtmlTags(string input)
    {
        return Regex.Replace(input, "<.*?>", string.Empty);
    }
    public class UserProjectViewModel
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string UserRole { get; set; }
        public bool IsSelected { get; set; }
        public decimal? HourlyRate { get; set; }
        public List<Project> Projects { get; set; }
    }
}