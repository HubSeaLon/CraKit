using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using CraKit.Models;

namespace CraKit.Services;

// Service pour gérer l'historique des commandes
public class HistoryService
{
    // Variable partagée pour avoir le même historique partout
    public static HistoryService Instance = new HistoryService();

    // Liste qui contient toutes les commandes exécutées
    private List<HistoryEntry> historyBrut;
    private List<HistoryEntry> historyParsed;

    // Constructeur
    public HistoryService()
    {
        historyBrut = new List<HistoryEntry>();
        historyParsed = new List<HistoryEntry>();
    }

    // Ajouter dans l'historique brut
    public void AddToHistoryBrut(string toolName, string command, string output, bool success, TimeSpan executionTime)
    {
        HistoryEntry entryBrut = new HistoryEntry();
        
        entryBrut.Timestamp = DateTime.Now;
        entryBrut.ToolName = toolName;
        entryBrut.Command = command;
        entryBrut.Output = output;
        entryBrut.Success = success;
        entryBrut.ExecutionTime = executionTime;
        
        historyBrut.Add(entryBrut);
       
        Console.WriteLine("[HistoryBrut] Nouvelle commande ajoutée pour " + toolName);
    }

    // Ajouter dans l'historique parsed
    public void AddToHistoryParsed(string toolName, string command, string username, string target, string protocol, string format, string result, bool success, TimeSpan executionTime)
    {
        HistoryEntry entryParsed = new HistoryEntry();
        
        entryParsed.Timestamp = DateTime.Now;
        entryParsed.ToolName = toolName;
        entryParsed.Command = command;
        entryParsed.Success = success;
        entryParsed.Username = username;
        entryParsed.Result = result;
        entryParsed.Target = target;
        entryParsed.Protocol = protocol;
        entryParsed.Format = format;
        entryParsed.ExecutionTime = executionTime;
        
        historyParsed.Add(entryParsed);
        
        Console.WriteLine("[HistoryParsed] Nouvelle commande ajoutée pour " + toolName);
    }
    
    // Récupérer l'historique d'un outil spécifique
    private List<HistoryEntry> GetHistoryByTool(string toolName)
    {
        List<HistoryEntry> resultBrut = new List<HistoryEntry>();

        foreach (var entry in historyBrut)
        {
            if (entry.ToolName == toolName) resultBrut.Add(entry);
        }
        
        return resultBrut;
    }
    
    // Récupérer l'historique parsed d'un outil
    private List<HistoryEntry> GetHistoryParsedByTool(string toolName)
    {
        List<HistoryEntry> resultParsed = new List<HistoryEntry>();

        foreach (var entry in historyParsed)
        {
            if (entry.ToolName == toolName) resultParsed.Add(entry);
        }
        
        return resultParsed;
    }

    // Effacer tout l'historique
    public void ClearHistory()
    {
        historyBrut.Clear();
        historyParsed.Clear();
        Console.WriteLine("[HistoryBrut] Historique effacé");
        Console.WriteLine("[HistoryParsed] Historique effacé");
    }

    // Sauvegarder l'historique dans un fichier
    public async Task<bool> SaveHistoryToFileAsync(Window owner, string toolName)
    {
        // Récupérer les commandes par outil (plus propre que tout en même temp)
        List<HistoryEntry> entriesBrut = GetHistoryByTool(toolName);
        List<HistoryEntry> entriesParsed = GetHistoryParsedByTool(toolName);

        // Vérifier qu'il y a des données
        if (entriesBrut.Count == 0 && entriesParsed.Count == 0)
        {
            Console.WriteLine("[HistoryBrut et HistoryParsed] Pas d'historique à sauvegarder");
            return false;
        }
        
        string fileNameBrut   = $"history_brut_{toolName}.txt";
        string fileNameParsed = $"history_parsed_{toolName}.txt";
        
        // Créer le contenu du fichier
        string contentBrut = CreateFileContentBrut(entriesBrut, toolName);
        string contentParsed = CreateFileContentParsed(entriesParsed, toolName);

        // Racine  projet CraKit 
        string root = Directory.GetParent(AppContext.BaseDirectory)!.Parent!.Parent!.Parent!.FullName;

        // Dossiers historique
        string cheminBrut = Path.Combine(root, "Sauvegarde", "Brut", fileNameBrut);
        string cheminParsed = Path.Combine(root, "Sauvegarde", "Parsed", fileNameParsed);
        
        // Sauvegarder le fichier
        await File.AppendAllTextAsync(cheminBrut, contentBrut);
        await File.AppendAllTextAsync(cheminParsed, contentParsed);
        
        Console.WriteLine("[HistoryBrut] " + fileNameBrut + "Sauvegarde ajouté : " + cheminBrut);
        Console.WriteLine("[HistoryParsed] " + fileNameParsed + "Sauvegarde ajouté : " + cheminParsed);
        return true;
    }

