using System;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
using Renci.SshNet;

namespace CraKit.Services;

// Service pour executer des commandes SSH
public class ExecuterCommandeService
{
    private ConnexionSshService sshService;
    private SshCommand commandeEnCours;

    // Constructeur
    public ExecuterCommandeService(ConnexionSshService ssh)
    {
        sshService = ssh;
        commandeEnCours = null;
    }
    
    // Executer une commande simple
    public async Task<string> ExecuteCommandAsync(string command, TimeSpan timeout)
    {
        SshClient client = sshService.Client;
        
        if (client == null || !client.IsConnected)
        {
            return "[SSH] Non connecte";
        }
        
        return await Task.Run(() =>
        {
            try
            {
                SshCommand cmd = client.CreateCommand(command);
                cmd.CommandTimeout = timeout;
                
                string output = cmd.Execute();
                string error = cmd.Error;

                if (string.IsNullOrWhiteSpace(error))
                {
                    return output;
                }
                else
                {
                    return output + "\n[stderr]\n" + error;
                }
            }
            catch (Exception ex)
            {
                return "[SSH] Erreur execution : " + ex.Message;
            }
        });
    }
    
    // Executer une commande en temps reel (pour voir le resultat ligne par ligne)
    public async Task ExecuteCommandStreamingAsync(
        string command,
        Action<string> onLineReceived,
        Action<string> onError,
        CancellationToken cancel)
    {
        SshClient client = sshService.Client;

        if (client == null || !client.IsConnected)
        {
            onError("[SSH] Non connecte");
            return;
        }

        await Task.Run(() =>
        {
            try
            {
                commandeEnCours = client.CreateCommand(command);
                IAsyncResult asyncResult = commandeEnCours.BeginExecute();

                StreamReader reader = new StreamReader(commandeEnCours.OutputStream);

                while (!asyncResult.IsCompleted || !reader.EndOfStream)
                {
                    // Verifier si on veut annuler
                    if (cancel.IsCancellationRequested)
                    {
                        commandeEnCours.CancelAsync();
                        break;
                    }

                    // Lire une ligne
                    string line = reader.ReadLine();
                    if (line != null)
                    {
                        onLineReceived(line);
                    }
                }

                commandeEnCours.EndExecute(asyncResult);
                reader.Close();
            }
            catch (Exception ex)
            {
                if (!ex.Message.Contains("canceled"))
                {
                    onError(ex.Message);
                }
            }
            finally
            {
                commandeEnCours = null;
            }
        });
    }
    
    // Arreter la commande en cours
    public void StopCurrent()
    {
        try
        {
            if (commandeEnCours != null)
            {
                commandeEnCours.CancelAsync();
                Console.WriteLine("[SSH] Arret commande en cours");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("[SSH] Erreur arret : " + ex.Message);
        }
    }
}

