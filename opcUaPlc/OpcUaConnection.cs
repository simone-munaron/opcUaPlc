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
    }
}