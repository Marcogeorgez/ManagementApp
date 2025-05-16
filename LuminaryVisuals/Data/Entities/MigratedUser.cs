using System.ComponentModel.DataAnnotations;

namespace LuminaryVisuals.Data.Entities;

public class MigratedUser
{
    [Key]
    public int Id { get; set; }
    public string GoogleProviderKey { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public DateTime MigrationDate { get; set; }
}
