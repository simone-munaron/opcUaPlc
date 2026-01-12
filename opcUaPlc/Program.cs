using opcUaPlc.opcUaConfig;

class Program
{
    static void Main()
    {
        // CORRETTO: Deconstruct tuple (non passa parametri)
        var (serverUrl, username, password) = opcConfigReader.ReadConfig();
        
        // Oppure assegna separatamente
        // (string serverUrl, string username, string password) config = opcConfigReader.ReadConfig();
        
        Console.WriteLine($"Server: {serverUrl}");
        Console.WriteLine($"User: {username}");
        // Usa per OPC UA...
    }
}
