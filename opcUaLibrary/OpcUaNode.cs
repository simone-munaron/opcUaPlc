using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using Opc.Ua;
using Opc.Ua.Client;

namespace opcUaPlc
{
    public class OpcUaNode
    {
        private readonly OpcUaConnection _connection;
        private readonly List<OpcNodeItem> _nodeItems = new List<OpcNodeItem>();

        public OpcUaNode(OpcUaConnection connection)
        {
            _connection = connection;
        }

        public void ScanAndSave(string startNodeId, string outputFilePath)
        {
            _nodeItems.Clear();
            
            if (_connection.Session == null)
            {
                Console.WriteLine("Client non inizializzato.");
                return;
            }

            Console.WriteLine($"Scansione iniziata da: {startNodeId}");
            
            // NodeId.Parse converte la stringa in NodeId
            BrowseVariables(_connection.Session, NodeId.Parse(startNodeId), 0);

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
            
            if (_connection.Session == null)
            {
                Console.WriteLine("Client non inizializzato.");
                return;
            }

            Console.WriteLine($"Scansione figli iniziata da: {parentNodeId}");
            
            
            BrowseVariables(_connection.Session, NodeId.Parse(parentNodeId), 0);

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

        public void WriteNode(string nodeId, object value)
        {
            if (_connection.Session == null)
            {
                Console.WriteLine("Client non inizializzato.");
                return;
            }

            try
            {
                Console.WriteLine($"[Write] Tentativo scrittura su: {nodeId} | Valore: {value}");
                
                WriteValueCollection valuesToWrite = new WriteValueCollection();
                valuesToWrite.Add(new WriteValue { NodeId = NodeId.Parse(nodeId), AttributeId = Attributes.Value, Value = new DataValue(new Variant(value)) });

                var writeResult = _connection.Session.WriteAsync(null, valuesToWrite, CancellationToken.None).GetAwaiter().GetResult();
                var results = writeResult.Results;

                if (StatusCode.IsGood(results[0]))
                    Console.WriteLine($"[Write] Scrittura completata con successo.");
                else
                    Console.WriteLine($"[Write] Errore scrittura. Status: {results[0]}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Write] Eccezione: {ex.Message}");
            }
        }

        private void BrowseVariables(Session session, NodeId nodeId, int level)
        {
            try
            {
                // Configurazione Browse
                ReferenceDescriptionCollection references = new ReferenceDescriptionCollection();
                byte[] continuationPoint;
                
                BrowseDescriptionCollection nodesToBrowse = new BrowseDescriptionCollection();
                nodesToBrowse.Add(new BrowseDescription
                {
                    NodeId = nodeId,
                    BrowseDirection = BrowseDirection.Forward,
                    ReferenceTypeId = ReferenceTypeIds.HierarchicalReferences,
                    IncludeSubtypes = true,
                    NodeClassMask = (uint)(NodeClass.Variable | NodeClass.Object),
                    ResultMask = (uint)BrowseResultMask.All
                });

                // Esegui Browse iniziale
                var browseResult = session.BrowseAsync(
                    null, 
                    null, 
                    0, 
                    nodesToBrowse,
                    CancellationToken.None).GetAwaiter().GetResult();
                var results = browseResult.Results;
                var diagnosticInfos = browseResult.DiagnosticInfos;

                ClientBase.ValidateResponse(results, nodesToBrowse);

                if (results.Count > 0)
                {
                    references.AddRange(results[0].References);
                    continuationPoint = results[0].ContinuationPoint;

                    // Gestione paginazione (se ci sono troppi nodi per una singola risposta)
                    while (continuationPoint != null)
                    {
                        var continuationPoints = new ByteStringCollection { continuationPoint };
                        var browseNextResult = session.BrowseNextAsync(null, false, continuationPoints, CancellationToken.None).GetAwaiter().GetResult();
                        results = browseNextResult.Results;
                        diagnosticInfos = browseNextResult.DiagnosticInfos;
                        
                        if (results.Count > 0) references.AddRange(results[0].References);
                        
                        continuationPoint = results.Count > 0 ? results[0].ContinuationPoint : new byte[0];
                    }
                }

                foreach (var childNode in references)
                {
                    // Converti ExpandedNodeId in NodeId
                    NodeId childNodeId = ExpandedNodeId.ToNodeId(childNode.NodeId, session.NamespaceUris);

                    // Filtra per categoria
                    if (childNode.NodeClass == NodeClass.Variable)
                    {
                        string indent = new string(' ', level * 2);
                        string valueStr;
                        int length = 0;
                        string variableType = "";
                        
                        try
                        {
                            // Leggi il valore
                            var nodesToRead = new ReadValueIdCollection { new ReadValueId { NodeId = childNodeId, AttributeId = Attributes.Value } };
                            var readResult = session.ReadAsync(null, 0, TimestampsToReturn.Both, nodesToRead, CancellationToken.None).GetAwaiter().GetResult();
                            var value = readResult.Results.Count > 0 ? readResult.Results[0] : new DataValue(StatusCodes.BadNoData);
                            var val = value.Value;

                            variableType = val?.GetType().Name ?? "null";

                            if (val is Array arr)
                                length = arr.Length;
                            else
                                length = val switch
                                {
                                    float => 4,
                                    double => 8,
                                    short or ushort => 2,
                                    int or uint => 4,
                                    long or ulong => 8,
                                    bool => 1,
                                    _ => 0
                                };

                            Console.WriteLine($"{indent}{childNode.DisplayName} = {val} | Type: {variableType} | Len: {length} | NodeId: {childNodeId}");
                            valueStr = val?.ToString() ?? "null";
                        }
                        catch
                        {
                            Console.WriteLine($"{indent}{childNode.DisplayName} (non leggibile) | NodeId: {childNodeId}");
                            valueStr = "(non leggibile)";
                            variableType = "Error";
                        }

                        // Aggiungi alla lista per il JSON
                        _nodeItems.Add(new OpcNodeItem
                        {
                            DisplayName = childNode.DisplayName.ToString(),
                            NodeId = childNodeId.ToString(),
                            Value = valueStr,
                            Length = length,
                            VariableType = variableType
                        });
                    }

                    // Continua la ricorsione
                    BrowseVariables(session, ExpandedNodeId.ToNodeId(childNode.NodeId, session.NamespaceUris), level + 1);
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
        public int Length { get; set; }
        public string VariableType { get; set; } = "";
    }
}