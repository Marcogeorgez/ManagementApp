using System.ComponentModel.DataAnnotations;

namespace LuminaryVisuals.Data.Entities
{
    public class Message
    {
        [Key]
        public int MessageId { get; set; }  // Primary key for Message
        [Required]
        public int ChatId { get; set; }     // Foreign key to Chat
        [Required]
        public string UserId { get; set; }

        [DataType(DataType.MultilineText)]
        public string Content { get; set; }
        public DateTime Timestamp { get; set; }

        public bool IsApproved { get; set; } = false;
        public bool IsDeleted { get; set; } = false;
        // Navigation property  
        public Chat Chat { get; set; }
        public virtual ApplicationUser User { get; set; }
    }
}
