using Avalonia.Controls;

namespace CraKit.Views.Tools.HashCat;

// Classe HashCat qui herite de ToolBase
public class HashCat : ToolBase 
{
    private Control view;

    public HashCat() : base("Hashcat", "Outil de crack de mot de passe")
    {
        view = null;
    }

    public override Control GetView()
    {
        if (view == null)
        {
            view = new HashCatVue(); 
        }
        return view;
    }
}

