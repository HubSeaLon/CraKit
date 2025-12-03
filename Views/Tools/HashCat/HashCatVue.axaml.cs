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

// Enumeration pour lister proprement les types d'attaques disponibles.
public enum AttackType
{
    Dictionary, // -a 0
    Rules,      // -a 0 + règles
    Mask,       // -a 3
    Combinator, // -a 1
    Association // -a 9
}


public partial class HashCatVue : TemplateControl
{
    // Services pour gerer les fichiers et l'execution SSH.
    private readonly ToolFileService _toolFileService;
    private readonly ExecuterCommandeService _execService;
    
    // Permet de savoir quel mode est selectionne.
    private AttackType _currentAttackType = AttackType.Dictionary;
    
    // Commande qui permet d'arreter une attaque.
    private CancellationTokenSource? _cts;
    
    public HashCatVue()
    {
        // On recupere la connexion SSH.
        _toolFileService = new ToolFileService(ConnexionSshService.Instance);
        _execService = new ExecuterCommandeService(ConnexionSshService.Instance);
        
        // Charge le fichier XAML.
        InitializeComponent();
        
        // On remplit les listes deroulantes dès le demarrage.
        ChargerLesListes();
        ChargerHashTypes();
    }
    
    // Charge les types de hash depuis un fichier JSON.
    private void ChargerHashTypes()
    {
        try 
        {
            // On construit le chemin vers le fichier JSON.
            string chemin = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "HashCat.json");
            
            // On lit le contenu brut du fichier.
            string jsonBrut = ToolBase.LireFichierTexte(chemin);
            
            if (string.IsNullOrEmpty(jsonBrut)) 
            {
                return;
            }
            
            // On transforme le texte en objet JSON manipulable.
            var rootNode = JsonNode.Parse(jsonBrut);
            var valuesNode = rootNode?["options"]["hashType"]["values"];

            // Si on a bien trouve la liste des valeurs dans le JSON
            if (valuesNode != null)
            {
                // Options pour ignorer la casse (Majuscule/minuscule) lors de la lecture.
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                
                // Deserialisation : On transforme le JSON en une liste d'objets C# (List<OptionMode>).
                var listeModes = valuesNode.Deserialize<List<OptionMode>>(options);
                
                // On recupère la ComboBox de l'interface.
                var boxType = this.FindControl<ComboBox>("HashTypeComboBox");
                
                // Si la liste est valide et que la boite existe, on lie les donnees.
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
    
    private void RemplirComboBox(ComboBox laBox, string chemin)
    {
        // Securite de base.
        if (laBox == null) 
        {
            return;
        }

        try
        {
            var ssh = ConnexionSshService.Instance.Client;
            
            if (ssh == null || !ssh.IsConnected) 
            {
                return;
            }
            
            var cmd = ssh.CreateCommand($"ls -1 {chemin}");
            string resultat = cmd.Execute();
            
            laBox.Items.Clear();
            
            if (!string.IsNullOrWhiteSpace(resultat) && !resultat.Contains("No such file"))
            {
                // On decoupe le resultat ligne par ligne pour obtenir les noms de fichiers.
                var fichiers = resultat.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                
                foreach (var f in fichiers) 
                {
                    laBox.Items.Add(f);
                }
            }
        }
        catch (Exception ex) 
        { 
            Console.WriteLine($"[SSH] Erreur : {ex.Message}"); 
        }
    }
    
    private void ChargerLesListes()
    {
        // On recupère toutes les references aux contrôles XAML.
        var boxWordlist = this.FindControl<ComboBox>("WordlistComboBox");
        var boxWordlist2 = this.FindControl<ComboBox>("WordlistComboBox2");
        var boxHashfile = this.FindControl<ComboBox>("HashfileComboBox");
        var boxUserHashfile = this.FindControl<ComboBox>("HashUserfileComboBox");
        var boxRules = this.FindControl<ComboBox>("RulesComboBox");

        // On appelle la fonction de remplissage pour chaque boite si elle existe.
        
        if (boxWordlist != null) 
        {
            RemplirComboBox(boxWordlist, "/root/wordlists");
        }
        
        if (boxWordlist2 != null) 
        {
            RemplirComboBox(boxWordlist2, "/root/wordlists");
        }
        
        if (boxHashfile != null) 
        {
            RemplirComboBox(boxHashfile, "/root/hashfiles");
        }
        
        if (boxUserHashfile != null) 
        {
            RemplirComboBox(boxUserHashfile, "/root/hashfiles");
        }
        
        if (boxRules != null) 
        {
            RemplirComboBox(boxRules, "/usr/share/hashcat/rules");
        }
    }
    
    // Gestionnaires d'evenements Clicks sur les boutons du menu
    private void DictionaryAttackClick(object? sender, RoutedEventArgs e) => SetAttackMode(AttackType.Dictionary);
    private void DictionaryAttackAndRulesClick(object? sender, RoutedEventArgs e) => SetAttackMode(AttackType.Rules);
    private void BruteForceAttackClick(object? sender, RoutedEventArgs e) => SetAttackMode(AttackType.Mask);
    private void CombinatorClick(object? sender, RoutedEventArgs e) => SetAttackMode(AttackType.Combinator);
    private void AssociationAttackClick(object? sender, RoutedEventArgs e) => SetAttackMode(AttackType.Association);

    // Declenche quand l'utilisateur change une selection
    private void OnSelectionChanged(object? sender, SelectionChangedEventArgs e) => GenererCommande();
    
    // Declenche quand l'utilisateur tape dans la zone de Masque.
    private void OnMaskInputChanged(object? sender, KeyEventArgs e) => GenererCommande();
    
    // Change l'interface selon le mode d'attaque choisi.
    private void SetAttackMode(AttackType type)
    {
        _currentAttackType = type;

        // Recuperation de tous les elements graphiques.
        var boxWordlist = this.FindControl<ComboBox>("WordlistComboBox");
        var boxWordlist2 = this.FindControl<ComboBox>("WordlistComboBox2");
        var boxHashfile = this.FindControl<ComboBox>("HashfileComboBox");
        var boxUserHashfile = this.FindControl<ComboBox>("HashUserfileComboBox");
        var boxHashType = this.FindControl<ComboBox>("HashTypeComboBox");
        var boxRules = this.FindControl<ComboBox>("RulesComboBox");
        var txtMask = this.FindControl<TextBox>("MaskInputBox"); 
        
        var ButtonOption1 = this.FindControl<Button>("ButtonOption1");
        var ButtonOption2 = this.FindControl<Button>("ButtonOption2");
        var ButtonOption3 = this.FindControl<Button>("ButtonOption3");
        var ButtonOption4 = this.FindControl<Button>("ButtonOption4");
        var ButtonOption5 = this.FindControl<Button>("ButtonOption5");

        // On cache tout par defaut.
        boxWordlist.IsVisible = false;
        boxWordlist2.IsVisible = false;
        boxHashfile.IsVisible = false;
        boxUserHashfile.IsVisible = false;
        boxHashType.IsVisible = false;
        boxRules.IsVisible = false;
        txtMask.IsVisible = false;
        
        ButtonOption1.Opacity = 1;
        ButtonOption2.Opacity = 1;
        ButtonOption3.Opacity = 1;
        ButtonOption4.Opacity = 1;
        ButtonOption5.Opacity = 1;
        
        // On reset les selections.
        boxWordlist.SelectedItem = -1 ;
        boxWordlist2!.SelectedItem = -1;
        boxHashfile.SelectedItem = -1 ;
        boxUserHashfile!.SelectedItem = -1;
        boxHashType.SelectedItem = -1 ;
        boxRules.SelectedItem = -1 ;
        txtMask.Text = string.Empty;
        
        // On reactive uniquement ce qui est necessaire pour le mode choisi.
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
            
            case AttackType.Combinator:
                ButtonOption4.Opacity = 0.4;
                boxWordlist.IsVisible = true;
                boxWordlist2.IsVisible = true;
                boxHashfile.IsVisible = true;
                boxHashType.IsVisible = true;
                break;
            
            case AttackType.Association:
                ButtonOption5.Opacity = 0.4;
                boxWordlist.IsVisible = true;
                boxUserHashfile.IsVisible = true;
                boxHashType.IsVisible = true;
                boxRules.IsVisible = true;
                break;
        }
        
        // Une fois l'interface calee, on regenère la commande.
        GenererCommande();
    }

    // Construit la ligne de commande Hashcat en fonction de l'UI.
    private void GenererCommande()
    {
        var txtInput = this.FindControl<TextBox>("TxtCommandInput");
        var boxWordlist = this.FindControl<ComboBox>("WordlistComboBox");
        var boxWordlist2 = this.FindControl<ComboBox>("WordlistComboBox2");
        var boxHashfile = this.FindControl<ComboBox>("HashfileComboBox");
        var boxUserHashfile = this.FindControl<ComboBox>("HashUserfileComboBox");
        var boxHashType = this.FindControl<ComboBox>("HashTypeComboBox");
        var boxRules = this.FindControl<ComboBox>("RulesComboBox");
        var txtMask = this.FindControl<TextBox>("MaskInputBox");

        if (txtInput == null) 
        {
            return;
        }
        

        // Recuperation du Mode Hash.
        string modeValue = "0"; // Valeur par defaut.
        if (boxHashType?.SelectedItem is OptionMode selectedMode)
        {
            modeValue = selectedMode.value.ToString();
        }
        
        // Chemins des fichiers.
        string hashPath = "<fichier_hashes>";
        if (boxHashfile?.SelectedItem != null)
        {
            hashPath = $"/root/hashfiles/{boxHashfile.SelectedItem}";
        }
        
        string hashUserPath = "<fichier_user:hash>";
        if (boxUserHashfile?.SelectedItem != null)
        {
            hashUserPath = $"/root/hashfiles/{boxUserHashfile.SelectedItem}";
        }
        
        string wordlistPath = "<wordlist>";
        if (boxWordlist?.SelectedItem != null)
        {
            wordlistPath = $"/root/wordlists/{boxWordlist.SelectedItem}";
        }
        
        string wordlist2Path = "<wordlist>";
        if (boxWordlist2?.SelectedItem != null)
        {
            wordlist2Path = $"/root/wordlists/{boxWordlist2.SelectedItem}";
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
                // Mode 0 simple : Hash + Dico
                finalCommand = $"hashcat -m {modeValue} -a 0 {hashPath} {wordlistPath}";
                break;

            case AttackType.Rules:
                // Mode 0 avec règles : Hash + Dico + Rule
                finalCommand = $"hashcat -m {modeValue} -a 0 {hashPath} {wordlistPath} {rulePath}";
                break;
            
            case AttackType.Mask:
                // Mode 3 : Hash + Masque (Bruteforce)
                finalCommand = $"hashcat -m {modeValue} -a 3 {hashPath} {maskValue}";
                break;
            
            case AttackType.Combinator:
                // Mode 1 : Hash + Dico1 + Dico2
                finalCommand = $"hashcat -m {modeValue} -a 1 {hashPath} {wordlistPath} {wordlist2Path}";
                break;
            
            case AttackType.Association:
                // Mode 9 : HashUser + DicoUsers + Rule + flag username
                finalCommand = $"hashcat -m {modeValue} -a 9 {hashUserPath} {wordlistPath} {rulePath} --username";
                break;
        }

        // On affiche la commande dans la TextBox.
        txtInput.Text = finalCommand;
    }

    // Fonction pour uploader une Wordlist depuis le PC local vers le serveur.
    private async void AjouterWordlistClick(object? sender, RoutedEventArgs e)
    {
        var window = TopLevel.GetTopLevel(this) as Window;
        if (window == null) 
        {
            return;
        }

        try
        {
            // 'await' est crucial ici : on attend que l'upload soit fini AVANT de recharger les listes.
            await _toolFileService.PickAndUploadAsync(ToolFileModel.Wordlist, window);
            
            // On recharge tout pour que le nouveau fichier apparaisse immediatement.
            ChargerLesListes(); 
        }
        catch (Exception ex) 
        { 
            Console.WriteLine(ex.Message); 
        }
    }

    // Fonction pour uploader un fichier de Hash.
    private async void AjouterHashfileClick(object? sender, RoutedEventArgs e)
    {
        var window = TopLevel.GetTopLevel(this) as Window;
        if (window == null) 
        {
            return;
        }

        try
        {
            await _toolFileService.PickAndUploadAsync(ToolFileModel.HashFile, window);
            ChargerLesListes();
        }
        catch (Exception ex) 
        { 
            Console.WriteLine(ex.Message); 
        }
    }
    

    private async void LancerCommandeClick(object? sender, RoutedEventArgs e)
    {
        var txtInput  = this.FindControl<TextBox>("TxtCommandInput");
        var txtOutput = this.FindControl<TextBox>("TxtOutput");
        var btnLancer = this.FindControl<Button>("BtnLancer");
        var btnStop   = this.FindControl<Button>("BtnStop");
        
        if (txtInput == null || txtOutput == null || btnLancer == null || btnStop == null)
        {
            return;
        }

        var commande = txtInput.Text;
        if (string.IsNullOrWhiteSpace(commande))
        {
            return;
        }

        // Preparation de l'UI
        txtOutput.Text = ">>> Demarrage de l'attaque Hashcat...\n";
        btnLancer.IsEnabled = false; // On desactive Lancer pour eviter le double clic.
        btnStop.IsEnabled   = true;  // On active Stop.

        // Gestion de l'annulation
        _cts?.Cancel();
        _cts = new CancellationTokenSource();
        
        // On utilise 'await' pour ne pas geler l'interface pendant que ça tourne.
        await _execService.ExecuteCommandStreamingAsync(
            commande,
            
            // Callback : Ce code s'execute à chaque nouvelle ligne.
            onLineReceived: ligne =>
            {
                // Le SSH tourne sur un "Thread" separe. On ne peut pas toucher l'UI depuis ce thread.
                // On doit demander au Thread principal (UI) de faire la mise à jour.
                Dispatcher.UIThread.Post(() =>
                {
                    txtOutput.Text += ligne + "\n";
                    txtOutput.CaretIndex = txtOutput.Text.Length;
                });
            },

            // Ce code s'execute quand la commande est fini.
            onCompleted: () =>
            {
                Dispatcher.UIThread.Post(() =>
                {
                    txtOutput.Text += "\n>>> Fin de l'execution Hashcat.";
                    btnLancer.IsEnabled = true;
                    btnStop.IsEnabled   = false;
                });
            },

            // Ce code s'execute s'il y a une erreur technique.
            onError: msg =>
            {
                Dispatcher.UIThread.Post(() =>
                {
                    txtOutput.Text += $"\n[ERREUR] : {msg}";
                    btnLancer.IsEnabled = true;
                    btnStop.IsEnabled   = false;
                });
            },

            // On passe le token pour pouvoir annuler si l'utilisateur clique sur STOP.
            cancel: _cts.Token
        );
    }
    
    private void StopCommandeClick(object? sender, RoutedEventArgs e)
    {
        var txtOutput = this.FindControl<TextBox>("TxtOutput");
        var btnLancer = this.FindControl<Button>("BtnLancer");
        var btnStop   = this.FindControl<Button>("BtnStop");

        // 1. On annule la tache.
        _cts?.Cancel();
        _execService.StopCurrent();

        // 2. On tue brutalement le processus Hashcat sur le serveur Kali.
        try
        {
            var ssh = ConnexionSshService.Instance.Client;
            if (ssh != null && ssh.IsConnected)
            {
                // garantit que la commande est bien nettoyee après usage.
                using var killCmd = ssh.CreateCommand("pkill -9 hashcat");
                killCmd.Execute();
            }
        }
        catch
        {
            
        }


        if (txtOutput != null)
        {
            txtOutput.Text += "\n\n>>> STOP FORCe PAR L'UTILISATEUR (Hashcat).";
        }

        if (btnLancer != null) btnLancer.IsEnabled = true;
        if (btnStop   != null) btnStop.IsEnabled   = false;
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    // Necessaire pour les templates Avalonia
    protected override Type StyleKeyOverride => typeof(TemplateControl);
}