using System.ComponentModel;

namespace CraKit.Views.ChoixOutils;

public partial class ChoixOutilsMode : INotifyPropertyChanged
{
    
    private bool _isSimpleMode; // false = expert (par défaut)

    public bool IsSimpleMode
    {
        get => _isSimpleMode;
        set
        {
            if (_isSimpleMode == value) return;
            _isSimpleMode = value;
            PropertyChanged?.Invoke(this, new(nameof(IsSimpleMode)));
            PropertyChanged?.Invoke(this, new(nameof(Title)));
            PropertyChanged?.Invoke(this, new(nameof(HeaderRightLabel)));
        }
    }
    public string Title => IsSimpleMode ? "Liste des outils par catégorie" : "Liste des outils";
    
    public string HeaderRightLabel => IsSimpleMode ? "Mode simple" : "Mode expert";

    public event PropertyChangedEventHandler? PropertyChanged;
}