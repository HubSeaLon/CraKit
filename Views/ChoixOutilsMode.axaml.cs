﻿using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Interactivity;
using CraKit.Services;
using CraKit.Views.Tools.HashCat;
using CraKit.Views.Tools.Hydra;
using CraKit.Views.Tools.John;

namespace CraKit.Views;

public partial class ChoixOutilsMode : UserControl, INotifyPropertyChanged
{
    public new event PropertyChangedEventHandler? PropertyChanged; // Even pour que les éléments puisse se charger
    public ChoixOutilsMode()
    {
        InitializeComponent();
        DataContext = this;  // Binding
        
        // Utilisation de l'évent PropertyChanged du service sur cette vue
        ConnexionSshService.Instance.PropertyChanged += PropertyChanged;
    }
    
    // Les valeurs en binding
    private bool _isSimpleMode; // false par défaut en C# pour le mode expert
    
    // Utiliser la même instance de ConnexionSshService partout pour récupérer le statut de connexion
    public string IsConnected => ConnexionSshService.Instance.IsConnected ? "Connecté localhost" : "Non connecté";
    public string Title => IsSimpleMode ? "Liste des outils par catégorie" : "Liste des outils";

    public bool IsSimpleMode
    {
        get => _isSimpleMode;
        set { if (_isSimpleMode == value) return; 
            _isSimpleMode = value; 
            OnPropertyChanged(nameof(IsSimpleMode)); 
            OnPropertyChanged(nameof(Title)); 
        }
    }
    void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    
    private void OpenJohn(object? sender, RoutedEventArgs e)
    {
        // Navigation à l'outil selon simple ou expert depuis le service 
        var johnTool = new John();
        var fenetreOutil = new Window
        {
            Title = johnTool.Name,
            Width = 1000,
            Height = 700,

            Content = johnTool.View
        };
        
        fenetreOutil.Show();
    }

    private void OpenHashCat(object? sender, RoutedEventArgs e)
    {
        // 1. On instancie le "Chef" (La logique de l'outil)
        var hashcatTool = new HashCat();

        // 2. On prépare une fenêtre vierge pour l'accueillir
        var fenetreOutil = new Window
        {
            Title = hashcatTool.Name, // Utilise le nom défini dans la classe (Hashcat)
            Width = 1000,
            Height = 700,
            
            // C'est ICI que la magie opère : 
            // On récupère le Template complet (Design + Boutons) via la propriété .View
            Content = hashcatTool.View 
        };

        // 3. On affiche la fenêtre
        fenetreOutil.Show();
    }

    private void OpenHydra(object? sender, RoutedEventArgs e)
    {
        var hydraTool = new Hydra();
        var fenetreOutil = new Window
        {
            Title = hydraTool.Name,
            Width = 1000,
            Height = 700,
            Content = hydraTool.View
        };
        fenetreOutil.Show();
    }

    private void OpenDnsMap(object? sender, RoutedEventArgs e)
    {
        // DNSMap n'est pas encore implémenté
        // TODO: Implémenter DNSMap
        System.Console.WriteLine("[Info] DNSMap n'est pas encore implémenté");
    }
}