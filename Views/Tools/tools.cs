using System;
using Avalonia.Controls;

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

    // Méthode abstraite implémentée par HashCat
    public abstract Control GetView();

    // Propriété inutile ici si on n'utilise pas de binding complexe, 
    // mais on la garde pour la compatibilité.
    public Control ContentSpecifique => GetView();

    // --- CORRECTION MAJEURE ICI ---
    public Control View
    {
        get
        {
            // 1. On récupère la vue spécifique créée par l'enfant (HashCatVue)
            var laVueReelle = GetView();

            // 2. On lui assigne le DataContext (pour que le Titre s'affiche)
            laVueReelle.DataContext = this;

            // 3. On retourne la VRAIE vue (pas une nouvelle vide)
            return laVueReelle;
        }
    }

    public virtual void Execute() { }
}