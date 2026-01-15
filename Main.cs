using opcUaPlc;

class Program
{
    // L'istanza vive qui fuori, quindi non viene distrutta alla fine di un metodo
    private static OpcUaConnection? _OpcUaConnection;
    static void Main()
    {
        Console.WriteLine("\n\n"); // Spazio per la leggibilità nella console

        //Read config file config.json
        //string path = @"C:\Prj\opcUaPlc\opcUaConfig\opcUaConfig.json"; //Esempio di percorso alternativo: string path = @"C:\Prj\OPC_UA_PLC\OPC_UA_PLC\opcUaConfig.json";
        //var (serverUrl, username, password) = OpcConfigReader.ReadConfig(path);
        var (serverUrl, username, password) = OpcConfigReader.ReadConfig(@"C:\Prj\opcUaPlc\opcUaConfig\opcUaConfig.json");


        // Inizializzo la connessione globale
        _OpcUaConnection = new OpcUaConnection(serverUrl, username, password);
        bool status;   
        status = _OpcUaConnection.Start(); // Avvio la connessione
            if (status){
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
        
        // Mantiene aperta la console fino a quando non si preme Invio
        Console.WriteLine("\n\n\n********** Press enter to exit **********");
        Console.ReadLine();
    }
}
