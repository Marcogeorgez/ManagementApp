using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace LuminaryVisuals.Data.Entities;

// Add profile data for application users by adding properties to the ApplicationUser class

public class Archive
{
    [Key]
    public int ArchiveId { get; set; }

    [Required]
    [ForeignKey("Project")]
    public int ProjectId { get; set; }

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Archive Date")]
    public DateTime ArchiveDate { get; set; } = DateTime.UtcNow;

    [DataType(DataType.MultilineText)]
    public string Reason { get; set; }
    public Project Project { get; set; }
}