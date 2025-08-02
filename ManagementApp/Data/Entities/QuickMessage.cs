using System.ComponentModel.DataAnnotations;

namespace ManagementApp.Data.Entities;

public class QuickMessage
{
    [Key]
    public int Id { get; set; }
    public string? Content { get; set; }
    public bool Preapproved { get; set; }
}


