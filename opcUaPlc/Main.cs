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
        bool status;   
        status = _OpcUaConnection.Start(); // Avvio la connessione
            if (status)
            {
                Console.WriteLine("true");
            }
            else
            {
                Console.WriteLine("false");
            }

        status = _OpcUaConnection.Status(); // Mostro lo stato della connessione
            if (status)
            {
                Console.WriteLine("true");
            }
            else
            {
                Console.WriteLine("false");
            }


        status = _OpcUaConnection.Stop(); // Chiudo la connessione alla fine del programma
            if (status)
            {
                Console.WriteLine("true");
            }
            else
            {
                Console.WriteLine("false");
            }

    }
}
