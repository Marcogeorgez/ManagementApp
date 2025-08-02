using System.ComponentModel.DataAnnotations;

namespace ManagementApp.Data.Entities;

public class BlockedWord
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Word { get; set; } = string.Empty;
}
