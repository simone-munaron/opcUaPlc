using System;
using System.IO;
using System.Text.Json;

namespace opcUaPlc

{
    public class OpcConfigReader
    {
        // Ora la funzione accetta il percorso del file come parametro
        public static (bool success, OpcConfigModel? config, string message) ReadConfig(string configFilePath)
        {
            try
            {
                // Se viene passata una cartella, aggiungiamo il nome del file standard
                if (Directory.Exists(configFilePath))
                {
                    configFilePath = Path.Combine(configFilePath, "config.json");
                }

                //Console.WriteLine($"[Config] Tentativo di apertura: {configFilePath}");

                if (!File.Exists(configFilePath))
                    throw new FileNotFoundException($"File non trovato in: {configFilePath}");

                string json = File.ReadAllText(configFilePath);
                
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var config = JsonSerializer.Deserialize<OpcConfigModel>(json, options);

                //Console.WriteLine("[Config] Caricato con successo!");
                return (true, config, "");
            }
            catch (Exception ex)
            {
                //Console.WriteLine($"[Config] Errore: {ex.Message}");
                return (false, null, ex.Message);
            }
        }
    }

    public class OpcConfigModel
    {
        public required string ServerUrl { get; set; }
        public required string Username { get; set; }
        public required string Password { get; set; }
        public float LogIntervalSeconds { get; set; } = 10;
        public string LogDatabasePath { get; set; } = "opc_log.db";
        public string NodesListPath { get; set; } = "opcUaNodesList.json";
        public string ProgramLogPath { get; set; } = "ProgramLog.csv";
    }
}