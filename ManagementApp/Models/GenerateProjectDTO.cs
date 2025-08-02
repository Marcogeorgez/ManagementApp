namespace ManagementApp.Models;

public class GenerateProjectDTO
{
    public required IEnumerable<Project> project { get; set; }
    public string ViewClient { get; set; } = string.Empty;
    public bool? editorPaid;
}
