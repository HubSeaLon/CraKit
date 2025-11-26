namespace CraKit.Models;

public class HashMode
{
    public int value { get; set; }
    public string label { get; set; } = "";

    public override string ToString() => $"{value} - {label}";
}