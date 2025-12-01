using System;
using System.Threading.Tasks;
using Renci.SshNet;
using System.IO;

namespace CraKit.Services;

// Service pour se connecter au serveur Kali en SSH
public class ConnexionSshService
{
    // Instance partagee pour toute l'application
    public static ConnexionSshService Instance = new ConnexionSshService();
    
    // Clients SSH et SFTP
    private SshClient sshClient;
    private SftpClient sftpClient;
    
    // Pour savoir si on est connecte
    public bool EstConnecte()
    {
        if (sshClient == null)
            return false;
            
        return sshClient.IsConnected;
    }
    
    // Recuperer le client SSH
    public SshClient Client
    {
        get { return sshClient; }
    }
    
    // Recuperer le client SFTP
    public SftpClient Sftp
    {
        get { return sftpClient; }
    }

    // Se connecter au serveur
    public async Task<bool> ConnectAsync(string host, int port, string username, string password)
    {
        return await Task.Run(() =>
        {
            try
            {
                // Si deja connecte, on deconnecte d'abord
                Disconnect();
                
                // Creer les informations de connexion
                var auth = new PasswordAuthenticationMethod(username, password);
                var connectionInfo = new ConnectionInfo(host, port, username, auth);
                
                // Creer le client SSH
                sshClient = new SshClient(connectionInfo);
                sshClient.ConnectionInfo.Timeout = TimeSpan.FromSeconds(10);
                
                // Creer le client SFTP
                sftpClient = new SftpClient(connectionInfo);
                sftpClient.ConnectionInfo.Timeout = TimeSpan.FromSeconds(10);
                
                // Se connecter
                sshClient.Connect();
                sftpClient.Connect();
                
                Console.WriteLine("[SSH] Connexion reussie!");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("[SSH] Erreur connexion : " + ex.Message);
                return false;
            }
        });
    }
    
    // Se deconnecter
    private void Disconnect()
    {
        if (sshClient != null)
        {
            try
            {
                if (sshClient.IsConnected)
                {
                    sshClient.Disconnect();
                }
                
                if (sftpClient != null && sftpClient.IsConnected)
                {
                    sftpClient.Disconnect();
                }
                
                Console.WriteLine("[SSH] Deconnexion OK");
            }
            catch (Exception ex)
            {
                Console.WriteLine("[SSH] Erreur deconnexion : " + ex.Message);
            }
        }
    }
    
    // Envoyer un fichier sur le serveur
    public async Task UploadFileAsync(string localPath, string remotePath)
    {
        if (!EstConnecte() || sftpClient == null)
        {
            throw new Exception("SFTP pas connecte");
        }

        await Task.Run(() =>
        {
            FileStream fileStream = File.OpenRead(localPath);
            sftpClient.UploadFile(fileStream, remotePath, true);
            fileStream.Close();
            
            Console.WriteLine("[SFTP] Fichier envoye : " + localPath + " -> " + remotePath);
        });
    }
}

