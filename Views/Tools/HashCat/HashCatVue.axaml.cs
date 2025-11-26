using System;
using Avalonia.Controls;
using Avalonia.Interactivity; // Nécessaire pour le type "Type"
using Avalonia.Markup.Xaml;
using CraKit.Models;
using CraKit.Services;
using CraKit.Templates; 

namespace CraKit.Views.Tools.HashCat;

public partial class HashCatVue : TemplateControl
{
    
    private readonly ToolFileService toolFileService;
    public HashCatVue()
    {
        InitializeComponent();
        toolFileService = new ToolFileService(ConnexionSshService.Instance);
    }
    
    private async void AjouterWordlistClick(object? sender, RoutedEventArgs e)
    {
        var window = TopLevel.GetTopLevel(this) as Window;
        if (window is null) return;
        
        try
        {
            await toolFileService.PickAndUploadAsync(ToolFileModel.Wordlist, window);
            Console.WriteLine("Wordlist uploaded !");
            
            // Ajouter MessageBox pour avertir
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
            Console.WriteLine("Hashfile uploaded !");
            
            // Ajouter MessageBox pour avertir
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
    
    // aller chercher le style <Style Selector="control|TemplateControl">
    protected override Type StyleKeyOverride => typeof(TemplateControl);
}