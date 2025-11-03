using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using CraKit.Services;

namespace CraKit.Views;

public partial class TestConnexionCommande : UserControl
{
    
    private readonly ConnexionSshService _sshService = new();
    private readonly ExecuterCommandeService _exec;
   
    public TestConnexionCommande()
    {
        InitializeComponent();
        _exec = new ExecuterCommandeService(_sshService);
    }
    
    public async void Connecter(object sender, RoutedEventArgs e)
    {
        var success = await _sshService.ConnectAsync("localhost", 2222, "root", "123");

        Dispatcher.UIThread.Post(() =>
        {
            SortieText.Text = success
                ? "[SSH] Connexion réussie.\n"
                : "[SSH] Échec de la connexion.\n";
        });
    }

    public async void LancerCommande(object? sender, RoutedEventArgs e)
    { 
        var cmd = CommandeTexte.Text?.Trim();
        if (string.IsNullOrEmpty(cmd)) return;

        SortieText.Text = $"$ {cmd}\n";
        var outp = await _exec.ExecuteCommandAsync(cmd, TimeSpan.FromMinutes(5));
        SortieText.Text += outp + "\n";
    }

    public void Retour(object sender, RoutedEventArgs e)
    {
        if (TopLevel.GetTopLevel(this) is MainWindow mainWindow)
        {
            mainWindow.Navigate(new AccueilConnexionVue());
        }
    }
    
}