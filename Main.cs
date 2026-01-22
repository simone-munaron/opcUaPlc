using System.IO;
using System.Text.Json;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using Opc.Ua;
using opcUaPlc;

class Program
{
    // L'istanza vive qui fuori, quindi non viene distrutta alla fine di un metodo
    private static OpcUaConnection? _OpcUaConnection;
    private static readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

    static void Main()
    {
        Console.WriteLine("\n\n"); // Spazio per la leggibilità nella console

        // Gestione chiusura pulita con Ctrl+C
        Console.CancelKeyPress += (sender, e) =>
        {
            e.Cancel = true; // Impedisce la terminazione immediata del processo
            _cancellationTokenSource.Cancel();
            Console.WriteLine("\nRichiesta di arresto ricevuta. Chiusura pulita in corso...");
        };
        //Read config file config.json
        var (fileReadSuccess, config, fileReadMessage) = OpcConfigReader.ReadConfig(@"C:\Prj\opcUaPlc\opcUaConfig\opcUaConfig.json");
        if (!fileReadSuccess || config == null)
        {
            Console.WriteLine("Errore durante la lettura del file di configurazione: Codice errore:");
            Console.WriteLine(fileReadMessage);
            return;
        }



        // Inizializzo la connessione globale
        _OpcUaConnection = new OpcUaConnection(config.ServerUrl, config.Username, config.Password);
        if (!_OpcUaConnection.Start()) // Avvio la connessione
        {
            Console.WriteLine("Impossibile avviare la connessione OPC UA. Il programma terminerà.");
        }
        else
        {
            // Carica la lista dei nodi dal file JSON configurato
            var nodesToLog = new List<string>();
            if (File.Exists(config.NodesListPath))
            {
                try
                {
                    string jsonNodes = File.ReadAllText(config.NodesListPath);
                    var nodeItems = JsonSerializer.Deserialize<List<OpcNodeItem>>(jsonNodes);
                    if (nodeItems != null)
                    {
                        nodesToLog = nodeItems.Select(n => n.NodeId).ToList();
                        //Console.WriteLine($"[Log] Caricati {nodesToLog.Count} nodi da: {config.NodesListPath}");
                        ProgramLog.Log(config.ProgramLogPath, $"[Log] Caricati {nodesToLog.Count} nodi da: {config.NodesListPath}");
                    }
                }
                catch (Exception ex)
                {
                    //Console.WriteLine($"[Log] Errore lettura file nodi: {ex.Message}");
                    ProgramLog.Log(config.ProgramLogPath, $"[Log] Errore lettura file nodi: {ex.Message}");
                }
            }
            else
            {
                //Console.WriteLine($"[Log] File lista nodi non trovato: {config.NodesListPath}");
                ProgramLog.Log(config.ProgramLogPath, $"[Log] File lista nodi non trovato: {config.NodesListPath}");
            }

            // --- LOGGING ---
            // Inizializza il logger con i parametri da config
            var logger = new OpcUaNodeLog(_OpcUaConnection!, config.LogDatabasePath);

            if (nodesToLog.Any())
            {
                Console.WriteLine("\nAvvio del ciclo di logging. Premere Ctrl+C per arrestare in modo pulito.");
                while (!_cancellationTokenSource.IsCancellationRequested)
                {
                    logger.LogNodes(nodesToLog);
                    ProgramLog.Log(config.ProgramLogPath, $"[Log] Ciclo di logging completato alle {DateTime.Now:HH:mm:ss}");
                    try
                    {
                        // Attesa cancellabile, più pulita di Thread.Sleep
                        Task.Delay((int)(config.LogIntervalSeconds * 1000), _cancellationTokenSource.Token).Wait();
                    }
                    catch (OperationCanceledException)
                    {
                        // Previsto quando si preme Ctrl+C, esce dal ciclo
                        break;
                    }
                }
            }
            else
            {
                ProgramLog.Log(config.ProgramLogPath, "[Log] Nessun nodo da monitorare. Il programma attende.");
                // Attendi la richiesta di chiusura se non ci sono nodi da loggare
                _cancellationTokenSource.Token.WaitHandle.WaitOne();
            }

            _OpcUaConnection.Stop(); // Chiudo la connessione alla fine del programma
        }
        Console.WriteLine("Programma terminato.");
    }
}
