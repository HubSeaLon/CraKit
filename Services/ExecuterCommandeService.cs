using System;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
using Renci.SshNet;

namespace CraKit.Services;

public class ExecuterCommandeService
{

    private readonly ConnexionSshService _sshService;
    private SshCommand? _currentCommand;

    public ExecuterCommandeService(ConnexionSshService sshService)
    {
        _sshService = sshService;
    }
    
    public async Task<string> ExecuteCommandAsync(string command, TimeSpan? timeout = null)
    {
        var client = _sshService.Client;
        
        if (client == null || !client.IsConnected) return "[SSH] Non connecté";
        
        return await Task.Run(() =>
        {
            try
            {
                using var cmd = client.CreateCommand(command);
                if (timeout.HasValue) cmd.CommandTimeout = timeout.Value;
                var output = cmd.Execute();
                var error = cmd.Error;

                return string.IsNullOrWhiteSpace(error)
                    ? output
                    : $"{output}\n[stderr]\n{error}";
            }
            catch (Exception ex)
            {
                return $"[SSH] Erreur exécution : {ex.Message}";
            }
        });
    }
    
    
    // Exécution en streaming (temps réel pour les commandes d'énumération)
   
     
    public async Task ExecuteCommandStreamingAsync(
        string command,
        Action<string> onLineReceived,
        Action? onCompleted = null,
        Action<string>? onError = null,
        CancellationToken? cancel = null)
    {
        var client = _sshService.Client;

        if (client == null || !client.IsConnected)
        {
            onError?.Invoke("[SSH] Non connecté");
            return;
        }

        await Task.Run(() =>
        {
            try
            {
                // Eviter que John garde l'historique et show le mot de passe à chaque exécution (marche pas encore)
                if (command.TrimStart().StartsWith("john "))
                {
                    command += " --pot=/dev/null";
                }

                _currentCommand = client.CreateCommand(command);
                var asyncResult = _currentCommand.BeginExecute();

                using var reader = new StreamReader(_currentCommand.OutputStream);

                while (!asyncResult.IsCompleted || !reader.EndOfStream)
                {
                    if (cancel?.IsCancellationRequested == true)
                    {
                        _currentCommand.CancelAsync();
                        break;
                    }

                    var line = reader.ReadLine();
                    if (line != null)
                        onLineReceived?.Invoke(line);
                }

                _currentCommand.EndExecute(asyncResult);
                onCompleted?.Invoke();
            }
            catch (Exception ex)
            {
                if (!ex.Message.Contains("command was canceled"))
                    onError?.Invoke(ex.Message);
            }
            finally
            {
                _currentCommand = null;
            }
        });
    }
    
    public void StopCurrent()
    {
        try
        {
            _currentCommand?.CancelAsync();
            Console.WriteLine("[SSH] Stopping current command");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SSH STOP Command] Error: {ex.Message}");
        }
    }
}