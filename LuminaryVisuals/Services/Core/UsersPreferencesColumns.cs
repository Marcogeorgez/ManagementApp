using LuminaryVisuals.Data;
using LuminaryVisuals.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace LuminaryVisuals.Services.Core;
public class ColumnPreferenceService
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

    public ColumnPreferenceService(IDbContextFactory<ApplicationDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<List<ColumnPreset>> GetUserPresets(string userId)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.ColumnPresets
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task SavePreset(string userId, string name, Dictionary<string, bool> preferences)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var preset = new ColumnPreset
        {
            UserId = userId,
            Name = name,
            Preferences = JsonSerializer.Serialize(preferences),
            CreatedAt = DateTime.UtcNow,
            LastUsedAt = DateTime.UtcNow
        };

        context.ColumnPresets.Add(preset);
        await context.SaveChangesAsync();
    }

    public async Task<Dictionary<string, bool>> GetPreferencesByName(string userId, string presetName)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var preset = await context.ColumnPresets
            .AsTracking()
            .Where(p => p.UserId == userId && p.Name == presetName)
            .FirstOrDefaultAsync();

        preset.LastUsedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();
        if (preset == null)
            return new Dictionary<string, bool>();

        return JsonSerializer.Deserialize<Dictionary<string, bool>>(preset.Preferences)!;
    }
    public async Task<string> GetLastPresetUsed(string userId)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        // Find the maximum LastUsedAt value for the given user
        var lastUsedAt = await context.ColumnPresets
            .Where(p => p.UserId == userId)
            .MaxAsync(p => p.LastUsedAt);

        // Retrieve the preset with the maximum LastUsedAt
        var preset = await context.ColumnPresets
            .Where(p => p.UserId == userId && p.LastUsedAt == lastUsedAt)
            .FirstOrDefaultAsync();
        return preset.Name!;
    }
}