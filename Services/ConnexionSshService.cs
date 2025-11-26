using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Renci.SshNet;
using System.IO;

namespace CraKit.Services;

public class ConnexionSshService : IDisposable, INotifyPropertyChanged
{
    
    // Injection de l'instance ConnexionSshService 
    public static ConnexionSshService Instance { get; } = new ConnexionSshService();
    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged(string n) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    
    
    private SshClient? _ssh; 
    private SftpClient? _sftp;
    public SshClient? Client => _ssh;
    public SftpClient? Sftp => _sftp;
    public bool IsConnected => _ssh?.IsConnected ?? false;
    public bool IsSftpConnected => _sftp?.IsConnected ?? false;

    public async Task<bool> ConnectAsync(string host, int port, string username, string password)
    {
        return await Task.Run(() =>
        {
            try
            {
                Disconnect();
                
                var auth = new PasswordAuthenticationMethod(username, password);
                var connectionInfo = new ConnectionInfo(host, port, username, auth);
                
                _ssh = new SshClient(connectionInfo)
                {
                    KeepAliveInterval = TimeSpan.FromSeconds(15)
                };
                _ssh.ConnectionInfo.Timeout = TimeSpan.FromSeconds(10);

                _sftp = new SftpClient(connectionInfo)
                {
                    KeepAliveInterval = TimeSpan.FromSeconds(15)
                };
                _sftp.ConnectionInfo.Timeout = TimeSpan.FromSeconds(10);
                
                _ssh.Connect();
                _sftp.Connect();
                
                OnPropertyChanged(nameof(IsConnected));
                OnPropertyChanged(nameof(IsSftpConnected));
                return _ssh.IsConnected && _sftp.IsConnected;
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
        if (_ssh != null && _sftp != null)
        {
            try
            {
                if (_ssh.IsConnected) _ssh.Disconnect(); // Deconnexion au SSH
                if (_sftp.IsConnected) _sftp.Disconnect(); // Déconnexion SFTP
                Console.WriteLine("[SSH-SFTP] Disconnected");
            }
            catch (Exception ex)
            {
                // Evite un crash si déjà connecté
                Console.WriteLine($"[SSH-SFTP] Erreur déconnexion : {ex.Message}");
            }   
            finally
            {
                // Le Dispose() de IDisposable permet de libérer la mémoire et le socket
                _ssh.Dispose(); // Libère le client SSH
                _ssh = null;     
                _sftp.Dispose();
                _sftp = null;
                Console.WriteLine("[SSH-SFTP] Libération ressources");
            }
        }
    }
    
    public async Task UploadFileAsync(string local, string remote)
    {
        if (!IsConnected || _sftp == null)
            throw new Exception("SFTP n'est pas connecté.");

        await Task.Run(() =>
        {
            using var fs = File.OpenRead(local);
            _sftp.UploadFile(fs, remote, true);
        });
    }
    
    public void Dispose() => Disconnect();
}