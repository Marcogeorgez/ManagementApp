namespace LuminaryVisuals.Models;

public class ArchiveProjectsResult
{
    public List<int> ArchivedProjects { get; set; } = new();
    public ArchiveReason? ArchiveReason { get; set; }
}
