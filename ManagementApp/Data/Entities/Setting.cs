using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ManagementApp.Data.Entities;

public class Setting
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; }

    [Required]
    public decimal? ConversionRateUSToLek { get; set; }

    [ForeignKey("User")]
    public string UpdatedByUserId { get; set; }

    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    public virtual ApplicationUser User { get; set; }
}
