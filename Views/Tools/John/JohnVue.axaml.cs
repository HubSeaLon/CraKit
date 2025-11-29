using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using CraKit.Models;
using CraKit.Services;
using CraKit.Templates;


namespace CraKit.Views.Tools.John;

public partial class JohnVue : TemplateControl
{
    // Initialisation des variables 
    private string commande = "";
    private string hashfile = "";
    private string wordlist = "";
    private string format = "";
    private string rule = "";
    private string mask = "";
    private bool hashidSelected;
    
    private readonly ToolFileService toolFileService;
    private readonly ExecuterCommandeService executerCommandeService;

    public JohnVue()
    {
        InitializeComponent();
        
        // Injection des Instances 
        toolFileService = new ToolFileService(ConnexionSshService.Instance);
        executerCommandeService = new ExecuterCommandeService(ConnexionSshService.Instance);
        
        AttachedToVisualTree += OnAttachedToVisualTree;
        
        // Chargement des listes déroulantes
        ChargerLesListes();
        ChargerHashTypes();
        ChargerFormatTypes();
    }
    
    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    // Affichage selon l'option choisi
    private void choixOptionClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button btn) return; 
        
        var name = btn.Name;
        
        ResetButtonStyles();
        
        switch (name)
        {
            case "ButtonOption1":
                hashidSelected = true;
                
                ButtonOption1.Opacity = 0.4;
                
                WordlistComboBox!.IsVisible = false;
                HashfileComboBox!.IsVisible = true;
                FormatHashComboBox!.IsVisible = false;
                RuleComboBox!.IsVisible = false;
                MaskTextBox!.IsVisible = false;
                break;
            
            case "ButtonOption2":
                hashidSelected = false;
          
                ButtonOption2.Opacity = 0.4;
                
                WordlistComboBox!.IsVisible = true;
                HashfileComboBox!.IsVisible = true;
                FormatHashComboBox!.IsVisible = false;
                RuleComboBox!.IsVisible = false;
                MaskTextBox!.IsVisible = false;
                break; 
            
            case "ButtonOption3":
                hashidSelected = false;
       
                ButtonOption3.Opacity = 0.4;
                
                WordlistComboBox!.IsVisible = true;
                HashfileComboBox!.IsVisible = true;
                FormatHashComboBox!.IsVisible = true;
                RuleComboBox!.IsVisible = false;
                MaskTextBox!.IsVisible = false;
                break;
            
            case "ButtonOption4":
                hashidSelected = false;
           
                ButtonOption4.Opacity = 0.4;
                
                WordlistComboBox!.IsVisible = true;
                HashfileComboBox!.IsVisible = true;
                FormatHashComboBox!.IsVisible = true;
                RuleComboBox!.IsVisible = true;
                MaskTextBox!.IsVisible = false;
                break;
            
            case "ButtonOption5":
                hashidSelected = false;
              
                ButtonOption5.Opacity = 0.4;
                
                WordlistComboBox!.IsVisible = true;
                HashfileComboBox!.IsVisible = true;
                FormatHashComboBox!.IsVisible = true;
                RuleComboBox!.IsVisible = false;
                MaskTextBox!.IsVisible = true;
                break;
        }
    }
    
    // Reset visuel et fonctionnel 
    private void ResetButtonStyles()
    {
        ButtonOption1.Opacity = 1;
        ButtonOption2.Opacity = 1;
        ButtonOption3.Opacity = 1;
        ButtonOption4.Opacity = 1;
        ButtonOption5.Opacity = 1;

        format = "";
        wordlist = "";
        hashfile = "";
        rule = "";

        FormatHashComboBox.SelectedIndex = -1;
        WordlistComboBox.SelectedIndex = -1;
        HashfileComboBox.SelectedIndex = -1;
        RuleComboBox.SelectedIndex = -1;
        MaskTextBox.Text = "";
    }
    
    
    // Ajout des wordlists et hashfile 
    private async void AjouterWordlistClick(object? sender, RoutedEventArgs e)
    {
        var window = TopLevel.GetTopLevel(this) as Window;
        if (window is null) return;
        
        try
        {
            await toolFileService.PickAndUploadAsync(ToolFileModel.Wordlist, window);
            // Ajouter MessageBox pour avertir ou zone texte
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
            // Ajouter MessageBox pour avertir ? Ou bien une zone texte simple
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        
    }
    
    
    // Méthode permettant de récupérer les noms des bouttons, listes, etc. vu qu'on utilise un template
    private void OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        WordlistComboBox = this.FindControl<ComboBox>("WordlistComboBox");
        HashfileComboBox = this.FindControl<ComboBox>("HashfileComboBox");
        FormatHashComboBox = this.FindControl<ComboBox>("FormatHashComboBox");
        RuleComboBox = this.FindControl<ComboBox>("RuleComboBox");
        MaskTextBox = this.FindControl<TextBox>("MaskTextBox");
        
        ButtonOption1 = this.FindControl<Button>("ButtonOption1");
        ButtonOption2 = this.FindControl<Button>("ButtonOption2");
        ButtonOption3 =  this.FindControl<Button>("ButtonOption3");
        ButtonOption4 =  this.FindControl<Button>("ButtonOption4");
        ButtonOption5 =  this.FindControl<Button>("ButtonOption5");
        
        EntreeTextBox =  this.FindControl<TextBox>("EntreeTextBox");
        SortieTextBox =  this.FindControl<TextBox>("SortieTextBox");

        WordlistComboBox!.IsVisible= false;
        HashfileComboBox!.IsVisible= false;
        FormatHashComboBox!.IsVisible= false;
        RuleComboBox!.IsVisible= false;
        MaskTextBox!.IsVisible= false;
    }
    
    
     // Fonction qui fait le travail (LS en SSH)
    private void RemplirComboBox(ComboBox laBox, string chemin)
    {
        try 
        {
            var ssh = ConnexionSshService.Instance.Client;
            
            var cmd = ssh!.CreateCommand($"ls -1 {chemin}");
            var resultat = cmd.Execute();

            laBox.Items.Clear();
            
            if (!string.IsNullOrWhiteSpace(resultat) && !resultat.Contains("No such file"))
            {
                var fichiers = resultat.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var f in fichiers)
                {
                    laBox.Items.Add(f);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur chargement liste : {ex.Message}");
        }
    }
    
    // Creation du Mask 
    private void OnChangedText(object? sender, TextChangedEventArgs e)
    {
        if (sender is not TextBox textBox) return; 
        
        if (textBox.Name == "MaskTextBox")
        {
            if (string.IsNullOrWhiteSpace(MaskTextBox.Text))
            {
                // aucun mask → on enlève complètement l’option
                mask = string.Empty;
            }
            else
            {
                mask = " --mask='" + MaskTextBox.Text + "'";
            }
        }
        
        commande = "john" + wordlist + mask + format + rule + hashfile;
        
        EntreeTextBox.Text = commande;
        Console.WriteLine("Commande : " + commande);
    }
    
    
    // Création de la commande selon les choix de l'user
    private void OnChangedList(object? sender, SelectionChangedEventArgs e)
    {
        if (sender is not ComboBox comboBox) return; 
        
        var name = comboBox.Name;
        
        switch (name)
        {
            case "WordlistComboBox":
                if (WordlistComboBox.SelectedItem is null)
                {
                    wordlist = "";
                    break;
                }
                
                if (WordlistComboBox.SelectedItem!.ToString() == "rockyou.txt")
                {
                    wordlist = " --wordlist=/usr/share/wordlists/rockyou.txt";
                }
                else
                {  
                    wordlist = " --wordlist=/wordlists/" + WordlistComboBox.SelectedItem!;
                }
                break;
            
            case "HashfileComboBox":
                if (HashfileComboBox.SelectedItem is null)
                {
                    hashfile = "";
                    break;
                }
                
                hashfile = " hashfiles/" + HashfileComboBox.SelectedItem!;
                break;
            
            case "FormatHashComboBox":
                if (FormatHashComboBox.SelectedItem is null)
                {
                    format = "";
                    break;
                }
                
                format = " --format=" + FormatHashComboBox.SelectedItem!;
                break;
            
            case "RuleComboBox":
                if (RuleComboBox.SelectedItem is null)
                {
                    rule = "";
                    break;
                }
                
                rule = " --rules=" + RuleComboBox.SelectedItem!;
                break;
        }
        
        if (hashidSelected)
        {
            commande = "hashid" + hashfile;
        }
        else
        {
            commande = "john" + wordlist + mask + format + rule + hashfile;
        }
        
        EntreeTextBox.Text = commande;
        Console.WriteLine("Commande : " + commande);
    }


    // Lancer la commande et afficher 
    private async void LancerCommandeClick(object? sender, RoutedEventArgs e)
    {
        var cmd = commande.Trim();
        if (string.IsNullOrWhiteSpace(cmd)) return;
        
        SortieTextBox.Text = $"$ {cmd}\n";
        
        // Tolérance de 5 min pour des craking long (peut etre ajouter un timer)
        var outp = await executerCommandeService.ExecuteCommandAsync(cmd, TimeSpan.FromMinutes(5));
        SortieTextBox.Text += outp + "\n";
    }
    
    // Chargement des différentes listes déroulantes (lecture json)
    private void ChargerLesListes()
    {
        var boxWordlist = this.FindControl<ComboBox>("WordlistComboBox");
        var boxHashfile = this.FindControl<ComboBox>("HashfileComboBox");

        // On ne lance la fonction que si on a bien trouvé les boites
        if (boxWordlist != null)
        {
            RemplirComboBox(boxWordlist, "/root/wordlists");
            boxWordlist.Items.Add("rockyou.txt");
        }
        if (boxHashfile != null) RemplirComboBox(boxHashfile, "/root/hashfiles");
    }
    
    private void ChargerHashTypes()
    {
        try 
        {
            string chemin = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "john_options.json");
            string? jsonBrut = ToolBase.LireFichierTexte(chemin);

            if (string.IsNullOrEmpty(jsonBrut)) return;
            
            var rootNode = JsonNode.Parse(jsonBrut);
            
            var valuesNode = rootNode?["options"]?["format"]?["values"];

            if (valuesNode != null)
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var listeModes = valuesNode.Deserialize<System.Collections.Generic.List<OptionMode>>(options);
                
                var boxType = this.FindControl<ComboBox>("FormatHashComboBox");
                if (listeModes != null && boxType != null) boxType.ItemsSource = listeModes;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[JSON ERROR] {ex.Message}");
        }
    }
    
    
    private void ChargerFormatTypes()
    {
        try 
        {
            string chemin = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "john_options.json");
            string? jsonBrut = ToolBase.LireFichierTexte(chemin);

            if (string.IsNullOrEmpty(jsonBrut)) return;
            
            var rootNode = JsonNode.Parse(jsonBrut);
            
            var valuesNode = rootNode?["options"]?["rules"]?["values"];

            if (valuesNode != null)
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var listeModes = valuesNode.Deserialize<System.Collections.Generic.List<OptionMode>>(options);
                
                var boxType = this.FindControl<ComboBox>("RuleComboBox");
                if (listeModes != null && boxType != null) boxType.ItemsSource = listeModes;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[JSON ERROR] {ex.Message}");
        }
    }
    
    // aller chercher le style <Style Selector="control|TemplateControl">
    protected override Type StyleKeyOverride => typeof(TemplateControl);
}