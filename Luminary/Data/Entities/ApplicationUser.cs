using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Luminary.Data.Entities;

// Add profile data for application users by adding properties to the ApplicationUser class

public class ApplicationUser : IdentityUser
{
    [Key]
    public int UserId { get; set; }

    [Required]
    [StringLength(255)]
    public required string GoogleId { get; set; }

    [Required]
    [StringLength(50)]
    [Display(Name = "Username")]
    public required string Username { get; set; }

    [Required]
    [EmailAddress]
    [StringLength(50)]
    public required string Email { get; set; }

    [Required]
    [ForeignKey("Role")]
    public int RoleId { get; set; }

    // Navigation properties
    public virtual Role Role { get; set; }
    public virtual ICollection<VideoEditor> VideoEditors { get; set; }
    public virtual ICollection<VideoStatus> VideoStatuses { get; set; }
    public virtual ICollection<Payment> Payments { get; set; }
    public virtual ICollection<Chat> Chats { get; set; }
}
