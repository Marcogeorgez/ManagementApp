namespace LuminaryVisuals.Data.Entities
{
    public enum ProjectStatus
    {
        Upcoming = 0,
        Ready_To_Edit = 1,
        Scheduled = 2,
        Working = 3,
        Review = 4,
        Delivered = 5,
        Revision = 6,
        Finished = 7
    }

    public enum AdminProjectStatus
    {
        Not_Finished = 0,
        Delivered_Not_Paid = 1,
        Sent_Invoice = 2,
        Paid = 3,
    }
}
