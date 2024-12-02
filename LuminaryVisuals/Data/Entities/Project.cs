using LuminaryVisuals.Data.Entities;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using LuminaryVisuals.Models;

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

    public ProjectSpecifications ProjectSpecifications { get; set; }


    [DataType(DataType.MultilineText)]
    public string? NotesForProject { get; set; }

    [DataType(DataType.MultilineText)]
    public string? link { get; set; }
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

    public decimal? ClientBillableHours { get; set; }
    public decimal? ClientBillableAmount { get; set; }
    public bool isPaymentVisible { get; set; } = false;

    // Billable hours, overtime, and payment details for both editors
    public EditorDetails PrimaryEditorDetails { get; set; }
    public EditorDetails SecondaryEditorDetails { get; set; }

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

    [JsonIgnore]
    public virtual ICollection<Revision> Revisions { get; set; }

    [NotMapped]
    public string FormattedShootDate => ShootDate?.ToString("MM-dd-yyyy");

    [NotMapped]
    public string FormattedDueDate => DueDate?.ToString("MM-dd-yyyy");

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
