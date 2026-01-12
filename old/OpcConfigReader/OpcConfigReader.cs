// C:\Prj\OPC_UA_PLC\OpcConfigReader\OpcConfigReader.cs
using System.IO;
using System.Reflection;
using System.Text.Json;

namespace OPC_UA_PLC  // ← Namespace del progetto
{
    public class OpcConfigReader
    {
        public static (string serverUrl, string username, string password) ReadConfig()
        {
            string serverUrl = "", username = "", password = "";
            try
            {
                // Trova root progetto (dove è config.json)
                string assemblyPath = Assembly.GetExecutingAssembly().Location;
                string binDir = Path.GetDirectoryName(assemblyPath);
                string projectDir = Directory.GetParent(binDir).FullName;  // Root!
                
                string jsonPath = Path.Combine(projectDir, "config.json");
                Console.WriteLine($"[Config] Cerco: {jsonPath}");
                
                if (!File.Exists(jsonPath))
                    throw new FileNotFoundException($"Manca: {jsonPath}");
                
                string json = File.ReadAllText(jsonPath);
                var configData = JsonSerializer.Deserialize<JsonElement>(json);
                
                serverUrl = configData.GetProperty("serverUrl").GetString();
                username = configData.GetProperty("username").GetString();
                password = configData.GetProperty("password").GetString();
                
                Console.WriteLine("[Config] Caricato!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Config] Errore: {ex.Message}");
                serverUrl = "opc.tcp://10.69.111.1";
                username = "OPC_UA";
                password = "Testtest";
            }
            return (serverUrl, username, password);
        }
    }
}
