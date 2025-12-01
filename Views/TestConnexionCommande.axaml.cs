using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using CraKit.Services;

namespace CraKit.Views;

// Page de test de connexion SSH et commandes
public partial class TestConnexionCommande : UserControl
{
    private ConnexionSshService sshService;
    private ExecuterCommandeService exec;
   
    public TestConnexionCommande()
    {
        InitializeComponent();
        sshService = new ConnexionSshService();
        exec = new ExecuterCommandeService(sshService);
    }
    
    // Se connecter au serveur
    public async void Connecter(object sender, RoutedEventArgs e)
    {
        bool success = await sshService.ConnectAsync("localhost", 2222, "root", "123");

        Dispatcher.UIThread.Post(() =>
        {
            if (success)
            {
                SortieText.Text = "[SSH] Connexion reussie.\n";
            }
            else
            {
                SortieText.Text = "[SSH] Echec de la connexion.\n";
            }
        });
    }

    // Executer une commande
    public async void LancerCommande(object sender, RoutedEventArgs e)
    { 
        string cmd = CommandeTexte.Text;
        
        if (cmd == null || cmd.Trim() == "")
        {
            return;
        }

        SortieText.Text = "$ " + cmd + "\n";
        
        string resultat = await exec.ExecuteCommandAsync(cmd, TimeSpan.FromMinutes(5));
        
        SortieText.Text += resultat + "\n";
    }

    // Retourner a la page d'accueil
    public void Retour(object sender, RoutedEventArgs e)
    {
        MainWindow mainWindow = TopLevel.GetTopLevel(this) as MainWindow;
        if (mainWindow != null)
        {
            mainWindow.Navigate(new AccueilConnexionVue());
        }
    }
}

