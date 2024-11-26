using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace LuminaryVisuals.Data.Entities;

public enum ProjectStatus
{
    Upcoming = 0,
    Ready_To_Edit = 1,
    Scheduled = 2,
    Working = 3,
    Review = 4,
    Delivered = 5,
    Revision = 6,
    Finished = 7
}

public enum AdminProjectStatus
{
    Not_Finished = 0,
    Delivered_Not_Paid = 1,
    Sent_Invoice = 2,
    Paid = 3,
}
public class Project
{
    [Key]
    public int ProjectId { get; set; }

    [Required]
    public string ClientId { get; set; }
    public string? PrimaryEditorId { get; set; }
    public string? SecondaryEditorId { get; set; }

    [Required]
    [StringLength(255)]
    [Display(Name = "Project Name")]
    public string ProjectName { get; set; }

    [Required]
    [DataType(DataType.MultilineText)]
    public string Description { get; set; }

    [DataType(DataType.MultilineText)]
    public string? NotesForProject { get; set; }

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Shoot Date")]
    public DateTime? ShootDate { get; set; }

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Due Date")]
    public DateTime? DueDate { get; set; }

    [Range(0, 100)]
    [Display(Name = "Progress")]
    public int ProgressBar { get; set; }
    public int? InternalOrder { get; set; }
    public int? ExternalOrder { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Working Month")]
    public DateTime? WorkingMonth { get; set; }
    public decimal? BillableHours { get; set; }
    public decimal? ClientBillableHours { get; set; }
    public decimal? ClientBillableAmount { get; set; }
    public bool isPaymentVisible { get; set; } = false;

    [Column(TypeName = "decimal(5,2)")]
    public decimal? EditorOvertime { get; set; }
    public decimal? EditorFinalBillable { get; set; }
    public decimal? EditorPaymentAmount { get; set; }

    public bool isEditorPaid { get; set; } = false;
    [Required]
    public bool IsArchived { get; set; } = false;

    public ProjectStatus Status { get; set; }
    public AdminProjectStatus AdminStatus { get; set; }








    // Navigation properties
    [ForeignKey("ClientId")]
    public virtual ApplicationUser Client { get; set; }
    [ForeignKey("PrimaryEditorId")]
    public virtual ApplicationUser? PrimaryEditor { get; set; }
    [ForeignKey("SecondaryEditorId")]
    public virtual ApplicationUser? SecondaryEditor { get; set; }
    public virtual ICollection<Chat> Chats { get; set; }
    public virtual Archive Archive { get; set; }









    [NotMapped]
    public string FormattedShootDate => ShootDate?.ToString("dd/MM/yyyy");

    [NotMapped]
    public string FormattedDueDate => DueDate?.ToString("dd/MM/yyyy");

    [NotMapped]
    public string FormattedWorkingMonth => WorkingMonth?.ToString("MMMM");
    [NotMapped]
    public string FormatStatus => Status.ToString().Replace("_", " ");
    [NotMapped]
    public string FormatAdminStatus => AdminStatus.ToString().Replace("_", " ");
    [NotMapped]
    public string ClientName { get; set; }
    [NotMapped]
    public string PrimaryEditorName { get; set; }
    [NotMapped]
    public string SecondaryEditorName { get; set; }
}