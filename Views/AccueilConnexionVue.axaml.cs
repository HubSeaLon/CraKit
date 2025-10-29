using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using CraKit.Services;

namespace CraKit.Views;

public partial class AccueilConnexionVue : UserControl
{
    public AccueilConnexionVue()
    {
        InitializeComponent();
    }

    private async void Connecter(object sender, RoutedEventArgs e)
    {
        var success = await ConnexionSshService.Instance.ConnectAsync("localhost", 22, "root", "123");
        
        if (success && TopLevel.GetTopLevel(this) is MainWindow mainWindow)
        {
            Console.WriteLine("[SSH] Connexion réussie");
            mainWindow.Content = new ChoixOutilsMode();
        }
        else
        {
            Sortie.Text = "[SSH] Échec de la connexion";
            Console.WriteLine("[SSH] Échec de la connexion");
        }
    }
    
    // Peut-être créer une commande pour lancer la machine Kali automatiquement ?
    
    // Fenêtre de test commmandes
    private void TestCommande(object sender, RoutedEventArgs e)
    {
        if (TopLevel.GetTopLevel(this) is MainWindow mainWindow)
        {
            mainWindow.Navigate(new TestConnexionCommande());
        }  
    }
    
}