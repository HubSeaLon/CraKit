using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using CraKit.Models;
using CraKit.Services;
using CraKit.Templates;


namespace CraKit.Views.Tools.John;

public partial class JohnVue : TemplateControl
{
    
    private readonly ToolFileService toolFileService;

    public JohnVue()
    {
        InitializeComponent();
        toolFileService = new ToolFileService(ConnexionSshService.Instance);
        AttachedToVisualTree += OnAttachedToVisualTree;
    }

    private void choixOptionClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button btn) return; 
        
        var name = btn.Name;
        
        switch (name)
        {
            case "ButtonOption1":
                ResetButtonStyles();
                ButtonOption1.Opacity = 0.4;
                
                WordlistComboBox!.IsEnabled = false;
                HashfileComboBox!.IsEnabled = true;
                FormatHashComboBox!.IsEnabled = false;
                RuleComboBox!.IsEnabled = false;
                MaskTextBox!.IsEnabled = false;
                break;
            
            case "ButtonOption2":
                ResetButtonStyles();
                ButtonOption2.Opacity = 0.4;
                
                WordlistComboBox!.IsEnabled = true;
                HashfileComboBox!.IsEnabled = true;
                FormatHashComboBox!.IsEnabled = false;
                RuleComboBox!.IsEnabled = false;
                MaskTextBox!.IsEnabled = false;
                break; 
            
            case "ButtonOption3":
                ResetButtonStyles();
                ButtonOption3.Opacity = 0.4;
                
                WordlistComboBox!.IsEnabled = true;
                HashfileComboBox!.IsEnabled = true;
                FormatHashComboBox!.IsEnabled = true;
                RuleComboBox!.IsEnabled = false;
                MaskTextBox!.IsEnabled = false;
                break;
            
            case "ButtonOption4":
                ResetButtonStyles();
                ButtonOption4.Opacity = 0.4;
                
                WordlistComboBox!.IsEnabled = true;
                HashfileComboBox!.IsEnabled = true;
                FormatHashComboBox!.IsEnabled = true;
                RuleComboBox!.IsEnabled = true;
                MaskTextBox!.IsEnabled = false;
                break;
            
            case "ButtonOption5":
                ResetButtonStyles();
                ButtonOption5.Opacity = 0.4;
                
                WordlistComboBox!.IsEnabled = true;
                HashfileComboBox!.IsEnabled = true;
                FormatHashComboBox!.IsEnabled = true;
                RuleComboBox!.IsEnabled = false;
                MaskTextBox!.IsEnabled = true;
                break;
        }
    }
    
    private void ResetButtonStyles()
    {
        ButtonOption1.Opacity = 1;
        ButtonOption2.Opacity = 1;
        ButtonOption3.Opacity = 1;
        ButtonOption4.Opacity = 1;
        ButtonOption5.Opacity = 1;
    }

    
    private async void AjouterWordlistClick(object? sender, RoutedEventArgs e)
    {
        var window = TopLevel.GetTopLevel(this) as Window;
        if (window is null) return;
        
        try
        {
            await toolFileService.PickAndUploadAsync(ToolFileModel.Wordlist, window);
            // Ajouter MessageBox pour avertir ou zone texte
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
    
    private async void AjouterHashfileClick(object? sender, RoutedEventArgs e)
    {
        var window = TopLevel.GetTopLevel(this) as Window;
        if (window is null) return;

        try
        {
            await toolFileService.PickAndUploadAsync(ToolFileModel.HashFile, window);
            // Ajouter MessageBox pour avertir ? Ou bien une zone texte simple
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        
    }
    
    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
    
    
    
    // Méthode permettant de récupérer les noms des bouttons, listes, etc. vu qu'on utilise un template
    private void OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        WordlistComboBox = this.FindControl<ComboBox>("WordlistComboBox");
        HashfileComboBox = this.FindControl<ComboBox>("HashfileComboBox");
        FormatHashComboBox = this.FindControl<ComboBox>("FormatHashComboBox");
        RuleComboBox = this.FindControl<ComboBox>("RuleComboBox");
        MaskTextBox = this.FindControl<TextBox>("MaskTextBox");
        
        ButtonOption1 = this.FindControl<Button>("ButtonOption1");
        ButtonOption2 = this.FindControl<Button>("ButtonOption2");
        ButtonOption3 =  this.FindControl<Button>("ButtonOption3");
        ButtonOption4 =  this.FindControl<Button>("ButtonOption4");
        ButtonOption5 =  this.FindControl<Button>("ButtonOption5");

        WordlistComboBox!.IsEnabled = false;
        HashfileComboBox!.IsEnabled = false;
        FormatHashComboBox!.IsEnabled = false;
        RuleComboBox!.IsEnabled = false;
        MaskTextBox!.IsEnabled = false;
    }
    
    
    // aller chercher le style <Style Selector="control|TemplateControl">
    protected override Type StyleKeyOverride => typeof(TemplateControl);
}