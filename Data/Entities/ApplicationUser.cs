using Microsoft.AspNetCore.Identity;
using System;

namespace LuminaryVisuals.Data.Entities
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUser : IdentityUser
    {

        // Navigation properties
        public virtual ICollection<VideoEditor> VideoEditors { get; set; }
        public virtual ICollection<VideoStatus> VideoStatuses { get; set; }
        public virtual ICollection<ClientPayment> Payments { get; set; }
        public virtual ICollection<Chat> Chats { get; set; }
        public virtual ICollection<EditorPayments> EditorPayments { get; set; }

    }

}
