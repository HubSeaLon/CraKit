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
using Renci.SshNet;

namespace CraKit.Views.Tools.John;

// Vue pour l'outil John The Ripper
public partial class JohnVue : TemplateControl
{
    // Variables pour construire la commande
    private string commande;
    private string hashfile;
    private string wordlist;
    private string format;
    private string rule;
    private string mask;
    private bool hashidSelected;
    
    // Services
    private ToolFileService toolFileService;
    private ExecuterCommandeService executerCommandeService;

    // Constructeur
    public JohnVue()
    {
        InitializeComponent();
        
        // Initialiser les variables
        commande = "";
        hashfile = "";
        wordlist = "";
        format = "";
        rule = "";
        mask = "";
        hashidSelected = false;
        
        // Creer les services
        toolFileService = new ToolFileService(ConnexionSshService.Instance);
        executerCommandeService = new ExecuterCommandeService(ConnexionSshService.Instance);
        
        AttachedToVisualTree += OnAttachedToVisualTree;
        
        // Charger les listes
        ChargerLesListes();
        ChargerHashTypes();
        ChargerFormatTypes();
    }
    
    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    // Quand on clique sur une option
    private void choixOptionClick(object sender, RoutedEventArgs e)
    {
        Button btn = sender as Button;
        if (btn == null) return;
        
        string name = btn.Name;
        
        ResetButtonStyles();
        
        if (name == "ButtonOption1")
        {
            // Identifier format hash (hashfile)
            hashidSelected = true;
            ButtonOption1.Opacity = 0.4;
            
            WordlistComboBox.IsVisible = false;
            HashfileComboBox.IsVisible = true;
            FormatHashComboBox.IsVisible = false;
            RuleComboBox.IsVisible = false;
            MaskTextBox.IsVisible = false;
        }
        else if (name == "ButtonOption2")
        {
            // Crack auto (wordlist, hashfile)
            hashidSelected = false;
            ButtonOption2.Opacity = 0.4;
            
            WordlistComboBox.IsVisible = true;
            HashfileComboBox.IsVisible = true;
            FormatHashComboBox.IsVisible = false;
            RuleComboBox.IsVisible = false;
            MaskTextBox.IsVisible = false;
        }
        else if (name == "ButtonOption3")
        {
            // Crack hash (wordlist, format, hashfile)
            hashidSelected = false;
            ButtonOption3.Opacity = 0.4;
            
            WordlistComboBox.IsVisible = true;
            HashfileComboBox.IsVisible = true;
            FormatHashComboBox.IsVisible = true;
            RuleComboBox.IsVisible = false;
            MaskTextBox.IsVisible = false;
        }
        else if (name == "ButtonOption4")
        {
            // Crack hash rule (wordlist, format, hashfile, rule)
            hashidSelected = false;
            ButtonOption4.Opacity = 0.4;
            
            WordlistComboBox.IsVisible = true;
            HashfileComboBox.IsVisible = true;
            FormatHashComboBox.IsVisible = true;
            RuleComboBox.IsVisible = true;
            MaskTextBox.IsVisible = false;
        }
        else if (name == "ButtonOption5")
        {
            // mask attack (wordlist, format, hashfile, mask)
            hashidSelected = false;
            ButtonOption5.Opacity = 0.4;
            
            WordlistComboBox.IsVisible = true;
            HashfileComboBox.IsVisible = true;
            FormatHashComboBox.IsVisible = true;
            RuleComboBox.IsVisible = false;
            MaskTextBox.IsVisible = true;
        }
    }
    
    // Remettre tout a zero
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
        FormatHashComboBox.SelectedItem = null;

        WordlistComboBox.SelectedIndex = -1;
        WordlistComboBox.SelectedItem = null;

        HashfileComboBox.SelectedIndex = -1;
        HashfileComboBox.SelectedItem = null;

        RuleComboBox.SelectedIndex = -1;
        RuleComboBox.SelectedItem = null;
        
