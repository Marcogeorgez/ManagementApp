using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Luminary.Data.Entities;

public class VideoStatus
{
    [Key]
    public int StatusId { get; set; }

    [Required]
    [ForeignKey("Project")]
    public int ProjectId { get; set; }

    [Required]
    [ForeignKey("User")]
    public string UserId { get; set; }

    [Required]
    [StringLength(50)]
    [RegularExpression(@"^(Editor|Client)$",
        ErrorMessage = "Status Type must be either 'Editor' or 'Client'")]
    public string StatusType { get; set; }

    // Navigation properties
    public virtual Project Project { get; set; }
    public virtual ApplicationUser User { get; set; }
}