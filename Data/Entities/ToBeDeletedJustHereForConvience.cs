
// Leaving this file for now just so I can read it in one place and change as needed.

/*
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Luminary.Data;

// Add profile data for application users by adding properties to the ApplicationUser class

public class ApplicationUser : IdentityUser
{
    [Key]
    public int UserId { get; set; }

    [Required]
    [StringLength(255)]
    public required string GoogleId { get; set; }

    [Required]
    [StringLength(50)]
    [Display(Name = "Username")]
    public required string Username { get; set; }

    [Required]
    [EmailAddress]
    [StringLength(50)]
    public required string Email { get; set; }

    [Required]
    [ForeignKey("Role")]
    public int RoleId { get; set; }

    // Navigation properties
    public virtual Role Role { get; set; }
    public virtual ICollection<VideoEditor> VideoEditors { get; set; }
    public virtual ICollection<VideoStatus> VideoStatuses { get; set; }
    public virtual ICollection<Payment> Payments { get; set; }
    public virtual ICollection<Chat> Chats { get; set; }
}

public class Role
{
    [Key]
    public int RoleId { get; set; }

    [Required]
    [StringLength(15)]
    [Display(Name = "Role Name")]
    public string RoleName { get; set; } = "Guest";

    // Navigation property
    public virtual ICollection<ApplicationUser> Users { get; set; }
}

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
    public virtual ICollection<Payment> Payments { get; set; }
    public virtual ICollection<Chat> Chats { get; set; }
    public virtual Archive Archive { get; set; }
}

public class VideoEditor
{
    [Key]
    public int VideoEditorId { get; set; }

    [Required]
    [ForeignKey("Project")]
    public int ProjectId { get; set; }

    [Required]
    [ForeignKey("User")]
    public int UserId { get; set; }

    [Required]
    [StringLength(50)]
    [RegularExpression(@"^(Editor|Assistant Editor)$",
        ErrorMessage = "Label must be either 'Editor' or 'Assistant Editor'")]
    public string Label { get; set; }

    // Navigation properties
    public virtual Role Role { get; set; }
    public virtual Project Project { get; set; }
    public virtual ApplicationUser User { get; set; }
}

public class VideoStatus
{
    [Key]
    public int StatusId { get; set; }

    [Required]
    [ForeignKey("Project")]
    public int ProjectId { get; set; }

    [Required]
    [ForeignKey("User")]
    public int UserId { get; set; }

    [Required]
    [StringLength(50)]
    [RegularExpression(@"^(Editor|Client)$",
        ErrorMessage = "Status Type must be either 'Editor' or 'Client'")]
    public string StatusType { get; set; }

    // Navigation properties
    public virtual Project Project { get; set; }
    public virtual ApplicationUser User { get; set; }
}

public class Payment
{
    [Key]
    public int PaymentId { get; set; }

    [Required]
    [ForeignKey("User")]
    public int UserId { get; set; }

    [Required]
    [ForeignKey("Project")]
    public int ProjectId { get; set; }

    [Required]
    [Range(0, 999.99)]
    [Column(TypeName = "decimal(5,2)")]
    [Display(Name = "Working Hours")]
    public decimal WorkingHours { get; set; }

    [Range(0, 999.99)]
    [Column(TypeName = "decimal(5,2)")]
    public decimal Overtime { get; set; }

    [Range(0, 999.99)]
    [Column(TypeName = "decimal(5,2)")]
    public decimal Undertime { get; set; }

    [Required]
    [Range(0, 9999999.99)]
    [Column(TypeName = "decimal(10,2)")]
    [Display(Name = "Editor Payment")]
    public decimal EditorPayment { get; set; }

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Date Paid")]
    public DateTime EditorDatePaid { get; set; }

    [Required]
    [Range(0, 999.99)]
    [Column(TypeName = "decimal(5,2)")]
    [Display(Name = "Billable Hours")]
    public decimal BillableHours { get; set; }

    [Required]
    [Range(0, 9999999.99)]
    [Column(TypeName = "decimal(10,2)")]
    public decimal Price { get; set; }

    [Display(Name = "Visible to Client")]
    public bool ClientVisible { get; set; }

    [Required]
    [StringLength(50)]
    [RegularExpression(@"^(Pending|Completed|Failed)$",
        ErrorMessage = "Payment Status must be either 'Pending', 'Completed', or 'Failed'")]
    [Display(Name = "Payment Status")]
    public string ClientPaymentStatus { get; set; }

    // Navigation properties
    public virtual Project Project { get; set; }
    public virtual ApplicationUser User { get; set; }
}

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
*/