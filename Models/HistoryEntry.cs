using System;

namespace CraKit.Models;

public class HistoryEntry
{
    public DateTime Timestamp { get; set; }
    public string ToolName { get; set; } = string.Empty;
    public string Command { get; set; } = string.Empty;
    public string Output { get; set; } = string.Empty;
    public bool Success { get; set; }
    public TimeSpan? ExecutionTime { get; set; }

    public override string ToString()
    {
        var status = Success ? "SUCCESS" : "FAILED";
        var duration = ExecutionTime.HasValue ? " (" + ExecutionTime.Value.TotalSeconds.ToString("F2") + "s)" : "";
        
        return $"[{Timestamp:yyyy-MM-dd HH:mm:ss}] {status}{duration}\n" +
               $"Tool: {ToolName}\n" +
               $"Command: {Command}\n" +
               $"Output:\n{Output}\n" +
               new string('-', 60);
    }
}

