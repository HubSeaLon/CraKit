using Avalonia.Controls;
using CraKit.Models;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;

namespace CraKit.Services;

// Service pour choisir et envoyer des fichiers sur le serveur
public class ToolFileService
{
    private ConnexionSshService ssh;
    
    // Constructeur
    public ToolFileService(ConnexionSshService sshService)
    {
        ssh = sshService;
    }

    // Choisir un fichier et l'envoyer sur le serveur
    public async Task<string> PickAndUploadAsync(ToolFileModel toolFileModel, Window owner)
    {
        var storage = owner.StorageProvider;

        // Determiner le titre selon le type de fichier
        string dialogTitle = "";
        
        if (toolFileModel == ToolFileModel.Wordlist)
            dialogTitle = "Choisir une wordlist";
        else if (toolFileModel == ToolFileModel.Userlist)
            dialogTitle = "Choisir une userlist";
        else if (toolFileModel == ToolFileModel.Combolist)
            dialogTitle = "Choisir une combolist";
        else if (toolFileModel == ToolFileModel.HashFile)
            dialogTitle = "Choisir un hashfile";
        else
            dialogTitle = "Choisir un fichier";

        // Ouvrir la fenetre pour choisir un fichier
        var result = await storage.OpenFilePickerAsync(
            new FilePickerOpenOptions
            {
                AllowMultiple = false,
                Title = dialogTitle,
                FileTypeFilter = new[]
                {
                    new FilePickerFileType("Text files")
                    {
                        Patterns = new [] { "*.txt" }
                    }
                }
            }
        );

        // Si aucun fichier choisi
        if (result.Count == 0)
            return null;

        // Recuperer le fichier choisi
        var file = result[0];
        string localPath = file.Path.LocalPath;
        string fileName = Path.GetFileName(localPath);

        // Determiner le dossier sur le serveur
        string remoteDir = "";
        
        if (toolFileModel == ToolFileModel.Wordlist)
            remoteDir = "/root/wordlists";
        else if (toolFileModel == ToolFileModel.Userlist)
            remoteDir = "/root/userlists";
        else if (toolFileModel == ToolFileModel.Combolist)
            remoteDir = "/root/combolists";
        else if (toolFileModel == ToolFileModel.HashFile)
            remoteDir = "/root/hashfiles";
        else
            remoteDir = "/root";

        string remotePath = remoteDir + "/" + fileName;

        // Envoyer le fichier
        await ssh.UploadFileAsync(localPath, remotePath);

        return remotePath;
    }
}

