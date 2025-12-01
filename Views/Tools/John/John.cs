using Avalonia.Controls;

namespace CraKit.Views.Tools.John;

// Classe John qui herite de ToolBase
public class John : ToolBase
{
    private Control view;
    
    public John() : base("John The Ripper", "Outil de crack de mot de passe")
    {
        view = null;
    }
    
    public override Control GetView()
    {
        if (view == null)
        {
            view = new JohnVue(); 
        }
        return view;
    }
}

