using System;

namespace CraKit.Models;

// Classe simple pour stocker une commande dans l'historique
public class HistoryEntry
{
    // Quand la commande a été exécutée
    public DateTime Timestamp;
    
    // Nom de l'outil (Hydra, John, etc.)
    public string ToolName;
    
    // La commande qui a été lancée
    public string Command;
    
    // Le résultat de la commande
    public string Output;
    
    // Si ça a marché ou pas
    public bool Success;
    
    // Combien de temps ça a pris
    public TimeSpan ExecutionTime;

    // Constructeur 
    public HistoryEntry()
    {
        Timestamp = DateTime.Now;
        ToolName = "";
        Command = "";
        Output = "";
        Success = false;
        ExecutionTime = TimeSpan.Zero;
    }
}