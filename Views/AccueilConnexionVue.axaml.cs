using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using CraKit.Services;

namespace CraKit.Views;

// Vue de connexion au serveur SSH
public partial class AccueilConnexionVue : UserControl
{
    public AccueilConnexionVue()
    {
        InitializeComponent();
    }

    // Quand on clique sur le bouton Connecter
    private async void Connecter(object sender, RoutedEventArgs e)
    {
        // Se connecter au serveur Kali
        bool success = await ConnexionSshService.Instance.ConnectAsync("localhost", 2222, "root", "123");
        
        if (success)
        {
            Console.WriteLine("[SSH] Connexion reussie");
            
            // Aller vers la page de choix des outils
            MainWindow mainWindow = TopLevel.GetTopLevel(this) as MainWindow;
            if (mainWindow != null)
            {
                mainWindow.Content = new ChoixOutilsMode();
            }
        }
        else
        {
            Sortie.Text = "[SSH] Echec de la connexion";
            Console.WriteLine("[SSH] Echec de la connexion");
        }
    }
    
    // Ouvrir la fenetre de test des commandes
    private void TestCommande(object sender, RoutedEventArgs e)
    {
        MainWindow mainWindow = TopLevel.GetTopLevel(this) as MainWindow;
        if (mainWindow != null)
        {
            mainWindow.Navigate(new TestConnexionCommande());
        }  
    }
}

