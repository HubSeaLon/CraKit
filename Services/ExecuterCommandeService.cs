using System;
using System.Threading.Tasks;
using Renci.SshNet;

namespace CraKit.Services;

public class ExecuterCommandeService
{

    private readonly ConnexionSshService _sshService;
    
    public ExecuterCommandeService(ConnexionSshService sshService)
        => _sshService = sshService;
    
    public async Task<string> ExecuteCommandAsync(string command, TimeSpan? timeout = null)
    {
        var client = _sshService.Client;
        if (client == null || !client.IsConnected)
            return "[SSH] Non connecté.";

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
    
    // Exécution en streaming (temps réel)
    /*
    public Task ExecuteCommandStreamAsync(
        string command,
        Action<string> onOutput,
        CancellationToken cancellationToken,
        int cols = 200, int rows = 50)
    {
        var client = _sshService.Client ?? throw new InvalidOperationException("SSH non connecté.");

        return Task.Run(() =>
        {
            var term = new Dictionary<TerminalModes, uint>();
            using var shell = client.CreateShellStream("xterm", cols, rows, cols, rows, 1024, term);

            shell.WriteLine(command);

            var buffer = new byte[4096];
            while (!cancellationToken.IsCancellationRequested && shell.CanRead)
            {
                if (shell.DataAvailable)
                {
                    var read = shell.Read(buffer, 0, buffer.Length);
                    if (read > 0)
                        onOutput(Encoding.UTF8.GetString(buffer, 0, read));
                }
                else
                {
                    Task.Delay(50, cancellationToken).Wait(cancellationToken);
                }
            }

            // tenter un Ctrl+C à l’annulation
            if (cancellationToken.IsCancellationRequested && shell.CanWrite)
                shell.Write("\x03");
        }, cancellationToken);
    } */
}