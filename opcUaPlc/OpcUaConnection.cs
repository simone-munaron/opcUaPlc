using System;
using Opc.UaFx;
using Opc.UaFx.Client;

namespace opcUaPlc
{
    public class OpcUaConnection : IDisposable // Implementiamo IDisposable per pulizia
    {
        private readonly OpcClient _client;
        private readonly string _serverUrl;

        public OpcUaConnection(string serverUrl, string username, string password)
        {
            _serverUrl = serverUrl;
            _client = new OpcClient(serverUrl);
            _client.Security.UserIdentity = new OpcClientIdentity(username, password);
        }

        public void Start()
        {
            try {
                _client.Connect();
                Console.WriteLine($"\nConnesso al server OPC UA: {_serverUrl}!");
            }
            catch (Exception ex) {
                Console.WriteLine($"Errore connessione: {ex.Message}");
            }
        }

        public void Stop()
        {
            _client.Disconnect();
            Console.WriteLine($"Disconnesso da: {_serverUrl}");
        }

        public void Status()
        {
            // Verifichiamo lo stato dell'istanza persistente
            if (_client.State == OpcClientState.Connected)
                Console.WriteLine($"Stato: {_client.State} ({_serverUrl})");
            else
                Console.WriteLine($"Stato: {_client.State} ({_serverUrl})");
        }

        public void Dispose()
        {
            _client?.Dispose();
        }
    }
}