using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Luminary.Data.Entities;

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
    public DateTime ArchiveDate { get; set; }

    [DataType(DataType.MultilineText)]
    public string Reason { get; set; }

    // Navigation property
    public virtual Project Project { get; set; }
}