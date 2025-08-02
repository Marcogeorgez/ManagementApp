using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ManagementApp.Data.Entities;


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