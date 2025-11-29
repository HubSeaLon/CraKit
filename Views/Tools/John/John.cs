using Avalonia.Controls;

namespace CraKit.Views.Tools.John;

public class John : ToolBase
{
    private Control? _view;
    
    public John() : base("John", "Outil de crack de mot de passe")
    {
    }
    
    public override Control GetView()
    {
        if (_view == null)
        {
            _view = new JohnVue(); 
        }
        return _view;
    }
}