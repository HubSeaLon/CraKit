using Avalonia.Controls;

namespace CraKit.Views.Tools.Hydra;

public class Hydra : ToolBase
{
    private Control? _view;
    
    public Hydra() : base("Hydra", "Outil de craquage de mots de passe réseau")
    {
    }
    
    public override Control GetView()
    {
        if (_view == null)
        {
            _view = new HydraVue(); 
        }
        return _view;
    }
}

