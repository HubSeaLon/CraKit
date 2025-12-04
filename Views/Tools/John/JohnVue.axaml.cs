using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
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
    private readonly HistoryService historyService;

    private CancellationTokenSource? _cts;
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
        MessageFile = this.FindControl<TextBlock>("MessageFile");
        var window = TopLevel.GetTopLevel(this) as Window;
        if (window is null) return;
        
        try
        {
            await toolFileService.PickAndUploadAsync(ToolFileModel.Wordlist, window);
            // Ajouter MessageBox pour avertir ou zone texte
            MessageFile!.Text = "Wordlist ajouté avec succès !";
            
            // Attendre 5 secondes sans bloquer l’UI
            await Task.Delay(5000);
            MessageFile.Text = "";
            ChargerLesListes();
        }
        catch (Exception ex)
        {
            MessageFile!.Text = "Erreur lors de l'upload wordlist !";
     
            await Task.Delay(5000);
            MessageFile.Text = "";
            Console.WriteLine(ex.Message);
        }
    }
    
    private async void AjouterHashfileClick(object? sender, RoutedEventArgs e)
    {
        MessageFile =  this.FindControl<TextBlock>("MessageFile");
        
        var window = TopLevel.GetTopLevel(this) as Window;
        if (window is null) return;

        try
        {
            await toolFileService.PickAndUploadAsync(ToolFileModel.HashFile, window);
            // Ajouter MessageBox pour avertir ? Ou bien une zone texte simple
            
            MessageFile!.Text = "Hashfile ajouté avec succès !";
            // Attendre 5 secondes sans bloquer l’UI
            await Task.Delay(5000);
            MessageFile.Text = "";
            
            ChargerLesListes();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            MessageFile!.Text = "Erreur lors de l'upload hashfile !";
            
            // Attendre 5 secondes sans bloquer l’UI
            await Task.Delay(5000);
            MessageFile.Text = "";
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
        BtnStop =  this.FindControl<Button>("BtnStop");
        
        BtnStop!.IsEnabled = false;
        
        EntreeTextBox = this.FindControl<TextBox>("EntreeTextBox");
        SortieTextBox = this.FindControl<TextBox>("SortieTextBox");

        WordlistComboBox!.IsVisible= false;
        HashfileComboBox!.IsVisible= false;
        FormatHashComboBox!.IsVisible= false;
        RuleComboBox!.IsVisible= false;
        MaskTextBox!.IsVisible= false;
    }
    
    
    // Fonction qui remplie les listes déroulantes
    private void RemplirComboBox(ComboBox laBox, string chemin)
    {
        try 
        {
            var ssh = ConnexionSshService.Instance.Client;

            // Si pas connecté, on arrête
            if (ssh == null || !ssh.IsConnected) return;
            
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
    }


    // Lancer la commande et afficher 
    private async void LancerCommandeClick(object? sender, RoutedEventArgs e)
    {
        var btnLancer = this.FindControl<Button>("BtnLancer");
        var btnStop = this.FindControl<Button>("BtnStop");
        
        // Nouveau token d’annulation
        _cts?.Cancel();
        _cts = new CancellationTokenSource();
        
        var cmd = commande.Trim();
        if (string.IsNullOrWhiteSpace(cmd)) return;
        
        SortieTextBox.Text = $"$ {cmd}\n";
        
        var stopwatch = Stopwatch.StartNew();
        var outputBuilder = new StringBuilder();
        
        btnLancer!.IsEnabled = false;
        btnStop!.IsEnabled   = true;
        
        try
        {
            // Laisser 1 min max si la commande met du temps à se lancer

            var outp = await executerCommandeService.ExecuteCommandAsync(cmd, TimeSpan.FromMinutes(1));
            SortieTextBox.Text += outp + "\n";
            outputBuilder.Append(outp);
            
            BtnStop.IsEnabled = false;
            BtnLancer.IsEnabled = true;
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
                
                var target = HashfileComboBox!.SelectionBoxItem?.ToString() ?? "No target";
                var username = ExtractJohnUsername(output, success);
                string format = FormatHashComboBox!.SelectionBoxItem?.ToString() ?? "No format";
                var result = ExtractJohnPassword(output, success);
                
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

    private string ExtractJohnPassword(string output, bool success)
    {
        if (string.IsNullOrWhiteSpace(output) || !success) return "No password found";

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

    private string ExtractJohnUsername(string output, bool success)
    {
        if (string.IsNullOrWhiteSpace(output) || !success) return "No username";

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
        MessageFile = this.FindControl<TextBlock>("MessageFile");
        var window = TopLevel.GetTopLevel(this) as Window;
        if (window is null) return;

        try
        {
            var success = await historyService.SaveHistoryToFileAsync(window, "John");
            
            if (success)
            {
                Console.WriteLine("[John] Historique sauvegardé avec succès !");
                // TODO: Afficher un message de confirmation à l'utilisateur
                
                MessageFile!.Text = "Historique de session enregistré !";
                await Task.Delay(5000);
                MessageFile.Text = "";
            }
            else
            {
                Console.WriteLine("[John] Aucun historique à sauvegarder ou annulé");
                MessageFile!.Text = "Aucun historique à sauvegarder !";
                await Task.Delay(5000);
                MessageFile.Text = "";
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[John] Erreur lors de la sauvegarde : {ex.Message}");
            MessageFile!.Text = "Erreur lors de la sauvegarder !";
            await Task.Delay(5000);
            MessageFile.Text = "";
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


    private void StopCommandeClick(object? sender, RoutedEventArgs e)
    {
        var btnLancer = this.FindControl<Button>("BtnLancer");
        var btnStop = this.FindControl<Button>("BtnStop");

        // Annuler le token
        _cts?.Cancel();
        executerCommandeService.StopCurrent();
        
        // Feedback UI
        if (SortieTextBox != null)
        {
            SortieTextBox.Text += "\n[Stop demandé - Processus john terminé]\n";
        }

        // Réactiver Lancer, désactiver Stop
        btnLancer!.IsEnabled = true;
        btnStop!.IsEnabled = false;
    }
    
    // aller chercher le style <Style Selector="control|TemplateControl">
    protected override Type StyleKeyOverride => typeof(TemplateControl);
}