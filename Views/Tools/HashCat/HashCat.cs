using Avalonia.Controls;
using CraKit.Services; 

namespace CraKit.Views.Tools.HashCat;

// Classe de definition de l'outil Hashcat.
// Elle herite de ToolBase pour etre reconnue par le systeme de plugins de l'application.
public class HashCat : ToolBase 
{
    // Stocke l'instance de la vue pour eviter de la recreer inutilement.
    private Control? _view;
    
    public HashCat() : base("Hashcat", "Outil de crack de mot de passe")
    {
    }

    // Methode appelee par l'application pour afficher l'interface.
    public override Control GetView()
    {
        // On instancie la vue seulement au premier appel.
        if (_view == null)
        {
            _view = new HashCatVue(); 
        }
        
        return _view;
    }
}