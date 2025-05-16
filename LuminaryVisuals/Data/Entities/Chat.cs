using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LuminaryVisuals.Data.Entities;

public class Chat
{
    [Key]
    public int ChatId { get; set; }

    [ForeignKey("Project")]
    public int? ProjectId { get; set; } // nullable for non-project chats


    public bool IsAdminChat { get; set; } = false; // by default it's false for project chats, otherwise it's true

    public string? UserId { get; set; } // nullable for project chats but required for non-project chats to get chats for user with admin (1-1)

    public ICollection<Message> Messages { get; set; }


    // Navigation properties

    public virtual Project? Project { get; set; }
    public virtual ApplicationUser? User { get; set; }
}


