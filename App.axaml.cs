using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using CraKit.Services;

namespace CraKit;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow();
            // Executer QuitterDeconnecter quand l'appli se ferme en s'abonnant (+=) à l'événement Exit
            desktop.Exit += QuitterDeconnecter;
        }
        base.OnFrameworkInitializationCompleted();
    }

    // Méthode déclenchée quand l'application se ferme. 
    private void QuitterDeconnecter(object? sender, ControlledApplicationLifetimeExitEventArgs e)
    {
        // Déconnecter du SSH quand l'appli se ferme
        ConnexionSshService.Instance.Dispose();
    }
}