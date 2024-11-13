using LuminaryVisuals.Data.Entities;
using LuminaryVisuals.Data;
using Microsoft.EntityFrameworkCore;

namespace LuminaryVisuals.Services
{
    public class SettingService
    {
        private readonly ApplicationDbContext _context;

        public SettingService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Setting> GetSettingByNameAsync(string name)
        {
            return await _context.Settings.FirstOrDefaultAsync(s => s.Name == name);
        }

        public async Task UpdateSettingAsync(string name, decimal? ConversionRateUSToLek, string updatedByUserId)
        {
            var setting = await _context.Settings.FirstOrDefaultAsync(s => s.Name == name);

            if (setting == null)
            {
                setting = new Setting
                {
                    Name = name,
                    ConversionRateUSToLek = ConversionRateUSToLek,
                    UpdatedByUserId = updatedByUserId,
                    LastUpdated = DateTime.UtcNow
                };
                await _context.Settings.AddAsync(setting);
            }
            else
            {
                setting.ConversionRateUSToLek = ConversionRateUSToLek;
                setting.UpdatedByUserId = updatedByUserId;
                setting.LastUpdated = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }
    }
}
