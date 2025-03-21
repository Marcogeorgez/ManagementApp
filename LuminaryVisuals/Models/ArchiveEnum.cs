namespace LuminaryVisuals.Models;

public static class ArchiveHelper
{
    public static string GetDisplayName(ArchiveReason reason)
    {
        return reason switch
        {
            ArchiveReason.Paid => "Paid",
            ArchiveReason.Cancelled => "Cancelled",
            ArchiveReason.Deleted => "Deleted",
            _ => "Unknown"
        };

    }
}
public enum ArchiveReason
{
    Paid = 0,
    Cancelled = 1,
    Deleted = 2
}
