/*using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Luminary.Data.Entities;

public class Role
{
    [Key]
    public int RoleId { get; set; }

    [Required]
    [StringLength(15)]
    [Display(Name = "Role Name")]
    public string RoleName { get; set; } = "Guest";

    // Navigation property
    public virtual ICollection<ApplicationUser> Users { get; set; }
}*/