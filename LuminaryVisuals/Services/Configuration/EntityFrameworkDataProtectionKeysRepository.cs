using LuminaryVisuals.Data.Entities;
using LuminaryVisuals.Data;
using Microsoft.AspNetCore.DataProtection.Repositories;
using System.Xml.Linq;

public class EntityFrameworkDataProtectionKeysRepository : IXmlRepository
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<EntityFrameworkDataProtectionKeysRepository> _logger;

    public EntityFrameworkDataProtectionKeysRepository(
        IServiceProvider serviceProvider,
        ILogger<EntityFrameworkDataProtectionKeysRepository> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public IReadOnlyCollection<XElement> GetAllElements()
    {
        _logger.LogInformation("GetAllElements called");
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var keys = context.DataProtectionKeys.ToList();
        _logger.LogInformation($"Found {keys.Count} keys");

        return keys.Select(k => XElement.Parse(k.Xml))
            .ToList()
            .AsReadOnly();
    }

    public void StoreElement(XElement element, string friendlyName)
    {
        _logger.LogInformation($"StoreElement called with friendlyName: {friendlyName}");
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var key = new DataProtectionKey
        {
            Id = Guid.NewGuid().ToString(),
            FriendlyName = friendlyName,
            Xml = element.ToString(SaveOptions.DisableFormatting)
        };

        try
        {
            context.DataProtectionKeys.Add(key);
            context.SaveChanges();
            _logger.LogInformation($"Successfully stored key with ID: {key.Id}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to store key");
            throw;
        }
    }
}