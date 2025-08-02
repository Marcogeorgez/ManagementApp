using System.ComponentModel.DataAnnotations;

namespace ManagementApp.Data.Entities;

public enum ProjectStatus
{
    Upcoming = 0,
    [Display(Description = "Ready To Edit")]
    Ready_To_Edit = 1,
    Scheduled = 2,
    Working = 3,
    [Display(Description = "Ready To Review")]
    Ready_To_Review = 4,
    Delivered = 5,
    Revision = 6,
    Finished = 7
}

public enum AdminProjectStatus
{
    [Display(Description = "Not Finished")]
    Not_Finished = 0,
    [Display(Description = "Delivered Not Paid")]
    Delivered_Not_Paid = 1,
    [Display(Description = "Send Invoice")]
    Sent_Invoice = 2,
    Paid = 3,
}
