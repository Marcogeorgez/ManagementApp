using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ManagementApp.Data.Entities;

public class Revision
{
    [Key]
    public int RevisionId { get; set; }

    [Required]
    public int ProjectId { get; set; }

    [Required]
    [DataType(DataType.MultilineText)]
    public string Content { get; set; }

    [Required]
    [DataType(DataType.DateTime)]
    public DateTime RevisionDate { get; set; }

    public bool isCompleted { get; set; } = false;

    [ForeignKey("ProjectId")]
    public virtual Project Project { get; set; }
}
