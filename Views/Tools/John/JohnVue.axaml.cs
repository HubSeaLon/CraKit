using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using CraKit.Models;
using CraKit.Services;
using CraKit.Templates;
using Org.BouncyCastle.Bcpg.Attr;


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
    private int optionSelectionnee = 0;
    
    
    private readonly ToolFileService toolFileService;
    private readonly ExecuterCommandeService executerCommandeService;
    private readonly HistoryService historyService;

    public JohnVue()
    {
        InitializeComponent();
        
        // Injection des Instances 
        toolFileService = new ToolFileService(ConnexionSshService.Instance);
        executerCommandeService = new ExecuterCommandeService(ConnexionSshService.Instance);
        historyService = HistoryService.Instance;
        
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
                optionSelectionnee = 1;
                hashidSelected = true;
                
                ButtonOption1.Opacity = 0.4;
                
                WordlistComboBox!.IsVisible = false;
                HashfileComboBox!.IsVisible = true;
                FormatHashComboBox!.IsVisible = false;
                RuleComboBox!.IsVisible = false;
                MaskTextBox!.IsVisible = false;
                break;
            
            case "ButtonOption2":
                optionSelectionnee = 2;
                hashidSelected = false;
          
                ButtonOption2.Opacity = 0.4;
                
                WordlistComboBox!.IsVisible = true;
                HashfileComboBox!.IsVisible = true;
                FormatHashComboBox!.IsVisible = false;
                RuleComboBox!.IsVisible = false;
                MaskTextBox!.IsVisible = false;
                break; 
            
            case "ButtonOption3":
                optionSelectionnee = 3;
                hashidSelected = false;
       
                ButtonOption3.Opacity = 0.4;
                
                WordlistComboBox!.IsVisible = true;
                HashfileComboBox!.IsVisible = true;
                FormatHashComboBox!.IsVisible = true;
                RuleComboBox!.IsVisible = false;
                MaskTextBox!.IsVisible = false;
                break;
            
            case "ButtonOption4":
                optionSelectionnee = 4;
                hashidSelected = false;
           
                ButtonOption4.Opacity = 0.4;
                
                WordlistComboBox!.IsVisible = true;
                HashfileComboBox!.IsVisible = true;
                FormatHashComboBox!.IsVisible = true;
                RuleComboBox!.IsVisible = true;
                MaskTextBox!.IsVisible = false;
                break;
            
            case "ButtonOption5":
                optionSelectionnee = 5;
                hashidSelected = false;
              
                ButtonOption5.Opacity = 0.4;
                
                WordlistComboBox!.IsVisible = true;
                HashfileComboBox!.IsVisible = true;
                FormatHashComboBox!.IsVisible = true;
                RuleComboBox!.IsVisible = false;
                MaskTextBox!.IsVisible = true;
                break;
        }
        
        // Vérifier si on peut activer le bouton LancerCommande
        VerifierEtActiverBouton();
    }
    
    private void VerifierEtActiverBouton()
    {
        var btnLancer = this.FindControl<Button>("BtnLancer");
        if (btnLancer == null) return;

        bool peutLancer = false;

        switch (optionSelectionnee)
        {
            case 1: // Hashid : juste le hashfile
                peutLancer = !string.IsNullOrWhiteSpace(hashfile);
                break;
            
            case 2: // John simple : wordlist + hashfile
                peutLancer = !string.IsNullOrWhiteSpace(wordlist) && 
                             !string.IsNullOrWhiteSpace(hashfile);
                break;
            
            case 3: // John avec format : wordlist + hashfile + format
                peutLancer = !string.IsNullOrWhiteSpace(wordlist) && 
                             !string.IsNullOrWhiteSpace(hashfile) &&
                             !string.IsNullOrWhiteSpace(format);
                break;
            
            case 4: // John avec rules : wordlist + hashfile + format + rule
                peutLancer = !string.IsNullOrWhiteSpace(wordlist) && 
                             !string.IsNullOrWhiteSpace(hashfile) &&
                             !string.IsNullOrWhiteSpace(format) &&
                             !string.IsNullOrWhiteSpace(rule);
                break;
            
            case 5: // John avec mask : wordlist + hashfile + format + mask
                peutLancer = !string.IsNullOrWhiteSpace(wordlist) && 
                             !string.IsNullOrWhiteSpace(hashfile) &&
                             !string.IsNullOrWhiteSpace(format) &&
                             !string.IsNullOrWhiteSpace(mask);
                break;
            
            default: // Aucune option sélectionnée
                peutLancer = false;
                break;
        }

        btnLancer.IsEnabled = peutLancer;
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
            ChargerLesListes();
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
            ChargerHashTypes();
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
        ButtonOption3 = this.FindControl<Button>("ButtonOption3");
        ButtonOption4 = this.FindControl<Button>("ButtonOption4");
        ButtonOption5 = this.FindControl<Button>("ButtonOption5");
        
        BtnLancer = this.FindControl<Button>("BtnLancer");
        
        EntreeTextBox = this.FindControl<TextBox>("EntreeTextBox");
        SortieTextBox = this.FindControl<TextBox>("SortieTextBox");

        WordlistComboBox!.IsVisible= false;
        HashfileComboBox!.IsVisible= false;
        FormatHashComboBox!.IsVisible= false;
        RuleComboBox!.IsVisible= false;
        MaskTextBox!.IsVisible= false;
        
        BtnLancer!.IsEnabled= false;
    }
    
    
    // Fonction qui remplie les listes déroulantes
    private void RemplirComboBox(ComboBox laBox, string chemin)
    {
        try 
        {
            var ssh = ConnexionSshService.Instance.Client;

            // Si pas connecté, on arrête
            if (ssh == null || !ssh.IsConnected) return;

            var cmd = ssh.CreateCommand($"ls -1 {chemin}");
            string resultat = cmd.Execute();

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
        
        VerifierEtActiverBouton();
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
                    wordlist = " --wordlist=/root/wordlists/" + WordlistComboBox.SelectedItem!;
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
        
        VerifierEtActiverBouton();
    }


    // Lancer la commande et afficher 
    private async void LancerCommandeClick(object? sender, RoutedEventArgs e)
    {
        var cmd = commande.Trim();
        if (string.IsNullOrWhiteSpace(cmd)) return;
        
        SortieTextBox.Text = $"$ {cmd}\n";
        
        var stopwatch = Stopwatch.StartNew();
        var outputBuilder = new StringBuilder();
        
        try
        {
            // Laisser 1 min max si la commande met du temps à se lancer

            var outp = await executerCommandeService.ExecuteCommandAsync(cmd, TimeSpan.FromMinutes(1));
            SortieTextBox.Text += outp + "\n";
            outputBuilder.Append(outp);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur lancement de la commande : {ex.Message}");
        }
        finally
        {
            // Ne pas enregistrer la commande hashid dans l'historique
            if (!hashidSelected)
            {
                stopwatch.Stop();

                var output = outputBuilder.ToString();
                var success = IsJohnSuccessful(output);
            
                HashfileComboBox = this.FindControl<ComboBox>("HashfileComboBox");
                FormatHashComboBox =  this.FindControl<ComboBox>("FormatHashComboBox");
            
                var target = HashfileComboBox!.SelectionBoxItem!.ToString();
                var username = ExtractJohnUsername(output);

                string format = FormatHashComboBox!.SelectionBoxItem?.ToString() ?? "";
                
                var result = ExtractJohnPassword(output);

                // Enregistrer dans l'historique brut
                historyService.AddToHistoryBrut("John", cmd, output, success, stopwatch.Elapsed);
                historyService.AddToHistoryParsed("John", cmd, username!, target!, "", format!, result, success, stopwatch.Elapsed);

                Console.WriteLine($"[Commande Brut + Parsed] ajoutées à l'historique ({stopwatch.Elapsed.TotalSeconds:F2}s) - Success: {success}");
            }
        }
    }
    
    private bool IsJohnSuccessful(string output)
    {
        if (string.IsNullOrWhiteSpace(output)) return false;

        // Vérifie si John a trouvé au moins 1 mot de passe
        // Recherche "1g" qui signifie 1 mot de passe trouvé
        if (Regex.IsMatch(output, @"\b1g\b|\bguesses:\s*1\b", RegexOptions.IgnoreCase))
            return true;

        // Alternative : cherche le pattern "mot_de_passe (utilisateur ou ?)"
        return Regex.IsMatch(output, @"^(\S+)\s+\([^)]*\)\s*$", RegexOptions.Multiline);
    }

    private string ExtractJohnPassword(string output)
    {
        if (string.IsNullOrWhiteSpace(output)) return "No password found";

        var passwords = new List<string>();
    
        var lines = output.Split('\n');
        foreach (var line in lines)
        {
            var match = Regex.Match(line.Trim(), @"^(\S+)\s+\([^)]*\)");
            if (match.Success)
            {
                passwords.Add(match.Groups[1].Value);
            }
        }

        return passwords.Count > 0 ? string.Join(", ", passwords) : "No password found";
    }

    private string ExtractJohnUsername(string output)
    {
        if (string.IsNullOrWhiteSpace(output)) return "No username";

        var usernames = new List<string>();
    
        var lines = output.Split('\n');
        foreach (var line in lines)
        {
            var match = Regex.Match(line.Trim(), @"^\S+\s+\(([^)]+)\)");
            if (match.Success)
            {
                var username = match.Groups[1].Value.Trim();
            
                // Remplacer "?" par "No username" pour garder la correspondance
                usernames.Add(username == "?" ? "No username" : username);
            }
        }

        return usernames.Count > 0 ? string.Join(", ", usernames) : "No username";
    }

    
    private async void SaveHistoryClick(object? sender, RoutedEventArgs e)
    {
        var window = TopLevel.GetTopLevel(this) as Window;
        if (window is null) return;

        try
        {
            var success = await historyService.SaveHistoryToFileAsync(window, "John");
            
            if (success)
            {
                Console.WriteLine("[John] Historique sauvegardé avec succès !");
                // TODO: Afficher un message de confirmation à l'utilisateur
            }
            else
            {
                Console.WriteLine("[John] Aucun historique à sauvegarder ou annulé");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[John] Erreur lors de la sauvegarde : {ex.Message}");
        }
    }
    
    
    // ------------------------------------------------------------------------
    // Chargement des différentes listes déroulantes des options (lecture json)
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