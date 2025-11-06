using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml.Templates;

namespace CraKit.Templates;

public class TemplateControl : TemplatedControl
{
    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<TemplateControl, string?>(nameof(Title));
    public string? Title { get => GetValue(TitleProperty); set => SetValue(TitleProperty, value); }

    public static readonly StyledProperty<object?> LeftPaneProperty =
        AvaloniaProperty.Register<TemplateControl, object?>(nameof(LeftPane));
    public object? LeftPane { get => GetValue(LeftPaneProperty); set => SetValue(LeftPaneProperty, value); }

    public static readonly StyledProperty<object?> InputProperty =
        AvaloniaProperty.Register<TemplateControl, object?>(nameof(Input));
    public object? Input { get => GetValue(InputProperty); set => SetValue(InputProperty, value); }

    public static readonly StyledProperty<object?> RunAreaProperty =
        AvaloniaProperty.Register<TemplateControl, object?>(nameof(RunArea));
    public object? RunArea { get => GetValue(RunAreaProperty); set => SetValue(RunAreaProperty, value); }

    public static readonly StyledProperty<object?> OutputProperty =
        AvaloniaProperty.Register<TemplateControl, object?>(nameof(Output));
    public object? Output { get => GetValue(OutputProperty); set => SetValue(OutputProperty, value); }

    public static readonly StyledProperty<object?> HeaderRightProperty =
        AvaloniaProperty.Register<TemplateControl, object?>(nameof(HeaderRight));
    public object? HeaderRight { get => GetValue(HeaderRightProperty); set => SetValue(HeaderRightProperty, value); }
}