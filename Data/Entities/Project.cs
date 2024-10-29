using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace LuminaryVisuals.Data.Entities;

public enum ProjectStatus
{
    Upcoming,
    Scheduled,
    Working,
    Delivered,
    Revision,
    Paid
}

public class Project
{
    [Key]
    public int ProjectId { get; set; }

    [Required]
    [StringLength(255)]
    [Display(Name = "Project Name")]
    public string ProjectName { get; set; }

    [Required]
    [DataType(DataType.MultilineText)]
    public string Description { get; set; }

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Shoot Date")]
    public DateTime ShootDate { get; set; }

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Due Date")]
    public DateTime DueDate { get; set; }

    [Range(0, 100)]
    [Display(Name = "Progress")]
    public int ProgressBar { get; set; }

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Working Month")]
    public DateTime WorkingMonth { get; set; }

    public ProjectStatus Status { get; set; }

    // Navigation properties
    public virtual ICollection<VideoEditor> VideoEditors { get; set; }
    public virtual ICollection<VideoStatus> VideoStatuses { get; set; }
    public virtual ClientPayment ClientPayment { get; set; }
    public virtual ICollection<Chat> Chats { get; set; }
    public virtual Archive Archive { get; set; }
    public virtual ICollection<EditorPayments> EditorPayments { get; set; }
}