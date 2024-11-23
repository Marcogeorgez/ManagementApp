using LuminaryVisuals.Data;
using LuminaryVisuals.Data.Entities;
using LuminaryVisuals.Migrations;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using System.Text.RegularExpressions;
using static LuminaryVisuals.Services.UserRoleViewModel;

namespace LuminaryVisuals.Services;

public class UserServices
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<UserServices> _logger;
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
    public UserServices(
        UserManager<ApplicationUser> userManager,
        IDbContextFactory<ApplicationDbContext> context,
        ILogger<UserServices> logger)
    {
        _contextFactory = context;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<List<UserRoleViewModel>> GetAllUsersAsync(decimal? storedValue)
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
                                        UserId = userRole.UserId,
                                        RoleName = role.Name
                                    } ).ToListAsync();

            // Query: Get all notes 
            var notes = await context.UserNote.ToListAsync();

            // Combine everything in memory and removing n+1 queries 
            var result = users.Select(user => new UserRoleViewModel
            {
                UserId = user.Id,
                UserName = user.UserName,
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
                                      .Where(u => u.Id == userId)
                                      .FirstOrDefaultAsync();

            return user;
        }
    }

    public async Task<bool> ChangeUserRoleAsync(string userId, string newRole)
    {
        try
        {
            using (var context = _contextFactory.CreateDbContext())
            {

                var user = await GetUserByIdAsync(userId);
                if (user != null)
                {
                    var currentRoles = await _userManager.GetRolesAsync(user);
                    await _userManager.RemoveFromRolesAsync(user, currentRoles);
                    await _userManager.AddToRoleAsync(user, newRole);
                    return true;
                }
                return false;
            }
        }
        catch
        {
            _logger.LogError($"Failed to change user role!");
            return false;
        }
    }
    public async Task<bool> UpdateHourlyRateAsync(string userId, decimal? newHourlyRate)
    {
        try
        {
            using (var context = _contextFactory.CreateDbContext())
            {

                var user = await GetUserByIdAsync(userId);
                if (user != null)
                {
                    user.HourlyRate = newHourlyRate;
                    await _userManager.UpdateAsync(user);
                    return true;
                }
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to update hourly rate for user {userId}. Error: {ex.Message}");
            return false;
        }
    }
    public async Task<bool> UpdateWeeksToDueDateDefault(string userId, int? WeeksToDueDateDefault)
    {
        try
        {
            using (var context = _contextFactory.CreateDbContext())
            {

                var user = await GetUserByIdAsync(userId);
                if (user != null)
                {
                    user.WeeksToDueDateDefault = WeeksToDueDateDefault;
                    await _userManager.UpdateAsync(user);
                    return true;
                }
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to update Weeks Due Date Default rate for user {userId}. Error: {ex.Message}");
            return false;
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
}

public class UserRoleViewModel
{
    public string UserId { get; set; }
    public string UserName { get; set; }
    public List<string> Roles { get; set; }
    public string SelectedRole { get; set; }
    public Dictionary<string, UserNote> Notes { get; set; }
    public string Note => Notes.ContainsKey(UserId) ? Notes[UserId]?.Note ?? string.Empty : string.Empty;
    public decimal? HourlyRate { get; set; }
    public decimal? HourlyRateInLek { get; set; }
    public int? WeeksToDueDateDefault { get; set; }
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