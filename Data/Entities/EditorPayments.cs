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

    [Required]
    [Range(0, 999.99)]
    [Column(TypeName = "decimal(5,2)")]
    [Display(Name = "Working Hours")]
    public decimal WorkingHours { get; set; }

    [Range(0, 999.99)]
    [Column(TypeName = "decimal(5,2)")]
    public decimal Overtime { get; set; }

    [Range(0, 999.99)]
    [Column(TypeName = "decimal(5,2)")]
    public decimal Undertime { get; set; }

    [Required]
    [Range(0, 9999999.99)]
    [Column(TypeName = "decimal(10,2)")]
    [Display(Name = "Editor Payment Amount")]
    public decimal PaymentAmount { get; set; }

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Date Paid")]
    public DateTime EditorDatePaid { get; set; }

    [Required]
    [Range(1, 12)]
    [Display(Name = "Payment Month")]
    public int PaymentMonth { get; set; }

    [Required]
    [Range(2000, 2100)]
    [Display(Name = "Payment Year")]
    public int PaymentYear { get; set; }

    [Required]
    [Range(0, 999.99)]
    [Column(TypeName = "decimal(5,2)")]
    [Display(Name = "Billable Hours")]
    public decimal BillableHours { get; set; }

    // Navigation properties
    public virtual Project Project { get; set; }
    public virtual ApplicationUser User { get; set; }
}