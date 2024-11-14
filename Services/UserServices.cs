using LuminaryVisuals.Data;
using LuminaryVisuals.Data.Entities;
using LuminaryVisuals.Migrations;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using static LuminaryVisuals.Services.UserRoleViewModel;

namespace LuminaryVisuals.Services;

public class UserServices
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<UserServices> _logger;
    private readonly ApplicationDbContext _context;
    public UserServices(
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext context,
        ILogger<UserServices> logger)
    {
        _context = context;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<List<UserRoleViewModel>> GetAllUsersAsync(decimal? storedValue)
    {
        // Query: Get all users
        var users = await _userManager.Users.ToListAsync();

        // Query: Get all role assignments
        var userRoles = await ( from userRole in _context.UserRoles
                                join role in _context.Roles
                                    on userRole.RoleId equals role.Id
                                select new
                                {
                                    UserId = userRole.UserId,
                                    RoleName = role.Name
                                } ).ToListAsync();

        // Query: Get all notes 
        var notes = await _context.UserNote.ToListAsync();

        // Combine everything in memory and removing n+1 queries 
        var result = users.Select(user => new UserRoleViewModel
        {
            UserId = user.Id,
            UserName = user.UserName,
            HourlyRate = user.HourlyRate,
            HourlyRateInLek = user.HourlyRate.HasValue ? user.HourlyRate.Value * storedValue : (decimal?) null,
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
    public async Task<ApplicationUser> GetUserByIdAsync(string userId)
    {
        // Query: Get a single user by userId
        var user = await _userManager.Users
                                      .Where(u => u.Id == userId)
                                      .FirstOrDefaultAsync();

        return user; // Return the user object or null if not found
    }

    public async Task<bool> ChangeUserRoleAsync(string userId, string newRole)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                var currentRoles = await _userManager.GetRolesAsync(user);
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
                await _userManager.AddToRoleAsync(user, newRole);
                return true;
            }
            return false;
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
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                user.HourlyRate = newHourlyRate;
                await _userManager.UpdateAsync(user);
                return true;
            }
            return false;
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
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                user.WeeksToDueDateDefault = WeeksToDueDateDefault;
                await _userManager.UpdateAsync(user);
                return true;
            }
            return false;
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
        var editors = await _userManager.GetUsersInRoleAsync("Editor");
        var result = await _context.Users
            .Where(u => editors.Select(e => e.Id).Contains(u.Id))
            .Include(u => u.VideoEditors)
                .ThenInclude(ve => ve.Project)
            .Select(u => new UserProjectViewModel
            {
                UserId = u.Id,
                UserName = u.UserName,
                HourlyRate = u.HourlyRate,
                UserRole = "Editor",
                Projects = u.VideoEditors.Select(ve => new Project
                {
                    ProjectId = ve.Project.ProjectId,
                    ProjectName = ve.Project.ProjectName,
                    Description = ve.Project.Description,
                    ShootDate = ve.Project.ShootDate,
                    DueDate = ve.Project.DueDate,
                    ProgressBar = ve.Project.ProgressBar,
                    Status = ve.Project.Status
                }).ToList()
            })
            .ToListAsync();

        return result;
    }

    // Fetches users with "Client" role and their associated projects
    public async Task<List<UserProjectViewModel>> GetClientsWithProjectsAsync()
    {
        var clients = await _userManager.GetUsersInRoleAsync("Client");

        var result = await _context.Users
            .Where(u => clients.Select(c => c.Id).Contains(u.Id))
            .Include(u => u.Payments)
                .ThenInclude(p => p.Project)
            .Select(u => new UserProjectViewModel
            {
                UserId = u.Id,
                UserName = u.UserName,
                HourlyRate = u.HourlyRate,
                UserRole = "Client",
                Projects = u.Payments.Select(p => new Project
                {
                    ProjectId = p.Project.ProjectId,
                    ProjectName = p.Project.ProjectName,
                    Description = p.Project.Description,
                    ShootDate = p.Project.ShootDate,
                    DueDate = p.Project.DueDate,
                    ProgressBar = p.Project.ProgressBar,
                    Status = p.Project.Status
                }).ToList()
            })
            .ToListAsync();

        return result;
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