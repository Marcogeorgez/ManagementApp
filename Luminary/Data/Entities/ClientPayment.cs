using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Luminary.Data.Entities;

public class ClientPayment
{
    [Key]
    public int PaymentId { get; set; }

    [Required]
    [ForeignKey("Project")]
    public int ProjectId { get; set; }

    [Required]
    [Range(0, 999.99)]
    [Column(TypeName = "decimal(5,2)")]
    [Display(Name = "Billable Hours")]
    public decimal BillableHours { get; set; }

    [Required]
    [Range(0, 9999999.99)]
    [Column(TypeName = "decimal(10,2)")]
    public decimal ProjectTotalPrice { get; set; }

    [Display(Name = "Visible to Client")]
    public bool PaymentVisibleClient { get; set; }

    [Required]
    [StringLength(50)]
    [RegularExpression(@"^(Pending|Completed)$",
        ErrorMessage = "Payment Status must be either 'Pending', 'Completed'")]
    [Display(Name = "Payment Status")]
    public string ClientPaymentStatus { get; set; }

    // Navigation properties
    public virtual Project Project { get; set; }
}