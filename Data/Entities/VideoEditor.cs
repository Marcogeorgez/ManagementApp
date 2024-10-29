using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace LuminaryVisuals.Data.Entities;

public class VideoEditor
{
    [Key]
    public int VideoEditorId { get; set; }

    [Required]
    [ForeignKey("Project")]
    public int ProjectId { get; set; }

    [Required]
    [ForeignKey("User")]
    public string UserId { get; set; }

    [Required]
    [StringLength(50)]
    [RegularExpression(@"^(Editor|Assistant Editor)$",
        ErrorMessage = "Label must be either 'Editor' or 'Assistant Editor'")]
    public string Label { get; set; }

    // Navigation properties
    public virtual Project Project { get; set; }
    public virtual ApplicationUser User { get; set; }
}
