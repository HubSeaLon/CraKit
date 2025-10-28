using Avalonia.Controls;
using CraKit.Views;


namespace CraKit;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();     
        
        // Afficher la vue d'accueil
        AccueilConnexionVue.Content = new AccueilConnexionVue();
    }
    
    // Méthode publique de navigation entre vue
    public void Navigate(UserControl newView)
    {
        AccueilConnexionVue.Content = newView;
    }
} 