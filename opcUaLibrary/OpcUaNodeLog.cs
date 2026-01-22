using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Data.Sqlite;
using Opc.Ua;

namespace opcUaPlc
{
    public class OpcUaNodeLog
    {
        private readonly OpcUaConnection _connection;
        private readonly string _dbPath;
        
        // Cache per evitare il parsing continuo delle stringhe NodeId
        private List<string>? _lastNodeIdsSource;
        private List<NodeId>? _cachedParsedNodeIds;

        public OpcUaNodeLog(OpcUaConnection connection, string dbPath)
        {
            _connection = connection;
            _dbPath = dbPath;
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            try
            {
                // Assicura che la cartella esista
                string? dir = Path.GetDirectoryName(_dbPath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                using (var connection = new SqliteConnection($"Data Source={_dbPath}"))
                {
                    connection.Open();
                    var command = connection.CreateCommand();
                    command.CommandText = @"
                        PRAGMA journal_mode = WAL;
                        CREATE TABLE IF NOT EXISTS Logs (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            Timestamp TEXT NOT NULL,
                            NodeId TEXT NOT NULL,
                            Value TEXT,
                            VariableType TEXT
                        );";
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Log] Errore inizializzazione DB: {ex.Message}");
            }
        }

        public void LogNodes(List<string> nodeIds)
        {
            if (nodeIds == null || nodeIds.Count == 0) return;

            const int BatchSize = 1000;

            // Aggiorna la cache dei NodeId se la lista è cambiata (ottimizzazione CPU)
            if (_lastNodeIdsSource != nodeIds)
            {
                try 
                {
                    _cachedParsedNodeIds = nodeIds.Select(n => NodeId.Parse(n)).ToList();
                    _lastNodeIdsSource = nodeIds;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Log] Errore parsing NodeIds: {ex.Message}");
                    return;
                }
            }

            try
            {
                using (var db = new SqliteConnection($"Data Source={_dbPath}"))
                {
                    db.Open();
                    
                    // Ottimizzazione scrittura DB: Synchronous NORMAL è molto più veloce e sicuro in WAL mode
                    using (var cmdPragma = db.CreateCommand())
                    {
                        cmdPragma.CommandText = "PRAGMA synchronous = NORMAL;";
                        cmdPragma.ExecuteNonQuery();
                    }

                    // Processa i nodi a blocchi per evitare timeout e ottimizzare la memoria
                    for (int i = 0; i < _cachedParsedNodeIds!.Count; i += BatchSize)
                    {
                        var batchNodes = _cachedParsedNodeIds.Skip(i).Take(BatchSize).ToList();
                        var batchStringIds = nodeIds.Skip(i).Take(BatchSize).ToList();
                        
                        // Lettura OPC UA del blocco corrente
                        var results = _connection.ReadVariables(batchNodes);

                        using (var transaction = db.BeginTransaction())
                        {
                            // Prepara il comando una sola volta per tutto il blocco (Performance Boost)
                            using (var cmd = db.CreateCommand())
                            {
                                cmd.Transaction = transaction;
                                cmd.CommandText = "INSERT INTO Logs (Timestamp, NodeId, Value, VariableType) VALUES ($ts, $node, $val, $type)";
                                
                                var pTs = cmd.Parameters.Add("$ts", SqliteType.Text);
                                var pNode = cmd.Parameters.Add("$node", SqliteType.Text);
                                var pVal = cmd.Parameters.Add("$val", SqliteType.Text);
                                var pType = cmd.Parameters.Add("$type", SqliteType.Text);

                                for (int k = 0; k < results.Count; k++)
                                {
                                    var result = results[k];
                                    if (result.Success)
                                    {
                                        pTs.Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                                        pNode.Value = batchStringIds[k];
                                        pVal.Value = result.Value?.ToString() ?? "null";
                                        pType.Value = result.VariableType;
                                        cmd.ExecuteNonQuery();
                                    }
                                }
                            }
                            transaction.Commit();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Log] Errore durante il ciclo di logging: {ex.Message}");
            }
        }
    }
}