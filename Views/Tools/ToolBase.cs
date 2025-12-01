using System;
using Avalonia.Controls;
using System.IO;

namespace CraKit.Views.Tools;

// Classe de base pour tous les outils (John, Hydra, HashCat)
public abstract class ToolBase
{
    // Identifiant unique de l'outil
    public Guid OutilId;
    
    // Nom de l'outil
    public string Name;
    
    // Description de l'outil
    public string Description;
    
    // Date de creation
    public DateTime CreatedAt;

    // Constructeur
    protected ToolBase(string name, string description)
    {
        OutilId = Guid.NewGuid();
        CreatedAt = DateTime.Now;
        Name = name;
        Description = description;
    }
    
    // Lire un fichier texte (pour les options JSON)
    public static string LireFichierTexte(string nomFichier)
    {
        try
        {
            if (!File.Exists(nomFichier))
            {
                Console.WriteLine("[ERREUR] Fichier introuvable : " + nomFichier);
                return null;
            }

            Console.WriteLine("Lecture JSON : " +  nomFichier);
            return File.ReadAllText(nomFichier);
        }
        catch (Exception ex)
        {
            Console.WriteLine("[ERREUR] Erreur lecture " + nomFichier + " : " + ex.Message);
            return null;
        }
    }

    // Methode abstraite pour recuperer la vue
    public abstract Control GetView();

    // Propriete pour recuperer la vue
    public Control View
    {
        get
        {
            // Recuperer la vue specifique
            Control laVueReelle = GetView();

            // Assigner le DataContext (pour le titre)
            laVueReelle.DataContext = this;

            // Retourner la vue
            return laVueReelle;
        }
    }

    // Methode vide par defaut
    public virtual void Execute() 
    { 
    }
}

