/*
using System;
using Opc.UaFx;
using Opc.UaFx.Client;
using System.Text.Json;
using System.IO;

string serverUrl = "";
string username = "";
string password = "";

try
{
    // Percorso dalla root del progetto (cartella .cs)
    //string projectDir = Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory);
    //projectDir = Directory.GetParent(projectDir).FullName;  // Sale di un livello da bin/
    string projectDir = @"C:\Prj\OPC_UA_PLC\OPC_UA_PLC";

    string jsonPath = Path.Combine(projectDir, "config.json");
    Console.WriteLine($"Cerco config.json in: {jsonPath}");
    
    if (!File.Exists(jsonPath))
        throw new FileNotFoundException($"File non trovato: {jsonPath}");
    
    string json = File.ReadAllText(jsonPath);
    var config = JsonSerializer.Deserialize<JsonElement>(json);
    serverUrl = config.GetProperty("serverUrl").GetString();
    username = config.GetProperty("username").GetString();
    password = config.GetProperty("password").GetString();
    
    Console.WriteLine("Config caricato dalla root progetto!");
}
catch
{
    // Fallback hardcoded solo se necessario
    Console.WriteLine("Non è stato possibile leggere il file config.json");   

    // Aspetta tasto per vedere output
    //Console.WriteLine("Premi un tasto per terminare...");
    //Console.ReadKey();  // Pausa comune

    // Oppure timeout specifico
    Console.WriteLine("Terminando in 3 secondi...");
    Thread.Sleep(3000);
    return;
}

Console.Write("Inserisci il NodeId da leggere: ");
string nodeId = Console.ReadLine();

try
{
    using (var client = new OpcClient(serverUrl))
    {
        client.Security.UserIdentity = new OpcClientIdentity(username, password);
        client.Connect();

        Console.WriteLine("\nConnesso al server OPC UA!");
        Console.WriteLine($"Lettura di: {nodeId}\n");

        OpcValue value = client.ReadNode(nodeId);

        if (value.Status.IsGood)
        {
            if (value.Value is byte[] byteArray)
            {
                Console.WriteLine($"Tipo: byte array (lunghezza: {byteArray.Length})");
                Console.WriteLine($"Byte raw: {BitConverter.ToString(byteArray)}\n");

                if (byteArray.Length == 4)
                {
                    float floatValue = BitConverter.ToSingle(byteArray, 0);
                    Console.WriteLine($"Valore (Float): {floatValue}");
                }
                else if (byteArray.Length == 8)
                {
                    double doubleValue = BitConverter.ToDouble(byteArray, 0);
                    Console.WriteLine($"Valore (Double): {doubleValue}");
                }
                else if (byteArray.Length == 2)
                {
                    short int16Value = BitConverter.ToInt16(byteArray, 0);
                    ushort uint16Value = BitConverter.ToUInt16(byteArray, 0);
                    Console.WriteLine($"Valore (Int16): {int16Value}");
                    Console.WriteLine($"Valore (UInt16): {uint16Value}");
                }
                else
                {
                    Console.WriteLine("Lunghezza byte array non gestita");
                }
            }
            else
            {
                Console.WriteLine($"Valore: {value.Value}");
                Console.WriteLine($"Tipo: {value.Value?.GetType().Name}");
            }

            Console.WriteLine($"Timestamp: {value.SourceTimestamp}");
        }
        else
        {
            Console.WriteLine($"Errore nella lettura: {value.Status}");
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Errore: {ex.Message}");
}

Console.WriteLine("\nPremi un tasto per uscire...");
Console.ReadKey();
*/