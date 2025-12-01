using Avalonia.Controls;
using Avalonia.Interactivity;
using CraKit.Views.Tools.HashCat;
using CraKit.Views.Tools.Hydra;
using CraKit.Views.Tools.John;

namespace CraKit.Views;

// Page de choix des outils (John, Hydra, HashCat)
public partial class ChoixOutilsMode : UserControl
{
    public ChoixOutilsMode()
    {
        InitializeComponent();
    }
    
    // Ouvrir John The Ripper
    private void OpenJohn(object sender, RoutedEventArgs e)
    {
        John johnTool = new John();
        
        Window fenetreOutil = new Window();
        fenetreOutil.Title = johnTool.Name;
        fenetreOutil.Width = 1000;
        fenetreOutil.Height = 700;
        fenetreOutil.Content = johnTool.View;
        
        fenetreOutil.Show();
    }

    // Ouvrir HashCat
    private void OpenHashCat(object sender, RoutedEventArgs e)
    {
        HashCat hashcatTool = new HashCat();

        Window fenetreOutil = new Window();
        fenetreOutil.Title = hashcatTool.Name;
        fenetreOutil.Width = 1000;
        fenetreOutil.Height = 700;
        fenetreOutil.Content = hashcatTool.View;

        fenetreOutil.Show();
    }

    // Ouvrir Hydra
    private void OpenHydra(object sender, RoutedEventArgs e)
    {
        Hydra hydraTool = new Hydra();
        
        Window fenetreOutil = new Window();
        fenetreOutil.Title = hydraTool.Name;
        fenetreOutil.Width = 1000;
        fenetreOutil.Height = 700;
        fenetreOutil.Content = hydraTool.View;
        
        fenetreOutil.Show();
    }

    // DNSMap pas encore fait
    private void OpenDnsMap(object sender, RoutedEventArgs e)
    {
        System.Console.WriteLine("[Info] DNSMap pas encore fait");
    }
}

