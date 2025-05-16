namespace LuminaryVisuals.Data.Entities;

public class CalculationParameter
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string ParameterType { get; set; }
    public decimal DefaultValue { get; set; }
    public List<CalculationOption> Options { get; set; } = [];
}

public class CalculationOption
{
    public int Id { get; set; }
    public int CalculationParameterId { get; set; }
    public string OptionName { get; set; }
    public decimal Multiplier { get; set; }
}
