using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using CraKit.Services;
using CraKit.Views.ChoixOutils;

namespace CraKit.Views;

public partial class AccueilConnexionVue : UserControl
{
    
    private readonly ConnexionSshService _sshService = new();
    public AccueilConnexionVue()
    {
        InitializeComponent();
    }

    private async void Connecter(object sender, RoutedEventArgs e)
    {
        var success = await _sshService.ConnectAsync("localhost", 22, "root", "123");
        
        Sortie.Text = success 
            ? "[SSH] Connexion réussie.\n"
            : "[SSH] Échec de la connexion.\n";
        
        if (TopLevel.GetTopLevel(this) is MainWindow mainWindow)
        {
            mainWindow.Navigate(new ChoixOutilsExpert());
        }
    }
    
}