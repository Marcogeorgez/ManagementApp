namespace LuminaryVisuals.Models
{
    public class ColumnDefinition
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public bool isHidden { get; set; } = false;
    }
    public class ColumnPreferenceResult
    {
        public string PresetName { get; set; }
        public Dictionary<string, bool> Visibility { get; set; }
    }
}
