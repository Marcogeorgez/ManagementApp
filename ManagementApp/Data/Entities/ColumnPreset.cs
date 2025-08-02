namespace ManagementApp.Data.Entities;

public class ColumnPreset
{
    public int Id { get; set; }
    public string UserId { get; set; }
    public string Name { get; set; }
    public string Preferences { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastUsedAt { get; set; }

    public virtual ApplicationUser User { get; set; }
}
