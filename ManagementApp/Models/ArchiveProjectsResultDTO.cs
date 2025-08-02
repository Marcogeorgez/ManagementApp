namespace ManagementApp.Models;

public class ArchiveProjectsResult
{
    public List<int> ArchivedProjects { get; set; } = [];
    public ArchiveReason? ArchiveReason { get; set; }
}
