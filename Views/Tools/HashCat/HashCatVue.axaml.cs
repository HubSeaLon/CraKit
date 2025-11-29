using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes; 
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using CraKit.Models;
using CraKit.Services;
using CraKit.Templates; 
using System.Threading;
using Avalonia.Threading;

namespace CraKit.Views.Tools.HashCat;

// Definition des modes d'attaque supportes
public enum AttackType
{
    Dictionary,
    Rules,
    Mask,
    Association,
    Prince
}

public partial class HashCatVue : TemplateControl
{
    private readonly ToolFileService _toolFileService;
    private AttackType _currentAttackType = AttackType.Dictionary;
    
    private readonly ExecuterCommandeService _execService;
    private CancellationTokenSource? _cts;

    public HashCatVue()
    {
        // Injection des instances
        _toolFileService = new ToolFileService(ConnexionSshService.Instance);
        _execService = new ExecuterCommandeService(ConnexionSshService.Instance);
        
        InitializeComponent();
        
        // Chargement initial des donnees
        ChargerLesListes();
        ChargerHashTypes();
    }
    
    
    private void ChargerHashTypes()
    {
        try 
        {
            // Lecture du fichier JSON contenant les types de hash (MD5, SHA1...)
            string chemin = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "HashCat.json");
            string? jsonBrut = ToolBase.LireFichierTexte(chemin);

            if (string.IsNullOrEmpty(jsonBrut)) return;
            
            var rootNode = JsonNode.Parse(jsonBrut);
            var valuesNode = rootNode?["options"]?["hashType"]?["values"];

            if (valuesNode != null)
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var listeModes = valuesNode.Deserialize<List<OptionMode>>(options);
                
                var boxType = this.FindControl<ComboBox>("HashTypeComboBox");
                if (listeModes != null && boxType != null)
                {
                    boxType.ItemsSource = listeModes;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[JSON ERROR] {ex.Message}");
        }
    }
    
    // Recupere la liste des fichiers sur le serveur via SSH
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

