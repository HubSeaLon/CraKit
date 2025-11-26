using System;
using System.Text.Json;
using System.Text.Json.Nodes; 
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
        ChargerHashTypes();
    }
    
    private void ChargerHashTypes()
    {
        try 
        {
            string chemin = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "HashCat.json");
            string? jsonBrut = ToolBase.LireFichierTexte(chemin);

            if (string.IsNullOrEmpty(jsonBrut)) return;
            
            var rootNode = JsonNode.Parse(jsonBrut);
            
            var valuesNode = rootNode?["options"]?["hashType"]?["values"];

            if (valuesNode != null)
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var listeModes = valuesNode.Deserialize<System.Collections.Generic.List<HashMode>>(options);
                
                var boxType = this.FindControl<ComboBox>("HashTypeComboBox");
                if (listeModes != null && boxType != null)
                {
                    boxType.ItemsSource = listeModes;
                    boxType.SelectedIndex = 0;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[JSON ERROR] {ex.Message}");
        }
    }
    
    private void RemplirComboBox(ComboBox laBox, string chemin)
    {
        if (laBox == null) return;

        try
        {
            var ssh = ConnexionSshService.Instance.Client;
            if (ssh == null || !ssh.IsConnected) return;

            var cmd = ssh.CreateCommand($"ls -1 {chemin}");
            string resultat = cmd.Execute();

            laBox.Items.Clear();

            if (!string.IsNullOrWhiteSpace(resultat) && !resultat.Contains("No such file"))
            {
                var fichiers = resultat.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var f in fichiers) laBox.Items.Add(f);
                
                if (laBox.Items.Count > 0) laBox.SelectedIndex = 0;
            }
        }
        catch (Exception ex) { Console.WriteLine($"[SSH] Erreur : {ex.Message}"); }
    }

    private void ChargerLesListes()
    {
        var boxWordlist = this.FindControl<ComboBox>("WordlistComboBox");
        var boxHashfile = this.FindControl<ComboBox>("HashfileComboBox");

        if (boxWordlist != null) RemplirComboBox(boxWordlist, "/root/wordlists");
        if (boxHashfile != null) RemplirComboBox(boxHashfile, "/root/hashfiles");
    }

    private void DictionaryAttackClick(object? sender, RoutedEventArgs e)
    {
        
    }
    
    private async void AjouterWordlistClick(object? sender, RoutedEventArgs e)
    {
        var window = TopLevel.GetTopLevel(this) as Window;
        if (window == null) return;
        try
        {
            await toolFileService.PickAndUploadAsync(ToolFileModel.Wordlist, window);
            var box = this.FindControl<ComboBox>("WordlistComboBox");
            if (box != null) RemplirComboBox(box, "/root/wordlists");
        }
        catch (Exception ex) { Console.WriteLine(ex.Message); }
    }

    private async void AjouterHashfileClick(object? sender, RoutedEventArgs e)
    {
        var window = TopLevel.GetTopLevel(this) as Window;
        if (window == null) return;
        try
        {
            await toolFileService.PickAndUploadAsync(ToolFileModel.HashFile, window);
            var box = this.FindControl<ComboBox>("HashfileComboBox");
            if (box != null) RemplirComboBox(box, "/root/hashfiles");
        }
        catch (Exception ex) { Console.WriteLine(ex.Message); }
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    protected override Type StyleKeyOverride => typeof(TemplateControl);
}

