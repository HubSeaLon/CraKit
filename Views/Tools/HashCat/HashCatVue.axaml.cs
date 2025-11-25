using System; // Nécessaire pour le type "Type"
using Avalonia.Markup.Xaml;
using CraKit.Templates; 

namespace CraKit.Views.Tools.HashCat;

public partial class HashCatVue : TemplateControl
{
    public HashCatVue()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    // AJOUTEZ CECI : C'est la ligne magique !
    // Elle dit à Avalonia d'aller chercher le style <Style Selector="control|TemplateControl">
    protected override Type StyleKeyOverride => typeof(TemplateControl);
}