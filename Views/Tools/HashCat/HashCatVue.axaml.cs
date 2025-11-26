using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using CraKit.Models;
using CraKit.Services;
using CraKit.Templates; 

namespace CraKit.Views.Tools.HashCat;

public partial class HashCatVue : TemplateControl
{
    private readonly ToolFileService toolFileService;
    public HashCatVue()
    {
        InitializeComponent();
        toolFileService = new ToolFileService(ConnexionSshService.Instance);
        ChargerLesListes();
    }
    
    // Fonction qui fait le travail (LS en SSH)
    private void RemplirComboBox(ComboBox laBox, string chemin)
    {
        // 1. SECURITÉ : Si la boite est null (pas trouvée), on arrête tout pour éviter le crash
        if (laBox == null) return;

        try 
        {
            var ssh = ConnexionSshService.Instance.Client;

            // 2. SECURITÉ : Si pas connecté, on arrête
            if (ssh == null || !ssh.IsConnected) return;

            var cmd = ssh.CreateCommand($"ls -1 {chemin}");
            string resultat = cmd.Execute();

            laBox.Items.Clear(); // <-- C'est ici que ça plantait avant si laBox était null
            
            if (!string.IsNullOrWhiteSpace(resultat) && !resultat.Contains("No such file"))
            {
                var fichiers = resultat.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var f in fichiers)
                {
                    laBox.Items.Add(f);
                }
                
                if (laBox.Items.Count > 0) laBox.SelectedIndex = 0;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur chargement liste : {ex.Message}");
        }
    }
    
    private void ChargerLesListes()
    {
        // --- CORRECTION MAJEURE ICI ---
        // On utilise FindControl pour être sûr à 100% de trouver l'objet du XAML
        // Sinon, les variables WordlistComboBox peuvent être nulles
        
        var boxWordlist = this.FindControl<ComboBox>("WordlistComboBox");
        var boxHashfile = this.FindControl<ComboBox>("HashfileComboBox");

        // On ne lance la fonction que si on a bien trouvé les boites
        if (boxWordlist != null) 
            RemplirComboBox(boxWordlist, "/root/wordlists");
        
        if (boxHashfile != null)
            RemplirComboBox(boxHashfile, "/root/hashfiles");
    }

    private void DictionaryAttackClick(object? sender, RoutedEventArgs e)
    {
      
    }

    private async void AjouterWordlistClick(object? sender, RoutedEventArgs e)
    {
        var window = TopLevel.GetTopLevel(this) as Window;
        if (window is null) return;
        
        try
        {
            await toolFileService.PickAndUploadAsync(ToolFileModel.Wordlist, window);
            Console.WriteLine("Wordlist uploaded !");
            
            // Ajouter MessageBox pour avertir
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
    
    private async void AjouterHashfileClick(object? sender, RoutedEventArgs e)
    {
        var window = TopLevel.GetTopLevel(this) as Window;
        if (window is null) return;

        try
        {
            await toolFileService.PickAndUploadAsync(ToolFileModel.HashFile, window);
            Console.WriteLine("Hashfile uploaded !");
            
            // Ajouter MessageBox pour avertir
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        
    }
    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
    protected override Type StyleKeyOverride => typeof(TemplateControl);
}