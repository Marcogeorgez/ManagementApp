namespace ManagementApp.Models;

public class ColumnDefinition
{
    public string Name { get; set; }
    public string DisplayName { get; set; }
    public bool isHidden { get; set; } = true;
}
public class ColumnPreferenceResult
{
    public string PresetName { get; set; }
    public Dictionary<string, bool> Visibility { get; set; }
}