    // Créer le contenu du fichier texte brut
    private string CreateFileContentBrut(List<HistoryEntry> entries, string toolName)
    {
        string contentBrut = "";
        
        // En-tête
        contentBrut += "========================================\n";
        contentBrut += "CRAKIT HISTORY EXPORT\n";
        contentBrut += "Generated: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\n";
        contentBrut += "Tool: " + toolName + "\n";
        contentBrut += "Total Entries: " + entries.Count + "\n";
        contentBrut += "========================================\n\n";

        // Ajouter chaque commande
        for (int i = 0; i < entries.Count; i++)
        {
            HistoryEntry entry = entries[i];
            
            string status = entry.Success ? "SUCCESS" : "FAILED";
            string time = entry.ExecutionTime.TotalSeconds.ToString("F2") + "s";
            
            contentBrut += "[" + entry.Timestamp.ToString("yyyy-MM-dd HH:mm:ss") + "] " + status + " (" + time + ")\n";
            contentBrut += "Tool: " + entry.ToolName + "\n";
            contentBrut += "Command: " + entry.Command + "\n";
            contentBrut += "Output:\n" + entry.Output + "\n";
            contentBrut += "------------------------------------------------------------\n\n";
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
        
        contentBrut += "\n========================================\n";
        contentBrut += "STATISTICS\n";
        contentBrut += "========================================\n";
        contentBrut += "Total commands: " + entries.Count + "\n";
        contentBrut += "Successful: " + successCount + " (" + successPercent.ToString("F1") + "%)\n";
        contentBrut += "Failed: " + failCount + " (" + failPercent.ToString("F1") + "%)\n";
        contentBrut += "Average execution time: " + avgTime.ToString("F2") + "s\n";
        contentBrut += "========================================\n";
        
        // Clear pour éviter de sauvegarder plusieurs fois les même choses
        historyBrut.Clear();

        return contentBrut;
    }
    
    
    
    
    // Créer le contenu du fichier texte brut
    private string CreateFileContentParsed(List<HistoryEntry> entries, string toolName)
    {
        string contentParsed = "";
        
        // Ajouter chaque commande de style log
        for (int i = 0; i < entries.Count; i++)
        {
            HistoryEntry entry = entries[i];
            
            contentParsed += entry.Timestamp.ToString("yyyy-MM-dd HH:mm:ss") + "; ";
            contentParsed += entry.Success ? "SUCCESS; " : "FAILED; ";
            contentParsed += entry.Command + "; ";
            contentParsed += entry.Target + "; ";
            
            if (entry.ToolName == "Hydra") contentParsed += entry.Protocol + "; ";
            if (entry.ToolName != "Hydra") contentParsed += entry.Format + "; ";
            if (entry.ToolName == "John") contentParsed += entry.Username + "; ";
            
            contentParsed += entry.Result + "; ";
            contentParsed += entry.ExecutionTime.TotalSeconds.ToString("F2") + "s \n";;
        }
        
        // Clear pour éviter de sauvegarder plusieurs fois les même choses
        historyParsed.Clear();
        
        return contentParsed;
    }
}