            // On verifie qu'il y a bien des fichiers et pas d'erreur Linux
            if (!string.IsNullOrWhiteSpace(resultat) && !resultat.Contains("No such file"))
            {
                var fichiers = resultat.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var f in fichiers) laBox.Items.Add(f);
            }
        }
        catch (Exception ex) { Console.WriteLine($"[SSH] Erreur : {ex.Message}"); }
    }

    private void ChargerLesListes()
    {
        var boxWordlist = this.FindControl<ComboBox>("WordlistComboBox");
        var boxHashfile = this.FindControl<ComboBox>("HashfileComboBox");
        var boxRules = this.FindControl<ComboBox>("RulesComboBox");

        // Remplissage des ComboBox avec les chemins distants
        
        if (boxWordlist != null) RemplirComboBox(boxWordlist, "/root/wordlists");
        if (boxHashfile != null) RemplirComboBox(boxHashfile, "/root/hashfiles");
        if (boxRules != null) RemplirComboBox(boxRules, "/usr/share/hashcat/rules");
    }
    
    // Methodes declenchees par les boutons du menu de gauche
    private void DictionaryAttackClick(object? sender, RoutedEventArgs e) => SetAttackMode(AttackType.Dictionary);
    private void DictionaryAttackAndRulesClick(object? sender, RoutedEventArgs e) => SetAttackMode(AttackType.Rules);
    private void BruteForceAttackClick(object? sender, RoutedEventArgs e) => SetAttackMode(AttackType.Mask);
    private void AssociationAttackClick(object? sender, RoutedEventArgs e) => SetAttackMode(AttackType.Association);
    private void PrinceAttackClick(object? sender, RoutedEventArgs e) => SetAttackMode(AttackType.Prince);

    // Declenche quand on change une valeur dans une liste
    private void OnSelectionChanged(object? sender, SelectionChangedEventArgs e) => GenererCommande();
    
    // Declenche quand on tape dans le champ Masque
    private void OnMaskInputChanged(object? sender, KeyEventArgs e) => GenererCommande();
    
    // Active ou desactive les elements de l'interface selon le mode choisi
    private void SetAttackMode(AttackType type)
    {
        _currentAttackType = type;

        // Recuperation des controles UI
        var boxWordlist = this.FindControl<ComboBox>("WordlistComboBox");
        var boxHashfile = this.FindControl<ComboBox>("HashfileComboBox");
        var boxHashType = this.FindControl<ComboBox>("HashTypeComboBox");
        var boxRules = this.FindControl<ComboBox>("RulesComboBox");
        var txtMask = this.FindControl<TextBox>("MaskInputBox"); 
        
        var ButtonOption1 = this.FindControl<Button>("ButtonOption1");
        var ButtonOption2 = this.FindControl<Button>("ButtonOption2");
        var ButtonOption3 = this.FindControl<Button>("ButtonOption3");
        var ButtonOption4 = this.FindControl<Button>("ButtonOption4");
        var ButtonOption5 = this.FindControl<Button>("ButtonOption5");

        // On cache/desactive/reset tout
        boxWordlist!.IsVisible = false;
        boxHashfile!.IsVisible = false;
        boxHashType!.IsVisible = false;
        boxRules!.IsVisible = false;
        txtMask!.IsVisible = false;
        
        ButtonOption1!.Opacity = 1;
        ButtonOption2!.Opacity = 1;
        ButtonOption3!.Opacity = 1;
        ButtonOption4!.Opacity = 1;
        ButtonOption5!.Opacity = 1;
        
        boxWordlist.SelectedItem = -1 ;
        boxHashfile.SelectedItem = -1 ;
        boxHashType.SelectedItem = -1 ;
        boxRules.SelectedItem = -1 ;
        txtMask.Text = string.Empty;
        
        // On active uniquement ce qui est necessaire
        switch (type)
        {
            case AttackType.Dictionary:
                ButtonOption1.Opacity = 0.4;
                boxWordlist.IsVisible = true;
                boxHashfile.IsVisible = true;
                boxHashType.IsVisible = true;
                break;

            case AttackType.Rules:
                ButtonOption2.Opacity = 0.4;
                boxWordlist.IsVisible = true;
                boxHashfile.IsVisible = true;
                boxHashType.IsVisible = true;
                boxRules.IsVisible = true;
                break;
            
            case AttackType.Mask:
                ButtonOption3.Opacity = 0.4;
                boxWordlist.IsVisible = false;
                boxHashfile.IsVisible = true;
                boxHashType.IsVisible = true;
                txtMask.IsVisible = true;
                break;
            
            case AttackType.Association:
                ButtonOption4.Opacity = 0.4;
                boxWordlist.IsVisible = true;
                boxHashfile.IsVisible = true;
                boxHashType.IsVisible = true;
                boxRules.IsVisible = true;
                break;
            
            case AttackType.Prince:
                ButtonOption5.Opacity = 0.4;
                boxWordlist.IsVisible = true;
                boxHashfile.IsVisible = true;
                boxHashType.IsVisible = true;
                break;
        }
        GenererCommande();
    }

    // Construction dynamique de la commande Hashcat
    private void GenererCommande()
    {
        var txtInput = this.FindControl<TextBox>("TxtCommandInput");
        var boxWordlist = this.FindControl<ComboBox>("WordlistComboBox");
        var boxHashfile = this.FindControl<ComboBox>("HashfileComboBox");
        var boxHashType = this.FindControl<ComboBox>("HashTypeComboBox");
        var boxRules = this.FindControl<ComboBox>("RulesComboBox");
        var txtMask = this.FindControl<TextBox>("MaskInputBox");

        if (txtInput == null) return;
        
        // Recuperation des valeurs
        
        string modeValue = "0"; 
        if (boxHashType?.SelectedItem is OptionMode selectedMode)
        {
            modeValue = selectedMode.value.ToString();
        }
        
        string hashPath = "<fichier_hashes>";
        if (boxHashfile?.SelectedItem != null)
        {
            hashPath = $"/root/hashfiles/{boxHashfile.SelectedItem}";
        }
        
        string wordlistPath = "<wordlist>";
        if (boxWordlist?.SelectedItem != null)
        {
            wordlistPath = $"/root/wordlists/{boxWordlist.SelectedItem}";
        }
        
        string rulePath = "";
        if (boxRules?.SelectedItem != null)
        {
            rulePath = $" -r /usr/share/hashcat/rules/{boxRules.SelectedItem}";
        }
        
        string maskValue = "";
        if (txtMask != null && !string.IsNullOrEmpty(txtMask.Text))
        {
            maskValue = txtMask.Text;
        }
        
        // Construction de la string finale
        string finalCommand = "";

        switch (_currentAttackType)
        {
            case AttackType.Dictionary:
                finalCommand = $"hashcat -m {modeValue} -a 0 {hashPath} {wordlistPath}";
                break;

            case AttackType.Rules:
                finalCommand = $"hashcat -m {modeValue} -a 0 {hashPath} {wordlistPath}{rulePath}";
                break;
            
            case AttackType.Mask:
                finalCommand = $"hashcat -m {modeValue} -a 3 {hashPath} {maskValue}";
                break;
            
            case AttackType.Association:
                finalCommand = $"hashcat -m {modeValue} -a 9 {hashPath} {wordlistPath}{rulePath}";
                break;
            
            case AttackType.Prince:
                finalCommand = $"hashcat -m {modeValue} -a 8 {hashPath} {wordlistPath}";
                break;
        }

        txtInput.Text = finalCommand;
    }
    private async void AjouterWordlistClick(object? sender, RoutedEventArgs e)
    {
        var window = TopLevel.GetTopLevel(this) as Window;
        if (window == null) return;
        try
        {
            await _toolFileService.PickAndUploadAsync(ToolFileModel.Wordlist, window);
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
            await _toolFileService.PickAndUploadAsync(ToolFileModel.HashFile, window);
            var box = this.FindControl<ComboBox>("HashfileComboBox");
            if (box != null) RemplirComboBox(box, "/root/hashfiles");
        }
        catch (Exception ex) { Console.WriteLine(ex.Message); }
    }
    
    private async void LancerCommandeClick(object? sender, RoutedEventArgs e)
    {
        var txtInput  = this.FindControl<TextBox>("TxtCommandInput");
        var txtOutput = this.FindControl<TextBox>("TxtOutput");
        var btnLancer = this.FindControl<Button>("BtnLancer");
        var btnStop   = this.FindControl<Button>("BtnStop");

        // Vérifications simples
        if (txtInput == null || txtOutput == null || btnLancer == null || btnStop == null)
            return;

        var commande = txtInput.Text;
        if (string.IsNullOrWhiteSpace(commande))
            return;

        // Reset / état UI
        txtOutput.Text = ">>> Démarrage de l'attaque Hashcat...\n";
        btnLancer.IsEnabled = false;
        btnStop.IsEnabled   = true;

        // Nouveau token d’annulation
        _cts?.Cancel();
        _cts = new CancellationTokenSource();

        await _execService.ExecuteCommandStreamingAsync(
            commande,

            // Reçoit chaque ligne en temps réel
            onLineReceived: ligne =>
            {
                Dispatcher.UIThread.Post(() =>
                {
                    txtOutput.Text += ligne + "\n";
                    txtOutput.CaretIndex = txtOutput.Text.Length; // auto-scroll
                });
            },

            // Fin de l’exécution (normalement)
            onCompleted: () =>
            {
                Dispatcher.UIThread.Post(() =>
                {
                    txtOutput.Text += "\n>>> Fin de l'exécution Hashcat.";
                    btnLancer.IsEnabled = true;
                    btnStop.IsEnabled   = false;
                });
            },

            // Erreur (SSH non connecté ou autre)
            onError: msg =>
            {
                Dispatcher.UIThread.Post(() =>
                {
                    txtOutput.Text += $"\n[ERREUR] : {msg}";
                    btnLancer.IsEnabled = true;
                    btnStop.IsEnabled   = false;
                });
            },

            // Cancel
            cancel: _cts.Token
        );
    }

    
    private void StopCommandeClick(object? sender, RoutedEventArgs e)
    {
        var txtOutput = this.FindControl<TextBox>("TxtOutput");
        var btnLancer = this.FindControl<Button>("BtnLancer");
        var btnStop   = this.FindControl<Button>("BtnStop");

        // Annulation côté client / service
        _cts?.Cancel();
        _execService.StopCurrent();

        // Kill brutal côté Kali (tous les processus hashcat)
        try
        {
            var ssh = ConnexionSshService.Instance.Client;
            if (ssh != null && ssh.IsConnected)
            {
                using var killCmd = ssh.CreateCommand("pkill -9 hashcat");
                killCmd.Execute();
            }
        }
        catch
        {
            // On ignore les erreurs de kill (process déjà mort, etc.)
        }

        // Feedback UI
        if (txtOutput != null)
            txtOutput.Text += "\n\n>>> STOP FORCÉ PAR L'UTILISATEUR (Hashcat).";

        if (btnLancer != null) btnLancer.IsEnabled = true;
        if (btnStop   != null) btnStop.IsEnabled   = false;
    }


    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    // aller chercher le style <Style Selector="control|TemplateControl">
    protected override Type StyleKeyOverride => typeof(TemplateControl);
}