using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Opc.UaFx;
using Opc.UaFx.Client;

namespace opcUaPlc
{
    // Classe helper per la serializzazione JSON
    public class OpcVariableInfo
    {
        public string NodeId { get; set; }
        public string DisplayName { get; set; }
        public object Value { get; set; }
        public int Length { get; set; }
        public string StatusCode { get; set; }
        public string VariableType { get; set; }
    }

    public class OpcUaNodeSearch
    {
        private readonly OpcUaConnection _connection;
        private readonly List<OpcVariableInfo> _foundVariables = new List<OpcVariableInfo>();

        public OpcUaNodeSearch(OpcUaConnection connection)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        }

        public void ScanAndSave(string startNodeId, string outputFilePath)
        {
            if (!_connection.Status())
            {
                Console.WriteLine("Impossibile avviare la scansione, il client OPC UA non è connesso.");
                return;
            }

            Console.WriteLine($"Avvio scansione dal nodo: {startNodeId}");
            _foundVariables.Clear();
            
            BrowseChildren(new OpcNodeId(startNodeId));

            var options = new JsonSerializerOptions { WriteIndented = true };
            string jsonString = JsonSerializer.Serialize(_foundVariables, options);
            File.WriteAllText(outputFilePath, jsonString);
            Console.WriteLine($"Scansione completata. Trovate {_foundVariables.Count} variabili. Salvataggio in {outputFilePath}");
        }

        private void BrowseChildren(OpcNodeId nodeId)
        {
            try
            {
                var children = _connection.Client.BrowseNode(nodeId).Children();

                foreach (var childNode in children)
                {
                    if (childNode.Category == OpcNodeCategory.Variable)
                    {
                        var (success, value, length, statusCode, variableType) = _connection.ReadVariable(childNode.NodeId.ToString());
                        
                        _foundVariables.Add(new OpcVariableInfo
                        {
                            NodeId = childNode.NodeId.ToString(),
                            DisplayName = childNode.DisplayName,
                            Value = success ? value : $"READ_ERROR: {statusCode}",
                            Length = length,
                            StatusCode = statusCode.ToString(),
                            VariableType = variableType
                        });
                    }

                    if (childNode.Category == OpcNodeCategory.Object || childNode.Category == OpcNodeCategory.View)
                    {
                        BrowseChildren(childNode.NodeId);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore durante la navigazione del nodo {nodeId}: {ex.Message}");
            }
        }
    }
}