using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace LuminaryVisuals.Data.Entities;

public class ApplicationUser : IdentityUser
{
    public decimal? HourlyRate { get; set; }
    public int? WeeksToDueDateDefault { get; set; } = 4;

    // Navigation properties

    public virtual ICollection<Chat> Chats { get; set; }
    public virtual ICollection<UserNote> CreatedNotes { get; set; } 

}
