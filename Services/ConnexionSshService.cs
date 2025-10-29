using System;
using System.ComponentModel;
using System.Threading.Tasks;
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
                if (_ssh.IsConnected) _ssh.Disconnect(); // Deconnexion au SSH
                Console.WriteLine("[SSH] Disconnected");
            }
            catch (Exception ex)
            {
                // Evite un crash si déjà connecté
                Console.WriteLine($"[SSH] Erreur déconnexion : {ex.Message}");
            }   
            finally
            {
                // Le Dispose() de IDisposable permet de libérer la mémoire et le socket
                _ssh.Dispose(); // Libère le client SSH
                _ssh = null;     
                Console.WriteLine("[SSH] Libération ressources");
            }
        }
    }
    public void Dispose() => Disconnect();
}