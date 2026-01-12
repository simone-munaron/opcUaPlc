using System;
using System.IO;
using System.Text.Json;

namespace opcUaPlc.opcUaConfig
{
    public class OpcConfigReader
    {
        // Ora la funzione accetta il percorso del file come parametro
        public static (string serverUrl, string username, string password) ReadConfig(string configFilePath)
        {
            try
            {
                // Se viene passata una cartella, aggiungiamo il nome del file standard
                if (Directory.Exists(configFilePath))
                {
                    configFilePath = Path.Combine(configFilePath, "config.json");
                }

                Console.WriteLine($"[Config] Tentativo di apertura: {configFilePath}");

                if (!File.Exists(configFilePath))
                    throw new FileNotFoundException($"File non trovato in: {configFilePath}");

                string json = File.ReadAllText(configFilePath);
                
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var config = JsonSerializer.Deserialize<OpcConfigModel>(json, options);

                Console.WriteLine("[Config] Caricato con successo!");
                return (config.ServerUrl, config.Username, config.Password);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Config] Errore: {ex.Message}");
                return ("", "", "");
            }
        }
    }

    public class OpcConfigModel
    {
        public string ServerUrl { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
}