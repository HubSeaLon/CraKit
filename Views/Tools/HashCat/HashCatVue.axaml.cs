using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using CraKit.Models;
using CraKit.Services;
using CraKit.Templates;
using Renci.SshNet;

namespace CraKit.Views.Tools.HashCat;

// Types d'attaque HashCat
public enum AttackType
{
    Dictionary,
    Rules,
    Mask,
    Association,
    Prince
}

// Vue pour l'outil HashCat
public partial class HashCatVue : TemplateControl
{
    // Services
    private ToolFileService toolFileService;
    private ExecuterCommandeService execService;
    private CancellationTokenSource cts;
    
    // Type d'attaque actuel
    private AttackType currentAttackType;

    // Constructeur
    public HashCatVue()
    {
        // Creer les services
        toolFileService = new ToolFileService(ConnexionSshService.Instance);
        execService = new ExecuterCommandeService(ConnexionSshService.Instance);
        
        currentAttackType = AttackType.Dictionary;
        cts = null;
        
        InitializeComponent();
        
        // Charger les donnees
        ChargerLesListes();
        ChargerHashTypes();
    }
    
    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
    
    // Charger les types de hash depuis JSON
    private void ChargerHashTypes()
    {
        try 
        {
            string chemin = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "HashCat.json");
            string jsonBrut = ToolBase.LireFichierTexte(chemin);

            if (jsonBrut == null || jsonBrut == "") return;
            
            JsonNode rootNode = JsonNode.Parse(jsonBrut);
            JsonNode valuesNode = rootNode["options"]["hashType"]["values"];

            if (valuesNode != null)
            {
                JsonSerializerOptions options = new JsonSerializerOptions();
                options.PropertyNameCaseInsensitive = true;
                
                System.Collections.Generic.List<OptionMode> listeModes = valuesNode.Deserialize<System.Collections.Generic.List<OptionMode>>(options);
                
                ComboBox boxType = this.FindControl<ComboBox>("HashTypeComboBox");
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
    
    // Charger les listes
    private void ChargerLesListes()
    {
        ComboBox boxWordlist = this.FindControl<ComboBox>("WordlistComboBox");
        ComboBox boxHashfile = this.FindControl<ComboBox>("HashfileComboBox");
        ComboBox boxRules = this.FindControl<ComboBox>("RulesComboBox");

        if (boxWordlist != null)
        {
            RemplirComboBox(boxWordlist, "/root/wordlists");
            boxWordlist.Items.Add("rockyou.txt");
        }
        if (boxHashfile != null)
        {
            RemplirComboBox(boxHashfile, "/root/hashfiles");
        }
        if (boxRules != null)
        {
            RemplirComboBox(boxRules, "/usr/share/hashcat/rules");
        }
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
            ChargerLesListes();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
    
    // Lancer la commande (version simplifiee sans streaming)
    private async void LancerCommandeClick(object sender, RoutedEventArgs e)
    {
        TextBox entreeTextBox = this.FindControl<TextBox>("EntreeTextBox");
        TextBox sortieTextBox = this.FindControl<TextBox>("SortieTextBox");
        
        if (entreeTextBox == null || sortieTextBox == null) return;
        
        string cmd = entreeTextBox.Text;
        if (cmd == null || cmd.Trim() == "") return;
        
        sortieTextBox.Text = "$ " + cmd + "\n";
        
        string outp = await execService.ExecuteCommandAsync(cmd, TimeSpan.FromMinutes(10));
        sortieTextBox.Text += outp + "\n";
    }
    
    // Attaque dictionnaire
    private void DictionaryAttackClick(object sender, RoutedEventArgs e)
    {
        currentAttackType = AttackType.Dictionary;
        Console.WriteLine("[HashCat] Mode: Dictionary Attack");
        
        // Reinitialiser les styles
        ResetButtonStyles();
        Button btn = sender as Button;
        if (btn != null)
        {
            btn.BorderBrush = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#3B82F6"));
            btn.Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#1E40AF"));
        }
        
        // Afficher les bons controles
        ComboBox wordlistBox = this.FindControl<ComboBox>("WordlistComboBox");
        ComboBox hashfileBox = this.FindControl<ComboBox>("HashfileComboBox");
        ComboBox hashTypeBox = this.FindControl<ComboBox>("HashTypeComboBox");
        ComboBox rulesBox = this.FindControl<ComboBox>("RulesComboBox");
        TextBox maskBox = this.FindControl<TextBox>("MaskInputBox");
        
        if (wordlistBox != null) wordlistBox.IsVisible = true;
        if (hashfileBox != null) hashfileBox.IsVisible = true;
        if (hashTypeBox != null) hashTypeBox.IsVisible = true;
        if (rulesBox != null) rulesBox.IsVisible = false;
        if (maskBox != null) maskBox.IsVisible = false;
        
        UpdateCommande();
    }
    
    // Attaque dictionnaire + rules
    private void DictionaryAttackAndRulesClick(object sender, RoutedEventArgs e)
    {
        currentAttackType = AttackType.Rules;
        Console.WriteLine("[HashCat] Mode: Dictionary + Rules Attack");
        
        // Reinitialiser les styles
        ResetButtonStyles();
        Button btn = sender as Button;
        if (btn != null)
        {
            btn.BorderBrush = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#3B82F6"));
            btn.Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#1E40AF"));
        }
        
        // Afficher les bons controles
        ComboBox wordlistBox = this.FindControl<ComboBox>("WordlistComboBox");
        ComboBox hashfileBox = this.FindControl<ComboBox>("HashfileComboBox");
        ComboBox hashTypeBox = this.FindControl<ComboBox>("HashTypeComboBox");
        ComboBox rulesBox = this.FindControl<ComboBox>("RulesComboBox");
        TextBox maskBox = this.FindControl<TextBox>("MaskInputBox");
        
        if (wordlistBox != null) wordlistBox.IsVisible = true;
        if (hashfileBox != null) hashfileBox.IsVisible = true;
        if (hashTypeBox != null) hashTypeBox.IsVisible = true;
        if (rulesBox != null) rulesBox.IsVisible = true;
        if (maskBox != null) maskBox.IsVisible = false;
        
        UpdateCommande();
    }
    
    // Attaque brute force
    private void BruteForceAttackClick(object sender, RoutedEventArgs e)
    {
        currentAttackType = AttackType.Mask;
        Console.WriteLine("[HashCat] Mode: Brute Force Attack");
        
        // Reinitialiser les styles
        ResetButtonStyles();
        Button btn = sender as Button;
        if (btn != null)
        {
            btn.BorderBrush = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#3B82F6"));
            btn.Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#1E40AF"));
        }
        
        // Afficher les bons controles
        ComboBox wordlistBox = this.FindControl<ComboBox>("WordlistComboBox");
        ComboBox hashfileBox = this.FindControl<ComboBox>("HashfileComboBox");
        ComboBox hashTypeBox = this.FindControl<ComboBox>("HashTypeComboBox");
        ComboBox rulesBox = this.FindControl<ComboBox>("RulesComboBox");
        TextBox maskBox = this.FindControl<TextBox>("MaskInputBox");
        
        if (wordlistBox != null) wordlistBox.IsVisible = false;
        if (hashfileBox != null) hashfileBox.IsVisible = true;
        if (hashTypeBox != null) hashTypeBox.IsVisible = true;
        if (rulesBox != null) rulesBox.IsVisible = false;
        if (maskBox != null) maskBox.IsVisible = true;
        
        UpdateCommande();
    }
    
    // Attaque association
    private void AssociationAttackClick(object sender, RoutedEventArgs e)
    {
        currentAttackType = AttackType.Association;
        Console.WriteLine("[HashCat] Mode: Association Attack");
        
        // Reinitialiser les styles
        ResetButtonStyles();
        Button btn = sender as Button;
        if (btn != null)
        {
            btn.BorderBrush = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#3B82F6"));
            btn.Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#1E40AF"));
        }
        
        // Afficher les bons controles
        ComboBox wordlistBox = this.FindControl<ComboBox>("WordlistComboBox");
        ComboBox hashfileBox = this.FindControl<ComboBox>("HashfileComboBox");
        ComboBox hashTypeBox = this.FindControl<ComboBox>("HashTypeComboBox");
        ComboBox rulesBox = this.FindControl<ComboBox>("RulesComboBox");
        TextBox maskBox = this.FindControl<TextBox>("MaskInputBox");
        
        if (wordlistBox != null) wordlistBox.IsVisible = true;
        if (hashfileBox != null) hashfileBox.IsVisible = true;
        if (hashTypeBox != null) hashTypeBox.IsVisible = true;
        if (rulesBox != null) rulesBox.IsVisible = false;
        if (maskBox != null) maskBox.IsVisible = false;
        
        UpdateCommande();
    }
    
    // Attaque prince
    private void PrinceAttackClick(object sender, RoutedEventArgs e)
    {
        currentAttackType = AttackType.Prince;
        Console.WriteLine("[HashCat] Mode: Prince Attack");
        
        // Reinitialiser les styles
        ResetButtonStyles();
        Button btn = sender as Button;
        if (btn != null)
        {
            btn.BorderBrush = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#3B82F6"));
            btn.Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#1E40AF"));
        }
        
        // Afficher les bons controles
        ComboBox wordlistBox = this.FindControl<ComboBox>("WordlistComboBox");
        ComboBox hashfileBox = this.FindControl<ComboBox>("HashfileComboBox");
        ComboBox hashTypeBox = this.FindControl<ComboBox>("HashTypeComboBox");
        ComboBox rulesBox = this.FindControl<ComboBox>("RulesComboBox");
        TextBox maskBox = this.FindControl<TextBox>("MaskInputBox");
        
        if (wordlistBox != null) wordlistBox.IsVisible = true;
        if (hashfileBox != null) hashfileBox.IsVisible = true;
        if (hashTypeBox != null) hashTypeBox.IsVisible = true;
        if (rulesBox != null) rulesBox.IsVisible = false;
        if (maskBox != null) maskBox.IsVisible = false;
        
        UpdateCommande();
    }
    
    // Reinitialiser les styles des boutons
    private void ResetButtonStyles()
    {
        Button btn1 = this.FindControl<Button>("ButtonOption1");
        Button btn2 = this.FindControl<Button>("ButtonOption2");
        Button btn3 = this.FindControl<Button>("ButtonOption3");
        Button btn4 = this.FindControl<Button>("ButtonOption4");
        Button btn5 = this.FindControl<Button>("ButtonOption5");
        
        if (btn1 != null)
        {
            btn1.BorderBrush = null;
            btn1.Background = null;
        }
        if (btn2 != null)
        {
            btn2.BorderBrush = null;
            btn2.Background = null;
        }
        if (btn3 != null)
        {
            btn3.BorderBrush = null;
            btn3.Background = null;
        }
        if (btn4 != null)
        {
            btn4.BorderBrush = null;
            btn4.Background = null;
        }
        if (btn5 != null)
        {
            btn5.BorderBrush = null;
            btn5.Background = null;
        }
    }
    
    // Mettre a jour la commande
    private void UpdateCommande()
    {
        ComboBox wordlistBox = this.FindControl<ComboBox>("WordlistComboBox");
        ComboBox hashfileBox = this.FindControl<ComboBox>("HashfileComboBox");
        ComboBox hashTypeBox = this.FindControl<ComboBox>("HashTypeComboBox");
        ComboBox rulesBox = this.FindControl<ComboBox>("RulesComboBox");
        TextBox maskBox = this.FindControl<TextBox>("MaskInputBox");
        TextBox entreeBox = this.FindControl<TextBox>("EntreeTextBox");
        
        if (entreeBox == null) return;
        
        string commande = "hashcat";
        
        // Mode d'attaque
        switch (currentAttackType)
        {
            case AttackType.Dictionary:
                commande += " -a 0"; // Dictionary attack
                break;
            case AttackType.Rules:
                commande += " -a 0"; // Dictionary + rules
                break;
            case AttackType.Mask:
                commande += " -a 3"; // Brute-force
                break;
            case AttackType.Association:
                commande += " -a 1"; // Combination
                break;
            case AttackType.Prince:
                commande += " -a 9"; // Prince
                break;
        }
        
        // Type de hash
        if (hashTypeBox != null && hashTypeBox.SelectedItem != null)
        {
            OptionMode option = hashTypeBox.SelectedItem as OptionMode;
            if (option != null)
            {
                commande += " -m " + option.value;
            }
        }
        
        // Hashfile
        if (hashfileBox != null && hashfileBox.SelectedItem != null)
        {
            commande += " /root/hashfiles/" + hashfileBox.SelectedItem.ToString();
        }
        
        // Wordlist (pour Dictionary, Rules, Association, Prince)
        if (currentAttackType == AttackType.Dictionary || 
            currentAttackType == AttackType.Rules ||
            currentAttackType == AttackType.Association ||
            currentAttackType == AttackType.Prince)
        {
            if (wordlistBox != null && wordlistBox.SelectedItem != null)
            {
                commande += " /root/wordlists/" + wordlistBox.SelectedItem.ToString();
            }
        }
        
        // Rules (pour Rules)
        if (currentAttackType == AttackType.Rules)
        {
            if (rulesBox != null && rulesBox.SelectedItem != null)
            {
                commande += " -r /root/rules/" + rulesBox.SelectedItem.ToString();
            }
        }
        
        // Mask (pour Brute-force)
        if (currentAttackType == AttackType.Mask)
        {
            if (maskBox != null && maskBox.Text != null && maskBox.Text.Trim() != "")
            {
                commande += " " + maskBox.Text.Trim();
            }
        }
        
        entreeBox.Text = commande;
    }
    
    // Quand on change une selection
    private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        Console.WriteLine("[HashCat] Selection changed");
        UpdateCommande();
    }
    
    // Quand on tape dans le mask
    private void OnMaskInputChanged(object sender, Avalonia.Input.KeyEventArgs e)
    {
        Console.WriteLine("[HashCat] Mask input changed");
        UpdateCommande();
    }
    
    // Arreter la commande
    private void StopCommandeClick(object sender, RoutedEventArgs e)
    {
        if (cts != null)
        {
            cts.Cancel();
        }
        
        execService.StopCurrent();
        
        try
        {
            SshClient ssh = ConnexionSshService.Instance.Client;
            if (ssh != null && ssh.IsConnected)
            {
                var killCmd = ssh.CreateCommand("pkill -9 hashcat");
                killCmd.Execute();
            }
        }
        catch
        {
            // Ignorer les erreurs
        }

        TextBox sortieTextBox = this.FindControl<TextBox>("TxtOutput");
        if (sortieTextBox != null)
        {
            sortieTextBox.Text += "\n[Stop demande - Processus hashcat termines]\n";
        }
    }
    
    // Style du template
    protected override Type StyleKeyOverride
    {
        get { return typeof(TemplateControl); }
    }
}

