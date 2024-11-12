using LuminaryVisuals.Data;
using LuminaryVisuals.Data.Entities;
using LuminaryVisuals.Migrations;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

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

    public async Task<List<UserRoleViewModel>> GetAllUsersAsync()
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
            Roles = userRoles.Where(ur => ur.UserId == user.Id)
                            .Select(ur => ur.RoleName)
                            .ToList(),
            SelectedRole = userRoles.FirstOrDefault(ur => ur.UserId == user.Id)?.RoleName ?? "",
            Notes = notes.Where(n => n.TargetUserId == user.Id)
                        .ToDictionary(n => n.TargetUserId, n => n)
        }).ToList();

        return result;
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

}

public class UserRoleViewModel
{
    public string UserId { get; set; }
    public string UserName { get; set; }
    public List<string> Roles { get; set; }
    public string SelectedRole { get; set; }
    public Dictionary<string, UserNote> Notes { get; set; }
    public decimal? HourlyRate { get; set; }
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
}