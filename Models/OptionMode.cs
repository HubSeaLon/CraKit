namespace CraKit.Models;

// Classe simple pour stocker les options des outils
public class OptionMode
{
    // La valeur de l'option (ex: "ssh", "16")
    public string value
    {
        get { return valueField; }
        set { valueField = value; }
    }
    private string valueField;
    
    // Le texte affiche (ex: "SSH (Secure Shell)", "16 threads")
    public string label
    {
        get { return labelField; }
        set { labelField = value; }
    }
    private string labelField;

    // Constructeur
    public OptionMode()
    {
        valueField = "";
        labelField = "";
    }
}

