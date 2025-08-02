using System.ComponentModel.DataAnnotations;

namespace ManagementApp.Data.Entities;

public class PayoneerSettings
{
    [Key]
    public string UserId { get; set; } // Mapped to UsersId 

    [Required]
    [StringLength(255)]
    public string? CompanyName { get; set; }

    [StringLength(255)]
    public string? CompanyUrl { get; set; }

    [Required]
    [StringLength(100)]
    public string? FirstName { get; set; }

    [Required]
    [StringLength(100)]
    public string? LastName { get; set; }

    [Required]
    [EmailAddress]
    [StringLength(255)]
    public string? Email { get; set; }

    [Required]
    [StringLength(3)]
    public string Currency { get; set; } = "USD";
    public string? Address { get; set; }
    public string TaxId { get; set; } = string.Empty;

    // Navigation property
    public virtual ApplicationUser User { get; set; }
}
