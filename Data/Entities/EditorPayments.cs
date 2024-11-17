using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace LuminaryVisuals.Data.Entities;

public class EditorPayments
{
    [Key]
    public int PaymentId { get; set; }

    [Required]
    [ForeignKey("User")]
    public string UserId { get; set; }

    [Required]
    [ForeignKey("Project")]
    public int ProjectId { get; set; }

    [Display(Name = "Working Hours")]
    public decimal WorkingHours { get; set; }

    [Column(TypeName = "decimal(5,2)")]
    public decimal Overtime { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    [Display(Name = "Editor Payment Amount")]
    public decimal PaymentAmount { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Date Paid")]
    public DateTime EditorDatePaid { get; set; }

    public bool isPaid { get; set; }

    // Navigation properties
    public virtual Project Project { get; set; }
    public virtual ApplicationUser User { get; set; }
}