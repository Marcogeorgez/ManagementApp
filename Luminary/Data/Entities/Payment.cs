using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Luminary.Data.Entities;

public class Payment
{
    [Key]
    public int PaymentId { get; set; }

    [Required]
    [ForeignKey("User")]
    public int UserId { get; set; }

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
    [Display(Name = "Editor Payment")]
    public decimal EditorPayment { get; set; }

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Date Paid")]
    public DateTime EditorDatePaid { get; set; }

    [Required]
    [Range(0, 999.99)]
    [Column(TypeName = "decimal(5,2)")]
    [Display(Name = "Billable Hours")]
    public decimal BillableHours { get; set; }

    [Required]
    [Range(0, 9999999.99)]
    [Column(TypeName = "decimal(10,2)")]
    public decimal Price { get; set; }

    [Display(Name = "Visible to Client")]
    public bool ClientVisible { get; set; }

    [Required]
    [StringLength(50)]
    [RegularExpression(@"^(Pending|Completed|Failed)$",
        ErrorMessage = "Payment Status must be either 'Pending', 'Completed', or 'Failed'")]
    [Display(Name = "Payment Status")]
    public string ClientPaymentStatus { get; set; }

    // Navigation properties
    public virtual Project Project { get; set; }
    public virtual ApplicationUser User { get; set; }
}