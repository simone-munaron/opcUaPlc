using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Opc.UaFx;
using Opc.UaFx.Client;

namespace opcUaPlc
{
    public class OpcUaNodeSearch
    {
        private readonly OpcUaConnection _connection;
        private readonly List<OpcNodeItem> _nodeItems = new List<OpcNodeItem>();

        public OpcUaNodeSearch(OpcUaConnection connection)
        {
            _connection = connection;
        }

        public void ScanAndSave(string startNodeId, string outputFilePath)
        {
            _nodeItems.Clear();
            
            if (_connection.Client == null)
            {
                Console.WriteLine("Client non inizializzato.");
                return;
            }

            Console.WriteLine($"Scansione iniziata da: {startNodeId}");
            
            // OpcNodeId supporta la conversione implicita da stringa
            BrowseVariables(_connection.Client, startNodeId, 0);

            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(_nodeItems, options);
                File.WriteAllText(outputFilePath, json);
                Console.WriteLine($"\nScansione completata. Salvati {_nodeItems.Count} nodi in: {outputFilePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore durante il salvataggio del file: {ex.Message}");
            }
        }

        public void ScanChildrenAndSave(string parentNodeId, string outputFilePath)
        {
            _nodeItems.Clear();
            
            if (_connection.Client == null)
            {
                Console.WriteLine("Client non inizializzato.");
                return;
            }

            Console.WriteLine($"Scansione figli iniziata da: {parentNodeId}");
            
            // OpcNodeId supporta la conversione implicita da stringa
            BrowseVariables(_connection.Client, parentNodeId, 0);

            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(_nodeItems, options);
                File.WriteAllText(outputFilePath, json);
                Console.WriteLine($"\nScansione figli completata. Salvati {_nodeItems.Count} nodi in: {outputFilePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore durante il salvataggio del file: {ex.Message}");
            }
        }

        private void BrowseVariables(OpcClient client, OpcNodeId nodeId, int level)
        {
            try
            {
                var nodeInfo = client.BrowseNode(nodeId);

                foreach (var childNode in nodeInfo.Children())
                {
                    // Filtra per categoria
                    if (childNode.Category == OpcNodeCategory.Variable)
                    {
                        string indent = new string(' ', level * 2);
                        string valueStr;
                        
                        try
                        {
                            // Leggi il valore
                            var value = client.ReadNode(childNode.NodeId);
                            Console.WriteLine($"{indent}{childNode.DisplayName} = {value} | NodeId: {childNode.NodeId}");
                            valueStr = value?.ToString() ?? "null";
                        }
                        catch
                        {
                            Console.WriteLine($"{indent}{childNode.DisplayName} (non leggibile) | NodeId: {childNode.NodeId}");
                            valueStr = "(non leggibile)";
                        }

                        // Aggiungi alla lista per il JSON
                        _nodeItems.Add(new OpcNodeItem
                        {
                            DisplayName = childNode.DisplayName.ToString(),
                            NodeId = childNode.NodeId.ToString(),
                            Value = valueStr
                        });
                    }

                    // Continua la ricorsione
                    BrowseVariables(client, childNode.NodeId, level + 1);
                }
            }
            catch (Exception)
            {
                // Ignora errori di accesso
            }
        }
    }


    public class OpcNodeItem
    {
        public string DisplayName { get; set; } = "";
        public string NodeId { get; set; } = "";
        public string Value { get; set; } = "";
    }
}