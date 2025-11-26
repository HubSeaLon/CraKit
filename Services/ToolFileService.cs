using Avalonia.Controls;
using CraKit.Models;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;

namespace CraKit.Services;

public interface IToolFileService
{
    Task<string?> PickAndUploadAsync(ToolFileModel toolFileModel, Window owner);
}

public class ToolFileService : IToolFileService
{
    private readonly ConnexionSshService _ssh;
    
    public ToolFileService(ConnexionSshService ssh)
    {
        _ssh = ssh;
    }

    public async Task<string?> PickAndUploadAsync(ToolFileModel toolFileModel, Window owner)
    {
        var storage = owner.StorageProvider;

        var result = await storage.OpenFilePickerAsync(
            new FilePickerOpenOptions
            {
                AllowMultiple = false,
                Title = toolFileModel == ToolFileModel.Wordlist
                    ? "Choisir une wordlist"
                    : "Choisir un hashfile",

                FileTypeFilter = new[]
                {
                    new FilePickerFileType("Text files")
                    {
                        Patterns = new [] { "*.txt" }
                    }
                }
            }
        );

        if (result == null || result.Count == 0)
            return null;

        var file = result[0];
        var localPath = file.Path.LocalPath;

        var fileName = Path.GetFileName(localPath);

        var remoteDir = toolFileModel switch
        {
            ToolFileModel.Wordlist => "/root/wordlists",
            ToolFileModel.HashFile => "/root/hashfiles",
            _ => "/root"
        };

        var remotePath = $"{remoteDir}/{fileName}";

        await _ssh.UploadFileAsync(localPath, remotePath);

        return remotePath;
    }

    
    
    
    
    
}