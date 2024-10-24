using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Luminary.Data.Entities;

public class Chat
{
    [Key]
    public int ChatId { get; set; }

    [Required]
    [ForeignKey("User")]
    public int UserId { get; set; }

    [Required]
    [ForeignKey("Project")]
    public int ProjectId { get; set; }

    [Required]
    [DataType(DataType.MultilineText)]
    public string Message { get; set; }

    [Required]
    [DataType(DataType.DateTime)]
    public DateTime Timestamp { get; set; }

    [Display(Name = "Editor Message")]
    public bool IsEditorMessage { get; set; }

    [Display(Name = "Approved")]
    public bool IsApproved { get; set; }

    // Navigation properties
    public virtual Project Project { get; set; }
    public virtual ApplicationUser User { get; set; }
}
