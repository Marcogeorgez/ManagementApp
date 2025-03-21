using System.ComponentModel.DataAnnotations;

namespace LuminaryVisuals.Data.Entities;

public class BlockedWord
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Word { get; set; } = string.Empty;
}
