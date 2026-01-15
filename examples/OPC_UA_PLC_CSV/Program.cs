/*using System;
using System.IO;
using System.Text;
using System.Threading;
using Opc.UaFx;
using Opc.UaFx.Client;

namespace OpcUaPlcCsv
{
    public partial class Program
    {
        static void Main(string[] args)
        {
           string serverUrl = "opc.tcp://10.69.131.1";
string username = "OPC_UA";
string password = "L3asOpc_uA";

Console.Write("Inserisci il NodeId da leggere: ");
string nodeId = Console.ReadLine();

Console.Write("Nome file CSV (default: opc_log.csv): ");
string csvFileName = Console.ReadLine();
if (string.IsNullOrWhiteSpace(csvFileName))
    csvFileName = "opc_log.csv";

Console.Write("Intervallo in secondi (default: 1): ");
int intervalSeconds = 1;
if (int.TryParse(Console.ReadLine(), out int parsed))
    intervalSeconds = parsed;

int readCount = 0;

try
{
    using (var client = new OpcClient(serverUrl))
    {
        client.Security.UserIdentity = new OpcClientIdentity(username, password);
        client.Connect();

        Console.WriteLine("\n✓ Connesso al server OPC UA!");
        Console.WriteLine($"NodeId: {nodeId}");
        Console.WriteLine($"File: {Path.GetFullPath(csvFileName)}");
        Console.WriteLine($"Intervallo: {intervalSeconds}s");
        Console.WriteLine("\nPremi ESC per fermare...\n");

        bool fileExists = File.Exists(csvFileName);
        
        using (StreamWriter csv = new StreamWriter(csvFileName, append: true, Encoding.UTF8))
        {
            if (!fileExists)
            {
                csv.WriteLine("Timestamp,NodeId,Value,Type");//,Status");
            }

            while (true)
            {
                if (Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Escape)
                    break;

                try
                {
                    OpcValue value = client.ReadNode(nodeId);
                    string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                    readCount++;

                    string valueStr = value.Value?.ToString() ?? "null";
                    string typeStr = value.Value?.GetType().Name ?? "null";
                    
                    valueStr = EscapeCsvField(valueStr);
                    
                    csv.WriteLine($"{timestamp},{nodeId},{valueStr},{typeStr}");     //,{value.Status}");
                    csv.Flush();

                    Console.WriteLine($"[{readCount}] {timestamp} | {valueStr}");
                }
                catch (Exception ex)
                {
                    string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                    csv.WriteLine($"{timestamp},{nodeId},ERROR,ERROR,{EscapeCsvField(ex.Message)}");
                    csv.Flush();
                    
                    Console.WriteLine($"[{readCount}] {timestamp} | ERRORE: {ex.Message}");
                }

                Thread.Sleep(intervalSeconds * 1000);
            }
        }

        client.Disconnect();
        Console.WriteLine($"\n✓ Salvate {readCount} letture in: {Path.GetFullPath(csvFileName)}");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"\n✗ Errore: {ex.Message}");
}

Console.WriteLine("\nPremi un tasto per uscire...");
Console.ReadKey();

static string EscapeCsvField(string field)
{
    if (string.IsNullOrEmpty(field))
        return "";

    if (field.Contains(",") || field.Contains("\"") || field.Contains("\n"))
    {
        return $"\"{field.Replace("\"", "\"\"")}\"";
    }

    return field;
        }
    }
}
}

*/