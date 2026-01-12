using opcUaPlc.opcUaConfig;
using opcUaPlc;

class Program
{
    static void Main()
    {
        //Read config file config.json
        string path = @"C:\Prj\opcUaPlc\opcUaPlc\opcUaConfig\config.json"; //Esempio di percorso alternativo: string path = @"C:\Prj\OPC_UA_PLC\OPC_UA_PLC\config.json";
        var (serverUrl, username, password) = OpcConfigReader.ReadConfig(path);

        using (var opc = new OpcUaConnection(serverUrl, username, password))
        {
            opc.Start();  // Apre la connessione
            opc.Status(); // Ora dirà "Connesso" perché l'oggetto è lo stesso!

            Thread.Sleep(2000);

            opc.Stop();   // Chiude la connessione
            opc.Status(); // Dirà "Disconnected"
        }
        
    }
}
