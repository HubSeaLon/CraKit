using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using Renci.SshNet;

namespace CraKit.Services;

public class ConnexionSshService : IDisposable, INotifyPropertyChanged
{
    
    // Injection de l'instance ConnexionSshService 
    public static ConnexionSshService Instance { get; } = new ConnexionSshService();
    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged(string n) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    
    
    private SshClient? _ssh;
    public SshClient? Client => _ssh;
    public bool IsConnected => _ssh?.IsConnected ?? false;

    public async Task<bool> ConnectAsync(string host, int port, string username, string password)
    {
        return await Task.Run(() =>
        {
            try
            {
                Disconnect();

                _ssh = new SshClient(host, port, username, password)
                {
                    KeepAliveInterval = TimeSpan.FromSeconds(15)
                };
                _ssh.ConnectionInfo.Timeout = TimeSpan.FromSeconds(10);

                _ssh.Connect();
                OnPropertyChanged(nameof(IsConnected));
                return _ssh.IsConnected;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SSH] Erreur connexion : {ex.Message}");
                return false;
            }
        });
    }
    
    private void Disconnect()
    {
        if (_ssh != null)
        {
            try
            {
                if (_ssh.IsConnected) _ssh.Disconnect();  // Deconnexion au SSH
            }
            catch { }   // Evite un crash si déjà connecté
            finally
            {
                // Le Dispose() de IDisposable permet de libérer la mémoire et le socket
                _ssh.Dispose(); // Libère le client SSH
                _ssh = null;     
            }
        }
    }
    public void Dispose() => Disconnect();
}