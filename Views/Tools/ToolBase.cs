using System;
using Avalonia.Controls;
using System.IO;

namespace CraKit.Views.Tools;

public abstract class ToolBase
{
    public Guid OutilId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public DateTime CreatedAt { get; set; }

    protected ToolBase(string name, string description)
    {
        OutilId = Guid.NewGuid();
        CreatedAt = DateTime.Now;
        Name = name;
        Description = description;
    }
    
    public static string? LireFichierTexte(string nomFichier)
    {
        try
        {
            if (!File.Exists(nomFichier))
            {
                Console.WriteLine($"[ERREUR] Fichier introuvable : {nomFichier}");
                return null;
            }

            // On lit juste le contenu et on le renvoie tel quel
            Console.WriteLine("Lecture JSON : " +  nomFichier);
            return File.ReadAllText(nomFichier);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERREUR] Erreur lecture {nomFichier} : {ex.Message}");
            return null;
        }
    }

    // Méthode abstraite implémentée pour récupérer vue outil
    public abstract Control GetView();

    // Propriété inutile ici si on n'utilise pas de binding complexe, 
    // mais on la garde pour la compatibilité.
    public Control ContentSpecifique => GetView();


    public Control View
    {
        get
        {
            var laVueReelle = GetView();
            laVueReelle.DataContext = this;

            // On récupère la vue spécifique créée par l'enfant
            var laVueReelle = GetView();

            // On lui assigne le DataContext (pour que le Titre s'affiche)
            laVueReelle.DataContext = this;

            // On retourne la VRAIE vue
            return laVueReelle;
        }
    }

    public virtual void Execute() { }
}