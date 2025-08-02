using System.ComponentModel.DataAnnotations;

namespace ManagementApp.Data.Entities;

public class UserChatPin
{
    [Key]
    public int Id { get; set; } // Primary key
    public string UserId { get; set; }      // Foreign key to User
    public int? ProjectId { get; set; }   // Foreign key to Project
    // So we can have a pinned project in a private chat
    // This is a nullable string to allow for null values and if its null its a project chat
    public string? UserChatId { get; set; }      // Foreign key to private chats

    public bool IsPinned { get; set; }      // Pinned state for the user

    // Navigation properties
    public ApplicationUser User { get; set; }
    public Project? Project { get; set; }
}
