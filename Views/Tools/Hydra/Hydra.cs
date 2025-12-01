using Avalonia.Controls;

namespace CraKit.Views.Tools.Hydra;

// Classe Hydra qui herite de ToolBase
public class Hydra : ToolBase
{
    private Control view;
    
    public Hydra() : base("Hydra", "Outil de craquage de mots de passe reseau")
    {
        view = null;
    }
    
    public override Control GetView()
    {
        if (view == null)
        {
            view = new HydraVue(); 
        }
        return view;
    }
}

