using Avalonia.Controls;
using CraKit.Services; // INDISPENSABLE pour trouver ToolBase

namespace CraKit.Views.Tools.HashCat;

// On hérite de ToolBase
public class HashCat : ToolBase 
{
    private Control? _view;

    // Le constructeur appelle "base" qui renvoie vers ToolBase
    public HashCat() : base("Hashcat", "Outil de crack de mot de passe")
    {
    }

    public override Control GetView()
    {
        if (_view == null)
        {
            _view = new HashCatVue(); 
        }
        return _view;
    }
}