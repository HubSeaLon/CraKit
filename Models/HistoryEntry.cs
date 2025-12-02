using System;

namespace CraKit.Models;

// Classe simple pour stocker une commande dans l'historique
public class HistoryEntry
{
    public DateTime Timestamp;
    public string ToolName;
    public string Command;
    public string Output;

    // Ajout du Username pour Hydra surtout
    public string Username;
    
    // Ajout du Target pour les logs parsed 
    public string Target;

    public string Protocol;
    // Ajout du Result pour logs parsed (mot de passe cracked ou trouvé)
    public string Result;
    public bool Success;
    public TimeSpan ExecutionTime;

    // Constructeur 
    public HistoryEntry()
    {
        Timestamp = DateTime.Now;
        ToolName = "";
        Command = "";
        Output = "";
        Username = "";
        Protocol = "";
        Target = "";
        Result = "";
        Success = false;
        ExecutionTime = TimeSpan.Zero;
    }
}

