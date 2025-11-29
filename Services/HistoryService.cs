using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CraKit.Models;

namespace CraKit.Services;

public class HistoryService
{
    // Singleton pour avoir le même historique partout dans l'application
    public static HistoryService Instance { get; } = new HistoryService();

    private readonly List<HistoryEntry> _history = new();

    // Propriété en lecture seule pour consulter l'historique
    public IReadOnlyList<HistoryEntry> History => _history.AsReadOnly();

    /// <summary>
    /// Ajoute une entrée dans l'historique
    /// </summary>
    public void AddToHistory(string toolName, string command, string output, bool success, TimeSpan? executionTime = null)
    {
        var entry = new HistoryEntry
        {
            Timestamp = DateTime.Now,
            ToolName = toolName,
            Command = command,
            Output = output,
            Success = success,
            ExecutionTime = executionTime
        };

        _history.Add(entry);
        Console.WriteLine($"[History] Entry added for {toolName}");
    }

    /// <summary>
    /// Récupère l'historique filtré par outil
    /// </summary>
    public IEnumerable<HistoryEntry> GetHistoryByTool(string toolName)
    {
        return _history.Where(h => h.ToolName.Equals(toolName, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Récupère l'historique filtré par date
    /// </summary>
    public IEnumerable<HistoryEntry> GetHistoryByDate(DateTime date)
    {
        return _history.Where(h => h.Timestamp.Date == date.Date);
    }

    /// <summary>
    /// Efface tout l'historique
    /// </summary>
    public void ClearHistory()
    {
        _history.Clear();
        Console.WriteLine("[History] History cleared");
    }

    /// <summary>
    /// Efface l'historique d'un outil spécifique
    /// </summary>
    public void ClearHistoryByTool(string toolName)
    {
        _history.RemoveAll(h => h.ToolName.Equals(toolName, StringComparison.OrdinalIgnoreCase));
        Console.WriteLine($"[History] History cleared for {toolName}");
    }

    /// <summary>
    /// Sauvegarde l'historique dans un fichier (ouvre un dialogue de sauvegarde)
    /// </summary>
    public async Task<bool> SaveHistoryToFileAsync(Window owner, string? toolNameFilter = null)
    {
        var storage = owner.StorageProvider;

        // Préparer les données à sauvegarder
        var entriesToSave = string.IsNullOrEmpty(toolNameFilter)
            ? _history
            : _history.Where(h => h.ToolName.Equals(toolNameFilter, StringComparison.OrdinalIgnoreCase)).ToList();

        if (entriesToSave.Count == 0)
        {
            Console.WriteLine("[History] No history to save");
            return false;
        }

        // Ouvrir le dialogue de sauvegarde
        var file = await storage.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Save History",
            SuggestedFileName = $"history_{toolNameFilter ?? "all"}_{DateTime.Now:yyyyMMdd_HHmmss}.txt",
            FileTypeChoices = new[]
            {
                new FilePickerFileType("Text files")
                {
                    Patterns = new[] { "*.txt" }
                },
                new FilePickerFileType("Log files")
                {
                    Patterns = new[] { "*.log" }
                }
            }
        });

        if (file == null)
        {
            Console.WriteLine("[History] Save cancelled by user");
            return false;
        }

        // Formater et sauvegarder
        var content = FormatHistory(entriesToSave, toolNameFilter);
        await File.WriteAllTextAsync(file.Path.LocalPath, content, Encoding.UTF8);
        
        Console.WriteLine($"[History] Saved to {file.Path.LocalPath}");
        return true;
    }

    /// <summary>
    /// Formate l'historique en texte lisible
    /// </summary>
    private string FormatHistory(IEnumerable<HistoryEntry> entries, string? toolFilter)
    {
        var entriesList = entries.ToList(); // Convertir en List pour éviter multiples énumérations
        var sb = new StringBuilder();
        
        // En-tête
        sb.AppendLine("========================================");
        sb.AppendLine($"CRAKIT HISTORY EXPORT");
        sb.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        if (!string.IsNullOrEmpty(toolFilter))
            sb.AppendLine($"Tool: {toolFilter}");
        sb.AppendLine($"Total Entries: {entriesList.Count}");
        sb.AppendLine("========================================");
        sb.AppendLine();

        // Entrées
        foreach (var entry in entriesList.OrderBy(e => e.Timestamp))
        {
            sb.AppendLine(entry.ToString());
            sb.AppendLine();
        }

        // Statistiques
        var successCount = entriesList.Count(e => e.Success);
        var failureCount = entriesList.Count(e => !e.Success);
        
        sb.AppendLine();
        sb.AppendLine("========================================");
        sb.AppendLine("STATISTICS");
        sb.AppendLine("========================================");
        sb.AppendLine($"Total commands: {entriesList.Count}");
        sb.AppendLine($"Successful: {successCount} ({(entriesList.Count > 0 ? successCount * 100.0 / entriesList.Count : 0):F1}%)");
        sb.AppendLine($"Failed: {failureCount} ({(entriesList.Count > 0 ? failureCount * 100.0 / entriesList.Count : 0):F1}%)");
        
        if (entriesList.Any(e => e.ExecutionTime.HasValue))
        {
            var avgTime = entriesList.Where(e => e.ExecutionTime.HasValue)
                                     .Average(e => e.ExecutionTime!.Value.TotalSeconds);
            sb.AppendLine($"Average execution time: {avgTime:F2}s");
        }

        return sb.ToString();
    }

    /// <summary>
    /// Exporte l'historique directement dans un fichier (sans dialogue)
    /// </summary>
    public async Task ExportHistoryAsync(string filePath, string? toolNameFilter = null)
    {
        var entriesToSave = string.IsNullOrEmpty(toolNameFilter)
            ? _history
            : _history.Where(h => h.ToolName.Equals(toolNameFilter, StringComparison.OrdinalIgnoreCase)).ToList();

        var content = FormatHistory(entriesToSave, toolNameFilter);
        await File.WriteAllTextAsync(filePath, content, Encoding.UTF8);
    }
}

