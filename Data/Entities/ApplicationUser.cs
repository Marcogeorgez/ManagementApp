using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LuminaryVisuals.Data.Entities;

public class ApplicationUser : IdentityUser
{
    public decimal? HourlyRate { get; set; }  
    public int? WeeksToDueDateDefault { get; set; }
    // Navigation properties
    public virtual ICollection<ClientPayment> Payments { get; set; }
    public virtual ICollection<Chat> Chats { get; set; }
    public virtual ICollection<EditorPayments> EditorPayments { get; set; }
    public virtual ICollection<UserNote> CreatedNotes { get; set; } 

}
