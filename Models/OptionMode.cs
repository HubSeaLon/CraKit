namespace CraKit.Models;

public class OptionMode
{
    public string value { get; set; } = "";
    public string label { get; set; } = "";

    public override string ToString() => $"{value}";
}