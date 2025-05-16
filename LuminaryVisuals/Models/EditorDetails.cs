namespace LuminaryVisuals.Models;

public class EditorDetails
{

    public decimal BillableHours { get; set; } = 0;
    public decimal Overtime { get; set; } = 0;
    public decimal? PaymentAmount { get; set; }
    public decimal AdjustmentHours { get; set; } = 0;
    public decimal FinalBillableHours { get; set; } = 0;
    public DateTime? DatePaidEditor { get; set; } = null;

}
