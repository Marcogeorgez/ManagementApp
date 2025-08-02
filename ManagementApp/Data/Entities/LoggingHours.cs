using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ManagementApp.Data.Entities;

public class EditorLoggingHours
{
    [Key]
    public int Id { get; set; }

    [Required]
    [ForeignKey("User")]
    public string UserId { get; set; }

    [Required]
    [ForeignKey("Project")]
    public int ProjectId { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Date")]
    [Required]
    public DateTime Date { get; set; }

    public bool AddedByAdmin { get; set; } = false;
    public string? AdminId { get; set; }
    public decimal EditorWorkingHours { get; set; } = 0;
    public virtual ApplicationUser User { get; set; }
    public virtual Project Project { get; set; }

    [NotMapped]
    public string FormattedDate => Date.ToString("MM-dd-yyyy");

}
