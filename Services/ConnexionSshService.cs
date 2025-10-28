using System;
using System.Threading.Tasks;
using Renci.SshNet;

namespace CraKit.Services;

public class ConnexionSshService : IDisposable
{
    
    private SshClient? _ssh;
    public SshClient? Client => _ssh;
    public  bool IsConnected => _ssh?.IsConnected ?? false;

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
                return _ssh.IsConnected;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SSH] Erreur connexion : {ex.Message}");
                return false;
            }
        });
    }
    
    
    public void Disconnect()
    {
        if (_ssh != null)
        {
            try
            {
                if (_ssh.IsConnected)
                    _ssh.Disconnect();
            }
            catch { }
            finally
            {
                _ssh.Dispose();
                _ssh = null;
            }
        }
    }
    
    public void Dispose() => Disconnect();
}