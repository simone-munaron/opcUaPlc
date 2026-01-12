using opcUaPlc;

class Program
{
    // L'istanza vive qui fuori, quindi non viene distrutta alla fine di un metodo
    private static OpcUaConnection? _OpcUaConnection;
    static void Main()
    {
        //Read config file config.json
        string path = @"C:\Prj\opcUaPlc\opcUaPlc\opcUaConfig\config.json"; //Esempio di percorso alternativo: string path = @"C:\Prj\OPC_UA_PLC\OPC_UA_PLC\config.json";
        var (serverUrl, username, password) = OpcConfigReader.ReadConfig(path);

        // Inizializzo la connessione globale
        _OpcUaConnection = new OpcUaConnection(serverUrl, username, password);
        
        // Avvio la connessione
        _OpcUaConnection.Start(); 
        // Controllo lo stato della connessione
        _OpcUaConnection.Status();

        Thread.Sleep(2000);
        // Chiudo la connessione alla fine del programma
        _OpcUaConnection.Stop();
        

    }
}
