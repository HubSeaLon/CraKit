using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CraKit.Models;

namespace CraKit.Services;

// Service pour gérer l'historique des commandes
public class HistoryService
{
    // Variable partagée pour avoir le même historique partout
    public static HistoryService Instance = new HistoryService();

    // Liste qui contient toutes les commandes exécutées
    private List<HistoryEntry> history;

    // Constructeur
    public HistoryService()
    {
        history = new List<HistoryEntry>();
    }

    // Ajouter une commande dans l'historique
    public void AddToHistory(string toolName, string command, string output, bool success, TimeSpan executionTime)
    {
        HistoryEntry entry = new HistoryEntry();
        entry.Timestamp = DateTime.Now;
        entry.ToolName = toolName;
        entry.Command = command;
        entry.Output = output;
        entry.Success = success;
        entry.ExecutionTime = executionTime;

        history.Add(entry);
        Console.WriteLine("[History] Nouvelle commande ajoutée pour " + toolName);
    }

    // Récupérer l'historique d'un outil spécifique
    public List<HistoryEntry> GetHistoryByTool(string toolName)
    {
        List<HistoryEntry> result = new List<HistoryEntry>();
        
        for (int i = 0; i < history.Count; i++)
        {
            if (history[i].ToolName == toolName)
            {
                result.Add(history[i]);
            }
        }
        
        return result;
    }

    // Effacer tout l'historique
    public void ClearHistory()
    {
        history.Clear();
        Console.WriteLine("[History] Historique effacé");
    }

    // Sauvegarder l'historique dans un fichier
    public async Task<bool> SaveHistoryToFileAsync(Window owner, string toolName)
    {
        // Récupérer les commandes pour cet outil
        List<HistoryEntry> entriesToSave = GetHistoryByTool(toolName);

        // Vérifier qu'il y a des données
        if (entriesToSave.Count == 0)
        {
            Console.WriteLine("[History] Pas d'historique à sauvegarder");
            return false;
        }

        // Demander où sauvegarder le fichier
        var storage = owner.StorageProvider;
        var file = await storage.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Sauvegarder l'historique",
            SuggestedFileName = "history_" + toolName + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".txt"
        });

        if (file == null)
        {
            Console.WriteLine("[History] Sauvegarde annulée");
            return false;
        }

        // Créer le contenu du fichier
        string content = CreateFileContent(entriesToSave, toolName);

        // Sauvegarder le fichier
        await File.WriteAllTextAsync(file.Path.LocalPath, content);
        
        Console.WriteLine("[History] Fichier sauvegardé : " + file.Path.LocalPath);
        return true;
    }

    // Créer le contenu du fichier texte
    private string CreateFileContent(List<HistoryEntry> entries, string toolName)
    {
        string content = "";
        
        // En-tête
        content += "========================================\n";
        content += "CRAKIT HISTORY EXPORT\n";
        content += "Generated: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\n";
        content += "Tool: " + toolName + "\n";
        content += "Total Entries: " + entries.Count + "\n";
        content += "========================================\n\n";

        // Ajouter chaque commande
        for (int i = 0; i < entries.Count; i++)
        {
            HistoryEntry entry = entries[i];
            
            string status = entry.Success ? "SUCCESS" : "FAILED";
            string time = entry.ExecutionTime.TotalSeconds.ToString("F2") + "s";
            
            content += "[" + entry.Timestamp.ToString("yyyy-MM-dd HH:mm:ss") + "] " + status + " (" + time + ")\n";
            content += "Tool: " + entry.ToolName + "\n";
            content += "Command: " + entry.Command + "\n";
            content += "Output:\n" + entry.Output + "\n";
            content += "------------------------------------------------------------\n\n";
        }

        // Statistiques simples
        int successCount = 0;
        int failCount = 0;
        double totalTime = 0;
        
        for (int i = 0; i < entries.Count; i++)
        {
            if (entries[i].Success)
                successCount++;
            else
                failCount++;
                
            totalTime += entries[i].ExecutionTime.TotalSeconds;
        }
        
        double avgTime = entries.Count > 0 ? totalTime / entries.Count : 0;
        double successPercent = entries.Count > 0 ? (successCount * 100.0) / entries.Count : 0;
        double failPercent = entries.Count > 0 ? (failCount * 100.0) / entries.Count : 0;
        
        content += "\n========================================\n";
        content += "STATISTICS\n";
        content += "========================================\n";
        content += "Total commands: " + entries.Count + "\n";
        content += "Successful: " + successCount + " (" + successPercent.ToString("F1") + "%)\n";
        content += "Failed: " + failCount + " (" + failPercent.ToString("F1") + "%)\n";
        content += "Average execution time: " + avgTime.ToString("F2") + "s\n";

        return content;
    }
}

