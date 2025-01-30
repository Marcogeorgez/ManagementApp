using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace LuminaryVisuals.Data.Entities;

public class UserProjectPin
{
    public string UserId { get; set; }      // Foreign key to User
    public int ProjectId { get; set; }      // Foreign key to Project
    public bool IsPinned { get; set; }      // Pinned state for the user

    // Navigation properties
    public ApplicationUser User { get; set; }
    public Project Project { get; set; }
}
