using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace CraKit.Templates;

// Template de base pour tous les outils (Hydra, John, HashCat)
public class TemplateControl : TemplatedControl
{
    // Titre de l'outil
    public static readonly StyledProperty<string> TitleProperty =
        AvaloniaProperty.Register<TemplateControl, string>(nameof(Title));
    
    public string Title
    {
        get { return GetValue(TitleProperty); }
        set { SetValue(TitleProperty, value); }
    }

    // Panneau gauche (boutons, options)
    public static readonly StyledProperty<object> LeftPaneProperty =
        AvaloniaProperty.Register<TemplateControl, object>(nameof(LeftPane));
    
    public object LeftPane
    {
        get { return GetValue(LeftPaneProperty); }
        set { SetValue(LeftPaneProperty, value); }
    }
    
    // Panneau haut droite (options)
    public static readonly StyledProperty<object> RightTopProperty =
        AvaloniaProperty.Register<TemplateControl, object>(nameof(RightTop));
    
    public object RightTop
    {
        get { return GetValue(RightTopProperty); }
        set { SetValue(RightTopProperty, value); }
    }
    
    // Zone d'entree
    public static readonly StyledProperty<object> InputProperty =
        AvaloniaProperty.Register<TemplateControl, object>(nameof(Input));
    
    public object Input
    {
        get { return GetValue(InputProperty); }
        set { SetValue(InputProperty, value); }
    }

    // Zone bouton lancer
    public static readonly StyledProperty<object> RunAreaProperty =
        AvaloniaProperty.Register<TemplateControl, object>(nameof(RunArea));
    
    public object RunArea
    {
        get { return GetValue(RunAreaProperty); }
        set { SetValue(RunAreaProperty, value); }
    }

    // Zone sortie
    public static readonly StyledProperty<object> OutputProperty =
        AvaloniaProperty.Register<TemplateControl, object>(nameof(Output));
    
    public object Output
    {
        get { return GetValue(OutputProperty); }
        set { SetValue(OutputProperty, value); }
    }

    // Bouton en haut a droite
    public static readonly StyledProperty<object> HeaderRightProperty =
        AvaloniaProperty.Register<TemplateControl, object>(nameof(HeaderRight));
    
    public object HeaderRight
    {
        get { return GetValue(HeaderRightProperty); }
        set { SetValue(HeaderRightProperty, value); }
    }
}

