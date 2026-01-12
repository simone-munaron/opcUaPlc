using System.IO;
using System.Reflection;
using System.Text.Json;

namespace opcUaPlc.opcUaConfig  // ← Namespace del progetto
{
    public class opcConfigReader
    {
        public static (string serverUrl, string username, string password) ReadConfig()
        {
            string serverUrl = "", username = "", password = "";
            try
            {
                //// Trova root progetto (dove è config.json)
                //string assemblyPath = Assembly.GetExecutingAssembly().Location;
                //string binDir = Path.GetDirectoryName(assemblyPath);
                //string projectDir = Directory.GetParent(binDir).FullName;  // Root!

                // Root progetto: dove sono i file .cs e config.json
                //string assemblyPath = Assembly.GetExecutingAssembly().Location;
                //string binDir = Path.GetDirectoryName(assemblyPath);
                //string projectDir = Directory.GetParent(binDir).FullName;  // Sale da bin/Debug/ alla root
                

                // Parte da EXE → risale fino opcUaPlcConfig
                string assemblyPath = Assembly.GetExecutingAssembly().Location;  // ...opcUaPlc\bin\Debug\EXE.dll
                string netVersion = Path.GetDirectoryName(assemblyPath);    // .net version
                string currentDir = Path.GetDirectoryName(netVersion);         // ...opcUaPlc\bin\Debug\
                string debugDir = Path.GetDirectoryName(currentDir);             // ...opcUaPlc\bin\
                string opcUaPlcDir = Path.GetDirectoryName(debugDir);            // ...opcUaPlc\
                string opcUaPlcConfigDir = Path.Combine(opcUaPlcDir, "opcUaConfig");  // ...opcUaPlc\opcUaPlcConfig ✓

                string jsonPath = Path.Combine(opcUaPlcConfigDir, "config.json");

                //string jsonPath = Path.Combine(projectDir, "config.json");
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
                serverUrl = "";
                username = "";
                password = "";
            }
            return (serverUrl, username, password);
        }
    }
}
