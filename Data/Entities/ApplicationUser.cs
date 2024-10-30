using Microsoft.AspNetCore.Identity;

namespace LuminaryVisuals.Data.Entities;

public class ApplicationUser : IdentityUser
{

    // Navigation properties
    public virtual ICollection<VideoEditor> VideoEditors { get; set; }
    public virtual ICollection<VideoStatus> VideoStatuses { get; set; }
    public virtual ICollection<ClientPayment> Payments { get; set; }
    public virtual ICollection<Chat> Chats { get; set; }
    public virtual ICollection<EditorPayments> EditorPayments { get; set; }

}
