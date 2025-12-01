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
    }
    
    // Attaque dictionnaire + rules
    private void DictionaryAttackAndRulesClick(object sender, RoutedEventArgs e)
    {
        currentAttackType = AttackType.Rules;
        Console.WriteLine("[HashCat] Mode: Dictionary + Rules Attack");
    }
    
    // Attaque brute force
    private void BruteForceAttackClick(object sender, RoutedEventArgs e)
    {
        currentAttackType = AttackType.Mask;
        Console.WriteLine("[HashCat] Mode: Brute Force Attack");
    }
    
    // Attaque association
    private void AssociationAttackClick(object sender, RoutedEventArgs e)
    {
        currentAttackType = AttackType.Association;
        Console.WriteLine("[HashCat] Mode: Association Attack");
    }
    
    // Attaque prince
    private void PrinceAttackClick(object sender, RoutedEventArgs e)
    {
        currentAttackType = AttackType.Prince;
        Console.WriteLine("[HashCat] Mode: Prince Attack");
    }
    
    // Quand on change une selection
    private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        Console.WriteLine("[HashCat] Selection changed");
    }
    
    // Quand on tape dans le mask
    private void OnMaskInputChanged(object sender, Avalonia.Input.KeyEventArgs e)
    {
        Console.WriteLine("[HashCat] Mask input changed");
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

        TextBox sortieTextBox = this.FindControl<TextBox>("SortieTextBox");
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

