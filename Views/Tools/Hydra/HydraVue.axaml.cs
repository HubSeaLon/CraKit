using System;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using CraKit.Models;
using CraKit.Services;
using CraKit.Templates;
using Renci.SshNet;

namespace CraKit.Views.Tools.Hydra;

// Vue pour l'outil Hydra
public partial class HydraVue : TemplateControl
{
    // Variables pour construire la commande
    private string commande;
    private string target;
    private string wordlist;
    private string protocol;
    private string mode;
    private string threads;
    private string verbose;
    private string username;
    private string userlist;
    private string combolist;
    
    // Services
    private ToolFileService toolFileService;
    private ExecuterCommandeService executerCommandeService;
    private HistoryService historyService;
    private CancellationTokenSource cts;

    // Constructeur
    public HydraVue()
    {
        InitializeComponent();
        
        // Initialiser les variables
        commande = "";
        target = "";
        wordlist = "";
        protocol = "";
        mode = "";
        threads = " -t 16";
        verbose = " -v";
        username = "";
        userlist = "";
        combolist = "";
        cts = null;
        
        // Creer les services
        toolFileService = new ToolFileService(ConnexionSshService.Instance);
        executerCommandeService = new ExecuterCommandeService(ConnexionSshService.Instance);
        historyService = HistoryService.Instance;
        
        AttachedToVisualTree += OnAttachedToVisualTree;
        
        // Charger les listes
        ChargerLesListes();
        ChargerProtocoles();
        ChargerThreads();
    }
    
    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    // Quand on clique sur un mode (Single, userlist, Combo)
    private void choixOptionClick(object sender, RoutedEventArgs e)
    {
        Button btn = sender as Button;
        if (btn == null) return;
        
        string name = btn.Name;
        
        ResetButtonStyles();
        
        // Afficher le panneau d'options
        Panel optionsPanel = this.FindControl<Panel>("OptionsPanel");
        if (optionsPanel != null)
        {
            optionsPanel.IsVisible = true;
        }
        
        if (name == "ButtonOption1")
        {
            // mode: Single user + wordlist
            mode = "single";
            ButtonOption1.BorderBrush = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#3B82F6"));
            ButtonOption1.Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#1E40AF"));
            
            UsernameTextBox.IsVisible = true;
            UserlistComboBox.IsVisible = false;
            CombolistComboBox.IsVisible = false;
            WordlistComboBox.IsVisible = true;
        }
        else if (name == "ButtonOption2")
        {
            // mode: User list + wordlist
            mode = "userlist";
            ButtonOption2.BorderBrush = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#3B82F6"));
            ButtonOption2.Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#1E40AF"));
            
            UsernameTextBox.IsVisible = false;
            UserlistComboBox.IsVisible = true;
            CombolistComboBox.IsVisible = false;
            WordlistComboBox.IsVisible = true;
        }
        else if (name == "ButtonOption3")
        {
            // mode: Combo list (user:pass)
            mode = "combo";
            ButtonOption3.BorderBrush = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#3B82F6"));
            ButtonOption3.Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#1E40AF"));
            
            UsernameTextBox.IsVisible = false;
            UserlistComboBox.IsVisible = false;
            CombolistComboBox.IsVisible = true;
            WordlistComboBox.IsVisible = false;
        }
        
        UpdateCommande();
    }
    
    // Remettre tous les boutons a zero
    private void ResetButtonStyles()
    {
        var defaultBg = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#334155"));
        var transparentBorder = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Colors.Transparent);
        
        ButtonOption1.Background = defaultBg;
        ButtonOption1.BorderBrush = transparentBorder;
        
        ButtonOption2.Background = defaultBg;
        ButtonOption2.BorderBrush = transparentBorder;
        
        ButtonOption3.Background = defaultBg;
        ButtonOption3.BorderBrush = transparentBorder;

        protocol = "";
        wordlist = "";
        username = "";
        userlist = "";
        combolist = "";

        ProtocolComboBox.SelectedIndex = -1;
        WordlistComboBox.SelectedIndex = -1;
        UserlistComboBox.SelectedIndex = -1;
        CombolistComboBox.SelectedIndex = -1;
        ThreadsComboBox.SelectedIndex = -1;

        UsernameTextBox.Text = "";
        TargetTextBox.Text = "";
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
            Console.WriteLine("wordlist uploaded!");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
    