        MaskTextBox.Text = "";
    }
    
    // Ajouter une wordlist
    private async void AjouterWordlistClick(object sender, RoutedEventArgs e)
    {
        Window window = TopLevel.GetTopLevel(this) as Window;
        if (window == null) return;
        
        try
        {
            await toolFileService.PickAndUploadAsync(ToolFileModel.Wordlist, window);
            ChargerLesListes();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
    
    // Ajouter un hashfile
    private async void AjouterHashfileClick(object sender, RoutedEventArgs e)
    {
        Window window = TopLevel.GetTopLevel(this) as Window;
        if (window == null) return;

        try
        {
            await toolFileService.PickAndUploadAsync(ToolFileModel.HashFile, window);
            ChargerHashTypes();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
    
    // Recuperer les controles
    private void OnAttachedToVisualTree(object sender, VisualTreeAttachmentEventArgs e)
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
        
        EntreeTextBox = this.FindControl<TextBox>("EntreeTextBox");
        SortieTextBox = this.FindControl<TextBox>("SortieTextBox");

        WordlistComboBox.IsVisible = false;
        HashfileComboBox.IsVisible = false;
        FormatHashComboBox.IsVisible = false;
        RuleComboBox.IsVisible = false;
        MaskTextBox.IsVisible = false;
    }
    
    // Remplir une ComboBox avec les fichiers d'un dossier
    private void RemplirComboBox(ComboBox laBox, string chemin)
    {
        if (laBox == null) return;

        try 
        {
            SshClient ssh = ConnexionSshService.Instance.Client;

            if (ssh == null || !ssh.IsConnected) return;

            var cmd = ssh.CreateCommand("ls -1 " + chemin);
            string resultat = cmd.Execute();

            laBox.Items.Clear();
            
            if (resultat != null && resultat.Trim() != "" && !resultat.Contains("No such file"))
            {
                string[] fichiers = resultat.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                
                for (int i = 0; i < fichiers.Length; i++)
                {
                    laBox.Items.Add(fichiers[i]);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Erreur chargement liste : " + ex.Message);
        }
    }
    
    // Quand on change le mask
    private void OnChangedText(object sender, TextChangedEventArgs e)
    {
        TextBox textBox = sender as TextBox;
        if (textBox == null) return;
        
        if (textBox.Name == "MaskTextBox")
        {
            if (MaskTextBox.Text == null || MaskTextBox.Text.Trim() == "")
            {
                mask = "";
            }
            else
            {
                mask = " --mask='" + MaskTextBox.Text + "'";
            }
        }
        
        commande = "john" + wordlist + mask + format + rule + hashfile;
        
        EntreeTextBox.Text = commande;
        Console.WriteLine("commande : " + commande);
    }
    
    // Quand on change une liste deroulante
    private void OnChangedList(object sender, SelectionChangedEventArgs e)
    {
        ComboBox comboBox = sender as ComboBox;
        if (comboBox == null) return;
        
        string name = comboBox.Name;
        
        if (name == "WordlistComboBox")
        {
            if (WordlistComboBox.SelectedItem == null)
            {
                wordlist = "";
            }
            else if (WordlistComboBox.SelectedItem.ToString() == "rockyou.txt")
            {
                wordlist = " --wordlist=/usr/share/wordlists/rockyou.txt";
            }
            else
            {  
                wordlist = " --wordlist=/root/wordlists/" + WordlistComboBox.SelectedItem.ToString();
            }
        }
        else if (name == "HashfileComboBox")
        {
            if (HashfileComboBox.SelectedItem == null)
            {
                hashfile = "";
            }
            else
            {
                hashfile = " /root/hashfiles/" + HashfileComboBox.SelectedItem.ToString();
            }
        }
        else if (name == "FormatHashComboBox")
        {
            if (FormatHashComboBox.SelectedItem == null)
            {
                format = "";
            }
            else
            {
                format = " --format=" + FormatHashComboBox.SelectedItem.ToString();
            }
        }
        else if (name == "RuleComboBox")
        {
            if (RuleComboBox.SelectedItem == null)
            {
                rule = "";
            }
            else
            {
                rule = " --rules=" + RuleComboBox.SelectedItem.ToString();
            }
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
        Console.WriteLine("commande : " + commande);
    }

    // Lancer la commande
    private async void LancerCommandeClick(object sender, RoutedEventArgs e)
    {
        string cmd = commande.Trim();
        if (cmd == null || cmd == "") return;
        
        SortieTextBox.Text = "$ " + cmd + "\n";
        
        string outp = await executerCommandeService.ExecuteCommandAsync(cmd, TimeSpan.FromMinutes(5));
        SortieTextBox.Text += outp + "\n";
    }
    
    // Charger les listes
    private void ChargerLesListes()
    {
        ComboBox boxWordlist = this.FindControl<ComboBox>("WordlistComboBox");
        ComboBox boxHashfile = this.FindControl<ComboBox>("HashfileComboBox");

        if (boxWordlist != null)
        {
            RemplirComboBox(boxWordlist, "/root/wordlists");
            boxWordlist.Items.Add("rockyou.txt");
        }
        if (boxHashfile != null)
        {
            RemplirComboBox(boxHashfile, "/root/hashfiles");
        }
    }
    
    // Charger les types de hash depuis JSON
    private void ChargerHashTypes()
    {
        try 
        {
            string chemin = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "john_options.json");
            string jsonBrut = ToolBase.LireFichierTexte(chemin);

            if (jsonBrut == null || jsonBrut == "") return;
            
            JsonNode rootNode = JsonNode.Parse(jsonBrut);
            
            JsonNode valuesNode = rootNode["options"]["format"]["values"];

            if (valuesNode != null)
            {
                JsonSerializerOptions options = new JsonSerializerOptions();
                options.PropertyNameCaseInsensitive = true;
                
                System.Collections.Generic.List<OptionMode> listeModes = valuesNode.Deserialize<System.Collections.Generic.List<OptionMode>>(options);
                
                ComboBox boxType = this.FindControl<ComboBox>("FormatHashComboBox");
                if (listeModes != null && boxType != null)
                {
                    boxType.ItemsSource = listeModes;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("[JSON ERROR] " + ex.Message);
        }
    }
    
    // Charger les formats depuis JSON
    private void ChargerFormatTypes()
    {
        try 
        {
            string chemin = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "john_options.json");
            string jsonBrut = ToolBase.LireFichierTexte(chemin);

            if (jsonBrut == null || jsonBrut == "") return;
            
            JsonNode rootNode = JsonNode.Parse(jsonBrut);
            
            JsonNode valuesNode = rootNode["options"]["rules"]["values"];

            if (valuesNode != null)
            {
                JsonSerializerOptions options = new JsonSerializerOptions();
                options.PropertyNameCaseInsensitive = true;
                
                System.Collections.Generic.List<OptionMode> listeModes = valuesNode.Deserialize<System.Collections.Generic.List<OptionMode>>(options);
                
                ComboBox boxRules = this.FindControl<ComboBox>("RuleComboBox");
                if (listeModes != null && boxRules != null)
                {
                    boxRules.ItemsSource = listeModes;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("[JSON ERROR] " + ex.Message);
        }
    }
    
    // Style du template
    protected override Type StyleKeyOverride
    {
        get { return typeof(TemplateControl); }
    }
}

