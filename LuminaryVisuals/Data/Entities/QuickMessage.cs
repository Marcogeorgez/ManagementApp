using System.ComponentModel.DataAnnotations;

namespace LuminaryVisuals.Data.Entities;

public class QuickMessage
{
    [Key]
    public int Id { get; set; }
    public string? Content { get; set; }
    public bool Preapproved { get; set; }
}


