using System;
using Opc.UaFx;
using Opc.UaFx.Client;

namespace opcUaPlc

{
    public class OpcUaConnection : IDisposable
    {
        private readonly OpcClient _client;
        private readonly string _serverUrl;

        public OpcUaConnection(string serverUrl, string username, string password)
        {
            _serverUrl = serverUrl;
            _client = new OpcClient(serverUrl);
            _client.Security.UserIdentity = new OpcClientIdentity(username, password);
        }

        // Restituisce true se la connessione avviene con successo
        public bool Start()
        {
            try 
            {
                _client.Connect();
                Console.WriteLine($"Connesso a: {_serverUrl}");
                return true;
            }
            catch (Exception ex) 
            {
                Console.WriteLine($"Errore connessione: {ex.Message}");
                return false;
            }
        }

        // Restituisce true se la disconnessione avviene senza errori
        public bool Stop()
        {
            try
            {
                _client.Disconnect();
                Console.WriteLine($"Disconnesso da: {_serverUrl}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore disconnessione: {ex.Message}");
                return false;
            }
        }

        // Restituisce true se lo stato attuale è "Connected"
        public bool Status()
        {
            bool isConnected = _client.State == OpcClientState.Connected;
            Console.WriteLine($"Stato attuale: {_client.State} ({_serverUrl})");
            return isConnected;
        }

        public void Dispose()
        {
            _client?.Dispose();
        }

        public (bool Success, object? Value, int ByteLength, OpcStatusCode Status) ReadVariable(string nodeId)
        {
            if (_client == null || _client.State != OpcClientState.Connected)
            {
                Console.WriteLine("Client non connesso!");
                return (false, null, 0, OpcStatusCode.BadNotConnected);
            }
        
            try
            {
                Console.WriteLine($"Lettura nodo: {nodeId}");
        
                OpcValue opcValue = _client.ReadNode(nodeId);
        
                if (opcValue.Status.IsGood)
                {
                    object? value = opcValue.Value;
                    int byteLength = 0; // Default 0
        
                    if (value is byte[] byteArray)
                    {
                        byteLength = byteArray.Length;
                        Console.WriteLine($"ByteString trovato (len: {byteLength}), raw: {BitConverter.ToString(byteArray)}");
        
                        // Parsing Siemens REAL/DINT (ByteString comune) [web:77][web:73]
                        value = byteArray.Length switch
                        {
                            4 => BitConverter.ToSingle(byteArray, 0),
                            8 => BitConverter.ToDouble(byteArray, 0),
                            2 => BitConverter.ToInt16(byteArray, 0),
                            _ => byteArray
                        };
                    }
                    else
                    {
                        // Tipo nativo (Float/Int etc.)
                        Console.WriteLine($"Tipo nativo: {value?.GetType().Name}");
                        // Stima length per tipi primitivi
                        byteLength = value switch
                        {
                            float => 4,
                            double => 8,
                            short or ushort => 2,
                            int => 4,
                            _ => 0
                        };
                    }
        
                    Console.WriteLine($"Valore finale: {value} (ByteLength: {byteLength}) | Timestamp: {opcValue.SourceTimestamp}");
                    return (true, value, byteLength, opcValue.Status.Code);
                }
                else
                {
                    Console.WriteLine($"Status error: {opcValue.Status}");
                    return (false, null, 0, opcValue.Status.Code);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                return (false, null, 0, OpcStatusCode.BadUnexpectedError);
            }
        }
    }
}