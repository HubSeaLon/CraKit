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

namespace CraKit.Views.Tools.Hydra;

public partial class HydraVue : TemplateControl
{
    // Initialisation des variables 
    private string commande = "";
    private string target = "";
    private string wordlist = "";
    private string protocol = "";
    private string mode = "";
    private string threads = " -t 16"; // threads par défaut
    private string verbose = " -vV"; // mode verbose activé par défaut
    private string username = "";
    private string userlist = "";
    private string combolist = "";
    
    private readonly ToolFileService toolFileService;
    private readonly ExecuterCommandeService executerCommandeService;

    public HydraVue()
    {
        InitializeComponent();
        
        // Injection des Instances 
        toolFileService = new ToolFileService(ConnexionSshService.Instance);
        executerCommandeService = new ExecuterCommandeService(ConnexionSshService.Instance);
        
        AttachedToVisualTree += OnAttachedToVisualTree;
        
        // Chargement des listes déroulantes
        ChargerLesListes();
        ChargerProtocoles();
        ChargerThreads();
    }
    
    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    // Affichage selon l'option choisie
    private void choixOptionClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button btn) return; 
        
        var name = btn.Name;
        
        ResetButtonStyles();
        
        switch (name)
        {
            case "ButtonOption1":
                // Mode: Single user + wordlist
                mode = "single";
                
                ButtonOption1.Opacity = 0.4;
                
                UsernameTextBox!.IsVisible = true;
                UserlistComboBox!.IsVisible = false;
                CombolistComboBox!.IsVisible = false;
                WordlistComboBox!.IsVisible = true;
                break;
            
            case "ButtonOption2":
                // Mode: User list + wordlist
                mode = "userlist";
          
                ButtonOption2.Opacity = 0.4;
                
                UsernameTextBox!.IsVisible = false;
                UserlistComboBox!.IsVisible = true;
                CombolistComboBox!.IsVisible = false;
                WordlistComboBox!.IsVisible = true;
                break; 
            
            case "ButtonOption3":
                // Mode: Combo list (user:pass)
                mode = "combo";
       
                ButtonOption3.Opacity = 0.4;
                
                UsernameTextBox!.IsVisible = false;
                UserlistComboBox!.IsVisible = false;
                CombolistComboBox!.IsVisible = true;
                WordlistComboBox!.IsVisible = false;
                break;
        }
        
        UpdateCommande();
    }
    
    // Reset visuel et fonctionnel 
    private void ResetButtonStyles()
    {
        ButtonOption1.Opacity = 1;
        ButtonOption2.Opacity = 1;
        ButtonOption3.Opacity = 1;

        protocol = "";
        wordlist = "";
        username = "";
        userlist = "";
        combolist = "";

        ProtocolComboBox.SelectedIndex = -1;
        ProtocolComboBox.SelectedItem = null;

        WordlistComboBox.SelectedIndex = -1;
        WordlistComboBox.SelectedItem = null;

        UserlistComboBox.SelectedIndex = -1;
        UserlistComboBox.SelectedItem = null;

        CombolistComboBox.SelectedIndex = -1;
        CombolistComboBox.SelectedItem = null;

        ThreadsComboBox.SelectedIndex = -1;
        ThreadsComboBox.SelectedItem = null;

        UsernameTextBox.Text = "";
        TargetTextBox.Text = "";
    }
    
    
    // Ajout des wordlists et userlists
    private async void AjouterWordlistClick(object? sender, RoutedEventArgs e)
    {
        var window = TopLevel.GetTopLevel(this) as Window;
        if (window is null) return;
        
        try
        {
            await toolFileService.PickAndUploadAsync(ToolFileModel.Wordlist, window);
            ChargerLesListes(); // Recharger après upload
            Console.WriteLine("Wordlist uploaded!");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
    
    private async void AjouterUserlistClick(object? sender, RoutedEventArgs e)
    {
        var window = TopLevel.GetTopLevel(this) as Window;
        if (window is null) return;

        try
        {
            await toolFileService.PickAndUploadAsync(ToolFileModel.Userlist, window);
            ChargerLesListes(); // Recharger après upload
            Console.WriteLine("Userlist uploaded!");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
    
    
    // Méthode permettant de récupérer les noms des boutons, listes, etc. vu qu'on utilise un template
    private void OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        WordlistComboBox = this.FindControl<ComboBox>("WordlistComboBox");
        UserlistComboBox = this.FindControl<ComboBox>("UserlistComboBox");
        CombolistComboBox = this.FindControl<ComboBox>("CombolistComboBox");
        ProtocolComboBox = this.FindControl<ComboBox>("ProtocolComboBox");
        ThreadsComboBox = this.FindControl<ComboBox>("ThreadsComboBox");
        
        UsernameTextBox = this.FindControl<TextBox>("UsernameTextBox");
        TargetTextBox = this.FindControl<TextBox>("TargetTextBox");
        
        ButtonOption1 = this.FindControl<Button>("ButtonOption1");
        ButtonOption2 = this.FindControl<Button>("ButtonOption2");
        ButtonOption3 = this.FindControl<Button>("ButtonOption3");
        
        EntreeTextBox = this.FindControl<TextBox>("EntreeTextBox");
        SortieTextBox = this.FindControl<TextBox>("SortieTextBox");

        WordlistComboBox!.IsVisible = false;
        UserlistComboBox!.IsVisible = false;
        CombolistComboBox!.IsVisible = false;
        UsernameTextBox!.IsVisible = false;
    }
    
    
    // Fonction qui fait le travail (LS en SSH)
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
    
    // Modification du texte (Target et Username)
    private void OnChangedText(object? sender, TextChangedEventArgs e)
    {
        if (sender is not TextBox textBox) return; 
        
        if (textBox.Name == "TargetTextBox")
        {
            target = string.IsNullOrWhiteSpace(TargetTextBox.Text) ? "" : " " + TargetTextBox.Text;
        }
        else if (textBox.Name == "UsernameTextBox")
        {
            username = string.IsNullOrWhiteSpace(UsernameTextBox.Text) ? "" : " -l " + UsernameTextBox.Text;
        }
        
        UpdateCommande();
    }
    
    // Gestion du mode verbose
    private void OnVerboseChanged(object? sender, RoutedEventArgs e)
    {
        if (sender is not CheckBox checkBox) return;
        
        verbose = checkBox.IsChecked == true ? " -vV" : "";
        UpdateCommande();
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
                    wordlist = " -P /usr/share/wordlists/rockyou.txt";
                }
                else
                {  
                    wordlist = " -P /root/wordlists/" + WordlistComboBox.SelectedItem!;
                }
                break;
            
            case "UserlistComboBox":
                if (UserlistComboBox.SelectedItem is null)
                {
                    userlist = "";
                    break;
                }
                
                // Userlists sont dans /root/userlists
                userlist = " -L /root/userlists/" + UserlistComboBox.SelectedItem!;
                break;
            
            case "CombolistComboBox":
                if (CombolistComboBox.SelectedItem is null)
                {
                    combolist = "";
                    break;
                }
                
                if (CombolistComboBox.SelectedItem!.ToString() == "rockyou.txt")
                {
                    combolist = " -C /usr/share/wordlists/rockyou.txt";
                }
                else
                {
                    combolist = " -C /root/wordlists/" + CombolistComboBox.SelectedItem!;
                }
                break;
            
            case "ProtocolComboBox":
                if (ProtocolComboBox.SelectedItem is null)
                {
                    protocol = "";
                    break;
                }
                
                var selectedProtocol = ProtocolComboBox.SelectedItem as OptionMode;
                protocol = selectedProtocol != null ? " " + selectedProtocol.value : "";
                break;
            
            case "ThreadsComboBox":
                if (ThreadsComboBox.SelectedItem is null)
                {
                    threads = " -t 16";
                    break;
                }
                
                var selectedThreads = ThreadsComboBox.SelectedItem as OptionMode;
                threads = selectedThreads != null ? " -t " + selectedThreads.value : " -t 16";
                break;
        }
        
        UpdateCommande();
    }

    // Mise à jour de la commande
    private void UpdateCommande()
    {
        switch (mode)
        {
            case "single":
                commande = "hydra" + username + wordlist + threads + verbose + target + protocol;
                break;
            case "userlist":
                commande = "hydra" + userlist + wordlist + threads + verbose + target + protocol;
                break;
            case "combo":
                commande = "hydra" + combolist + threads + verbose + target + protocol;
                break;
            default:
                commande = "hydra";
                break;
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
        
        // Tolérance de 10 min pour des attaques longues
        var outp = await executerCommandeService.ExecuteCommandAsync(cmd, TimeSpan.FromMinutes(10));
        SortieTextBox.Text += outp + "\n";
    }
    
    // Chargement des différentes listes déroulantes
    private void ChargerLesListes()
    {
        var boxWordlist = this.FindControl<ComboBox>("WordlistComboBox");
        var boxUserlist = this.FindControl<ComboBox>("UserlistComboBox");
        var boxCombolist = this.FindControl<ComboBox>("CombolistComboBox");

        if (boxWordlist != null)
        {
            RemplirComboBox(boxWordlist, "/root/wordlists");
            boxWordlist.Items.Add("rockyou.txt");
        }
        if (boxUserlist != null)
        {
            RemplirComboBox(boxUserlist, "/root/wordlists");
            boxUserlist.Items.Add("rockyou.txt");
        }
        if (boxCombolist != null)
        {
            RemplirComboBox(boxCombolist, "/root/wordlists");
            boxCombolist.Items.Add("rockyou.txt");
        }
    }
    
    private void ChargerProtocoles()
    {
        try 
        {
            string chemin = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "hydra_options.json");
            string? jsonBrut = ToolBase.LireFichierTexte(chemin);

            if (string.IsNullOrEmpty(jsonBrut)) return;
            
            var rootNode = JsonNode.Parse(jsonBrut);
            
            var valuesNode = rootNode?["options"]?["protocol"]?["values"];

            if (valuesNode != null)
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var listeModes = valuesNode.Deserialize<System.Collections.Generic.List<OptionMode>>(options);
                
                var boxProtocol = this.FindControl<ComboBox>("ProtocolComboBox");
                if (listeModes != null && boxProtocol != null) boxProtocol.ItemsSource = listeModes;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[JSON ERROR] {ex.Message}");
        }
    }
    
    private void ChargerThreads()
    {
        try 
        {
            string chemin = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "hydra_options.json");
            string? jsonBrut = ToolBase.LireFichierTexte(chemin);

            if (string.IsNullOrEmpty(jsonBrut)) return;
            
            var rootNode = JsonNode.Parse(jsonBrut);
            
            var valuesNode = rootNode?["options"]?["threads"]?["values"];

            if (valuesNode != null)
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var listeModes = valuesNode.Deserialize<System.Collections.Generic.List<OptionMode>>(options);
                
                var boxThreads = this.FindControl<ComboBox>("ThreadsComboBox");
                if (listeModes != null && boxThreads != null) boxThreads.ItemsSource = listeModes;
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

