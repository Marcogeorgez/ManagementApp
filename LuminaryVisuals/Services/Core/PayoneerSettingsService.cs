using LuminaryVisuals.Data.Entities;
using LuminaryVisuals.Data;
using Microsoft.EntityFrameworkCore;

namespace LuminaryVisuals.Services.Core;

public class PayoneerSettingsService
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

    public PayoneerSettingsService(IDbContextFactory<ApplicationDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<Dictionary<string, PayoneerSettings>> GetSettingsForClientsAsync(List<string> clientIds)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.PayoneerSettings
            .Where(ps => clientIds.Contains(ps.UserId))
            .ToDictionaryAsync(ps => ps.UserId);
    }

    public async Task<PayoneerSettings?> GetSettingsForUserAsync(string userId)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.PayoneerSettings
            .FirstOrDefaultAsync(ps => ps.UserId == userId);
    }



    public async Task<PayoneerSettings> CreateSettingsAsync(string userId)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var existingSettings = await context.PayoneerSettings
            .AnyAsync(ps => ps.UserId == userId);

        if (existingSettings)
        {
            throw new InvalidOperationException("Settings already exist for this user.");
        }
        // Create new settings
        var newSettings = new PayoneerSettings
        {
            UserId = userId,
            CompanyName = string.Empty, // Default values
            CompanyUrl = string.Empty,
            FirstName = string.Empty,
            LastName = string.Empty,
            Email = string.Empty,
        };
        context.PayoneerSettings.Add(newSettings);
        await context.SaveChangesAsync();
        return newSettings;
    }
    public async Task<PayoneerSettings> UpdateSettingsAsync(PayoneerSettings settings)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var existingSettings = await context.PayoneerSettings
            .FirstOrDefaultAsync(ps => ps.UserId == settings.UserId);

        if (existingSettings == null)
        {
            // Create new settings
            context.PayoneerSettings.Add(settings);
        }
        else
        {
            // Update existing settings
            existingSettings.CompanyName = settings.CompanyName;
            existingSettings.CompanyUrl = settings.CompanyUrl;
            existingSettings.FirstName = settings.FirstName;
            existingSettings.LastName = settings.LastName;
            existingSettings.Email = settings.Email;
            existingSettings.Currency = settings.Currency;
            existingSettings.Address = settings.Address;
            existingSettings.TaxId = settings.TaxId;    

            context.PayoneerSettings.Update(existingSettings);
        }

        await context.SaveChangesAsync();
        return existingSettings ?? settings;
    }
}
