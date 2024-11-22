using LuminaryVisuals.Data.Entities;
using LuminaryVisuals.Data;
using Microsoft.EntityFrameworkCore;

namespace LuminaryVisuals.Services
{
    public class SettingService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

        public SettingService(IDbContextFactory<ApplicationDbContext> context)
        {
            _contextFactory = context;
        }

        public async Task<Setting> GetSettingByNameAsync(string name)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                var setting = await context.Settings.FirstOrDefaultAsync(s => s.Name == name);
                if (setting == null)
                {
                    setting = new Setting();
                }
                return setting;
            }
        }

        public async Task UpdateSettingAsync(string name, decimal? ConversionRateUSToLek, string updatedByUserId)
        {
            using (var context = _contextFactory.CreateDbContext())
            { 
                var setting = await context.Settings.FirstOrDefaultAsync(s => s.Name == name);
                if (setting == null)
                {
                    setting = new Setting
                    {
                        Name = name,
                        ConversionRateUSToLek = ConversionRateUSToLek,
                        UpdatedByUserId = updatedByUserId,
                        LastUpdated = DateTime.UtcNow
                    };
                    await context.Settings.AddAsync(setting);
                }
                else
                {
                    setting.ConversionRateUSToLek = ConversionRateUSToLek;
                    setting.UpdatedByUserId = updatedByUserId;
                    setting.LastUpdated = DateTime.UtcNow;
                }

                await context.SaveChangesAsync();
            }
        }
    }
}
