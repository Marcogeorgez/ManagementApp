using Microsoft.AspNetCore.Identity;

namespace ManagementApp.Data.Entities;

public class ApplicationUser : IdentityUser
{
    public decimal? HourlyRate { get; set; }
    public int? WeeksToDueDateDefault { get; set; } = 8;

    // Navigation properties
    public virtual PayoneerSettings PayoneerSettings { get; set; }
    public virtual ICollection<Chat> Chats { get; set; }
    public virtual ICollection<UserNote> CreatedNotes { get; set; }
    public List<UserChatPin> PinnedProjects { get; set; }

}
