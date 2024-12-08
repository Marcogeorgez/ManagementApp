using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace LuminaryVisuals.Data.Entities;

public class Chat
{
    [Key]
    public int ChatId { get; set; }

    [Required]
    [ForeignKey("Project")]
    public int ProjectId { get; set; }

    public ICollection<Message> Messages { get; set; }

  
    // Navigation properties

    public virtual Project Project { get; set; }
}