    // Ajouter une userlist
    private async void AjouterUserlistClick(object sender, RoutedEventArgs e)
    {
        Window window = TopLevel.GetTopLevel(this) as Window;
        if (window == null) return;

        try
        {
            await toolFileService.PickAndUploadAsync(ToolFileModel.Userlist, window);
            ChargerLesListes();
            Console.WriteLine("userlist uploaded!");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
    
    // Ajouter une combolist
    private async void AjouterCombolistClick(object sender, RoutedEventArgs e)
    {
        Window window = TopLevel.GetTopLevel(this) as Window;
        if (window == null) return;

        try
        {
            await toolFileService.PickAndUploadAsync(ToolFileModel.Combolist, window);
            ChargerLesListes();
            Console.WriteLine("combolist uploaded!");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
    
    // Recuperer les controles de l'interface
    private void OnAttachedToVisualTree(object sender, VisualTreeAttachmentEventArgs e)
    {
        WordlistComboBox = this.FindControl<ComboBox>("WordlistComboBox");
        UserlistComboBox = this.FindControl<ComboBox>("UserlistComboBox");
        CombolistComboBox = this.FindControl<ComboBox>("CombolistComboBox");
        ProtocolComboBox = this.FindControl<ComboBox>("ProtocolComboBox");
        ThreadsComboBox = this.FindControl<ComboBox>("ThreadsComboBox");
        VerboseComboBox = this.FindControl<ComboBox>("VerboseComboBox");
        
        UsernameTextBox = this.FindControl<TextBox>("UsernameTextBox");
        TargetTextBox = this.FindControl<TextBox>("TargetTextBox");
        
        ButtonOption1 = this.FindControl<Button>("ButtonOption1");
        ButtonOption2 = this.FindControl<Button>("ButtonOption2");
        ButtonOption3 = this.FindControl<Button>("ButtonOption3");
        
        EntreeTextBox = this.FindControl<TextBox>("EntreeTextBox");
        SortieTextBox = this.FindControl<TextBox>("SortieTextBox");

        WordlistComboBox.IsVisible = false;
        UserlistComboBox.IsVisible = false;
        CombolistComboBox.IsVisible = false;
        UsernameTextBox.IsVisible = false;
    }
    
    // Remplir une ComboBox avec les fichiers d'un dossier
    private void RemplirComboBox(ComboBox laBox, string chemin)
    {
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
    
    // Quand on change le texte dans target ou username
    private void OnChangedText(object sender, TextChangedEventArgs e)
    {
        TextBox textBox = sender as TextBox;
        if (textBox == null) return;
        
        if (textBox.Name == "TargetTextBox")
        {
            if (TargetTextBox.Text == null || TargetTextBox.Text.Trim() == "")
            {
                target = "";
            }
            else
            {
                target = " " + TargetTextBox.Text;
            }
        }
        else if (textBox.Name == "UsernameTextBox")
        {
            if (UsernameTextBox.Text == null || UsernameTextBox.Text.Trim() == "")
            {
                username = "";
            }
            else
            {
                username = " -l " + UsernameTextBox.Text;
            }
        }
        
        UpdateCommande();
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
                wordlist = " -P /usr/share/wordlists/rockyou.txt";
            }
            else
            {  
                wordlist = " -P /root/wordlists/" + WordlistComboBox.SelectedItem.ToString();
            }
        }
        else if (name == "UserlistComboBox")
        {
            if (UserlistComboBox.SelectedItem == null)
            {
                userlist = "";
            }
            else
            {
                userlist = " -L /root/userlists/" + UserlistComboBox.SelectedItem.ToString();
            }
        }
        else if (name == "CombolistComboBox")
        {
            if (CombolistComboBox.SelectedItem == null)
            {
                combolist = "";
            }
            else
            {
                combolist = " -C /root/combolists/" + CombolistComboBox.SelectedItem.ToString();
            }
        }
        else if (name == "ProtocolComboBox")
        {
            if (ProtocolComboBox.SelectedItem == null)
            {
                protocol = "";
            }
            else
            {
                OptionMode selectedProtocol = ProtocolComboBox.SelectedItem as OptionMode;
                if (selectedProtocol != null)
                {
                    protocol = " " + selectedProtocol.value;
                }
                else
                {
                    protocol = "";
                }
            }
        }
        else if (name == "ThreadsComboBox")
        {
            if (ThreadsComboBox.SelectedItem == null)
            {
                threads = "";
            }
            else
            {
                OptionMode selectedThreads = ThreadsComboBox.SelectedItem as OptionMode;
                if (selectedThreads != null)
                {
                    threads = " -t " + selectedThreads.value;
                }
                else
                {
                    threads = "";
                }
            }
        }
        else if (name == "VerboseComboBox")
        {
            if (VerboseComboBox.SelectedItem == null)
            {
                verbose = "";
            }
            else
            {
                ComboBoxItem selectedVerbose = VerboseComboBox.SelectedItem as ComboBoxItem;
                if (selectedVerbose != null && selectedVerbose.Tag != null)
                {
                    verbose = selectedVerbose.Tag.ToString();
                }
                else
                {
                    verbose = "";
                }
            }
        }
        
        UpdateCommande();
    }

    // Mettre a jour la commande affichee
    private void UpdateCommande()
    {
        // Si aucun mode selectionne, ne rien afficher
        if (mode == null || mode == "")
        {
            commande = "";
            if (EntreeTextBox != null)
            {
                EntreeTextBox.Text = "";
            }
            return;
        }
        
        if (mode == "single")
        {
            commande = "hydra" + username + wordlist + threads + verbose + target + protocol;
        }
        else if (mode == "userlist")
        {
            commande = "hydra" + userlist + wordlist + threads + verbose + target + protocol;
        }
        else if (mode == "combo")
        {
            commande = "hydra" + combolist + threads + verbose + target + protocol;
        }
        else
        {
            commande = "";
        }
        
        if (EntreeTextBox != null)
        {
            EntreeTextBox.Text = commande;
        }
        Console.WriteLine("commande : " + commande);
    }

    // Lancer la commande Hydra
    private async void LancerCommandeClick(object sender, RoutedEventArgs e)
    {
        string cmd = commande.Trim();
        if (cmd == null || cmd == "") return;
        if (SortieTextBox == null) return;

        Button btnLancer = this.FindControl<Button>("BtnLancer");
        Button btnStop = this.FindControl<Button>("BtnStop");

        // Annuler la commande precedente si elle existe
        if (cts != null)
        {
            cts.Cancel();
        }
        cts = new CancellationTokenSource();

        // Desactiver Lancer, activer Stop
        if (btnLancer != null) btnLancer.IsEnabled = false;
        if (btnStop != null) btnStop.IsEnabled = true;

        SortieTextBox.Text = "$ " + cmd + "\n";

        Stopwatch stopwatch = Stopwatch.StartNew();
        StringBuilder outputBuilder = new StringBuilder();

        try
        {
            await executerCommandeService.ExecuteCommandStreamingAsync(
                cmd,
                // Quand on recoit une ligne
                ligne =>
                {
                    outputBuilder.AppendLine(ligne);

                    Dispatcher.UIThread.Post(() =>
                    {
                        SortieTextBox.Text += ligne + "\n";
                        SortieTextBox.CaretIndex = SortieTextBox.Text.Length;
                    });
                },
                // En cas d'erreur
                msg =>
                {
                    outputBuilder.AppendLine("[Erreur] " + msg);

                    Dispatcher.UIThread.Post(() =>
                    {
                        SortieTextBox.Text += "\n[Erreur] " + msg + "\n";
                        SortieTextBox.CaretIndex = SortieTextBox.Text.Length;
                    });
                },
                // Token pour annuler
                cts.Token
            );
        }
        catch (OperationCanceledException)
        {
            // commande annulee
            outputBuilder.AppendLine("\n[commande arretee par l'utilisateur]");
            Dispatcher.UIThread.Post(() =>
            {
                SortieTextBox.Text += "\n[commande arretee par l'utilisateur]\n";
            });
        }
        finally
        {
            stopwatch.Stop();

            string output = outputBuilder.ToString();
            bool success = IsHydraSuccessful(output);

            // Enregistrer dans l'historique
            historyService.AddToHistory("Hydra", cmd, output, success, stopwatch.Elapsed);

            Console.WriteLine("[Hydra] commande ajoutee a l'historique (" + stopwatch.Elapsed.TotalSeconds.ToString("F2") + "s) - Success: " + success);

            // Reactiver Lancer, desactiver Stop
            if (btnLancer != null) btnLancer.IsEnabled = true;
            if (btnStop != null) btnStop.IsEnabled = false;
        }
    }

    // Arreter la commande
    private void StopCommandeClick(object sender, RoutedEventArgs e)
    {
        Button btnLancer = this.FindControl<Button>("BtnLancer");
        Button btnStop = this.FindControl<Button>("BtnStop");

        // Annuler le token
        if (cts != null)
        {
            cts.Cancel();
        }
        
        executerCommandeService.StopCurrent();
        
        // Kill tous les processus hydra sur le serveur
        try
        {
            SshClient ssh = ConnexionSshService.Instance.Client;
            if (ssh != null && ssh.IsConnected)
            {
                var killCmd = ssh.CreateCommand("pkill -9 hydra");
                killCmd.Execute();
            }
        }
        catch
        {
            // Ignorer les erreurs
        }

        // Afficher un message
        if (SortieTextBox != null)
        {
            SortieTextBox.Text += "\n[Stop demande - Processus hydra termines]\n";
        }

        // Reactiver les boutons
        if (btnLancer != null) btnLancer.IsEnabled = true;
        if (btnStop != null) btnStop.IsEnabled = false;
    }
    
    // Verifier si Hydra a reussi (mot de passe trouve)
    private bool IsHydraSuccessful(string output)
    {
        if (output == null || output.Trim() == "") 
            return false;

        // Mot de passe trouve
        if (output.Contains("[") && output.Contains("]") && output.Contains("login:") && output.Contains("password:"))
        {
            return true;
        }
        
        if (output.Contains("valid password found"))
        {
            return true;
        }
        
        return false;
    }
    
    // Sauvegarder l'historique
    private async void SaveHistoryClick(object sender, RoutedEventArgs e)
    {
        Window window = TopLevel.GetTopLevel(this) as Window;
        if (window == null) return;

        try
        {
            bool success = await historyService.SaveHistoryToFileAsync(window, "Hydra");
            
            if (success)
            {
                Console.WriteLine("[Hydra] Historique sauvegarde avec succes !");
            }
            else
            {
                Console.WriteLine("[Hydra] Aucun historique a sauvegarder ou annule");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("[Hydra] Erreur lors de la sauvegarde : " + ex.Message);
        }
    }
    
    // Charger les wordlists, userlists et combolists
    private void ChargerLesListes()
    {
        ComboBox boxWordlist = this.FindControl<ComboBox>("WordlistComboBox");
        ComboBox boxUserlist = this.FindControl<ComboBox>("UserlistComboBox");
        ComboBox boxCombolist = this.FindControl<ComboBox>("CombolistComboBox");

        if (boxWordlist != null)
        {
            RemplirComboBox(boxWordlist, "/root/wordlists");
            boxWordlist.Items.Add("rockyou.txt");
        }
        if (boxUserlist != null)
        {
            RemplirComboBox(boxUserlist, "/root/userlists");
        }
        if (boxCombolist != null)
        {
            RemplirComboBox(boxCombolist, "/root/combolists");
        }
    }
    
    // Charger les protocoles depuis le JSON
    private void ChargerProtocoles()
    {
        try 
        {
            string chemin = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "hydra_options.json");
            string jsonBrut = ToolBase.LireFichierTexte(chemin);

            if (jsonBrut == null || jsonBrut == "") return;
            
            JsonNode rootNode = JsonNode.Parse(jsonBrut);
            
            JsonNode valuesNode = rootNode["options"]["protocol"]["values"];

            if (valuesNode != null)
            {
                JsonSerializerOptions options = new JsonSerializerOptions();
                options.PropertyNameCaseInsensitive = true;
                
                System.Collections.Generic.List<OptionMode> listeModes = valuesNode.Deserialize<System.Collections.Generic.List<OptionMode>>(options);
                
                ComboBox boxProtocol = this.FindControl<ComboBox>("ProtocolComboBox");
                if (listeModes != null && boxProtocol != null)
                {
                    boxProtocol.ItemsSource = listeModes;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("[JSON ERROR] " + ex.Message);
        }
    }
    
    // Charger les threads depuis le JSON
    private void ChargerThreads()
    {
        try 
        {
            string chemin = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "hydra_options.json");
            string jsonBrut = ToolBase.LireFichierTexte(chemin);

            if (jsonBrut == null || jsonBrut == "") return;
            
            JsonNode rootNode = JsonNode.Parse(jsonBrut);
            
            JsonNode valuesNode = rootNode["options"]["threads"]["values"];

            if (valuesNode != null)
            {
                JsonSerializerOptions options = new JsonSerializerOptions();
                options.PropertyNameCaseInsensitive = true;
                
                System.Collections.Generic.List<OptionMode> listeModes = valuesNode.Deserialize<System.Collections.Generic.List<OptionMode>>(options);
                
                ComboBox boxThreads = this.FindControl<ComboBox>("ThreadsComboBox");
                if (listeModes != null && boxThreads != null)
                {
                    boxThreads.ItemsSource = listeModes;
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

