using Avalonia.Controls;
using CraKit.Views;

namespace CraKit;

// Fenetre principale de l'application
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();     
        
        // Afficher la vue d'accueil
        AccueilConnexionVue.Content = new AccueilConnexionVue();
    }
    
    // Changer de vue (navigation)
    public void Navigate(UserControl newView)
    {
        AccueilConnexionVue.Content = newView;
    }
}

