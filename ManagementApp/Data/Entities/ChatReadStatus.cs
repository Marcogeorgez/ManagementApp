using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ManagementApp.Data.Entities;

public class ChatReadStatus
{
    [Key]
    public int Id { get; set; }
    [Required]
    [ForeignKey("Message")]
    public int MessageId { get; set; }

    [Required]
    [ForeignKey("User")]
    public string UserId { get; set; }

    [Required]
    public bool IsRead { get; set; }

    [Required]
    public DateTime ReadTimestamp { get; set; }

    // Navigation properties
    public virtual Message Message { get; set; }
    public virtual ApplicationUser User { get; set; }
}
