using Opc.Ua;
using Opc.UaFx;
using opcUaPlc;

class Program
{
    // L'istanza vive qui fuori, quindi non viene distrutta alla fine di un metodo
    private static OpcUaConnection? _OpcUaConnection;
    static void Main()
    {
        Console.WriteLine("\n\n"); // Spazio per la leggibilità nella console

        //Read config file config.json
        var (fileReadSuccess, serverUrl, username, password, fileReadMessage) = OpcConfigReader.ReadConfig(@"C:\Prj\opcUaPlc\opcUaConfig\opcUaConfig.json");
        if (!fileReadSuccess)
        {
            Console.WriteLine("Errore durante la lettura del file di configurazione: Codice errore:");
            Console.WriteLine(fileReadMessage);
            return;
        }



        // Inizializzo la connessione globale
        _OpcUaConnection = new OpcUaConnection(serverUrl, username, password);
        bool status;   
        status = _OpcUaConnection.Start(); // Avvio la connessione
        status = _OpcUaConnection.Status(); // Mostro lo stato della connessione
        
        var (success, value, length, statusCode, variableType) = _OpcUaConnection.ReadVariable(@"ns=3;s=""IFM"".""IOLink_SV4200""[2].""Sts"".""Flow"""); // Leggo una variabile di esempio
        Console.WriteLine($"Lettura variabile: Success={success}, Value={value}, Length={length}, StatusCode={statusCode}, Type={variableType}");

        if (status)
        {
            // Avvia la scansione dei nodi e salva su file JSON
            Console.WriteLine("\nAvvio scansione nodi...");
            var nodeSearch = new OpcUaNodeSearch(_OpcUaConnection);
            nodeSearch.ScanAndSave(OpcObjectTypes.ObjectsFolder.ToString(), "opcUaNodes.json");
            Console.WriteLine("Scansione nodi terminata.");
        }

        status = _OpcUaConnection.Stop(); // Chiudo la connessione alla fine del programma

        
        // Mantiene aperta la console fino a quando non si preme Invio
        Console.WriteLine("\n\n\n********** Press enter to exit **********");
        Console.ReadLine();
    }
}
