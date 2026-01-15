using Opc.Ua;
using opcUaPlc;

class Program
{
    // L'istanza vive qui fuori, quindi non viene distrutta alla fine di un metodo
    private static OpcUaConnection? _OpcUaConnection;
    static void Main()
    {
        Console.WriteLine("\n\n"); // Spazio per la leggibilità nella console

        //Read config file config.json
        var (serverUrl, username, password) = OpcConfigReader.ReadConfig(@"C:\Prj\opcUaPlc\opcUaConfig\opcUaConfig.json");


        // Inizializzo la connessione globale
        _OpcUaConnection = new OpcUaConnection(serverUrl, username, password);
        bool status;   
        status = _OpcUaConnection.Start(); // Avvio la connessione
        status = _OpcUaConnection.Status(); // Mostro lo stato della connessione

        var (success, value, length, statusCode) = _OpcUaConnection.ReadVariable(@"ns=3;s=""IFM"".""IOLink_SV4200""[2].""Sts"".""Flow"""); // Leggo una variabile di esempio
        Console.WriteLine($"Lettura variabile: Success={success}, Value={value}, Length={length}, StatusCode={statusCode}");

        status = _OpcUaConnection.Stop(); // Chiudo la connessione alla fine del programma

        
        // Mantiene aperta la console fino a quando non si preme Invio
        Console.WriteLine("\n\n\n********** Press enter to exit **********");
        Console.ReadLine();
    }
}
