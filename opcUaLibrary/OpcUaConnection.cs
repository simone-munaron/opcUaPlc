using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Opc.Ua;
using Opc.Ua.Client;
using Opc.Ua.Configuration;

namespace opcUaPlc

{
    public class OpcUaConnection : IDisposable
    {
        private Session? _session;
        public Session? Session => _session;
        private readonly string _serverUrl;
        private readonly string _username;
        private readonly string _password;
        private ApplicationConfiguration? _config;

        public OpcUaConnection(string serverUrl, string username, string password)
        {
            _serverUrl = serverUrl;
            _username = username;
            _password = password;
        }

        // Restituisce true se la connessione avviene con successo
        public bool Start()
        {
            try 
            {
                // Creazione configurazione applicazione OPC UA
                _config = new ApplicationConfiguration()
                {
                    ApplicationName = "OpcUaPlcClient",
                    ApplicationUri = Utils.Format(@"urn:{0}:OpcUaPlcClient", System.Net.Dns.GetHostName()),
                    ApplicationType = ApplicationType.Client,
                    SecurityConfiguration = new SecurityConfiguration
                    {
                        ApplicationCertificate = new CertificateIdentifier { StoreType = @"Directory", StorePath = @"%CommonApplicationData%\OPC Foundation\CertificateStores\MachineDefault", SubjectName = "OpcUaPlcClient" },
                        TrustedIssuerCertificates = new CertificateTrustList { StoreType = @"Directory", StorePath = @"%CommonApplicationData%\OPC Foundation\CertificateStores\UA Certificate Authorities" },
                        TrustedPeerCertificates = new CertificateTrustList { StoreType = @"Directory", StorePath = @"%CommonApplicationData%\OPC Foundation\CertificateStores\UA Applications" },
                        RejectedCertificateStore = new CertificateTrustList { StoreType = @"Directory", StorePath = @"%CommonApplicationData%\OPC Foundation\CertificateStores\RejectedCertificates" },
                        AutoAcceptUntrustedCertificates = true, // Accetta certificati non fidati (utile per test/PLC)
                        AddAppCertToTrustedStore = true
                    },
                    TransportConfigurations = new TransportConfigurationCollection(),
                    TransportQuotas = new TransportQuotas { OperationTimeout = 15000 },
                    ClientConfiguration = new ClientConfiguration { DefaultSessionTimeout = 60000 },
                    TraceConfiguration = new TraceConfiguration()
                };
                
                _config.ValidateAsync(ApplicationType.Client).GetAwaiter().GetResult();

                if (_config.SecurityConfiguration.AutoAcceptUntrustedCertificates)
                {
                    _config.CertificateValidator.CertificateValidation += (s, e) => { e.Accept = true; };
                }

                var endpoint = SelectEndpoint(_serverUrl, useSecurity: false);
                var userIdentity = new UserIdentity(_username, _password);
                
#pragma warning disable CS0618
                _session = Session.Create(_config, new ConfiguredEndpoint(null, endpoint, EndpointConfiguration.Create(_config)), false, "", 60000, userIdentity, null, CancellationToken.None).GetAwaiter().GetResult();
#pragma warning restore CS0618

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
                if (_session != null)
                {
#pragma warning disable CS0618
                    _session.Close();
#pragma warning restore CS0618
                }
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
            bool isConnected = _session != null && _session.Connected;
            Console.WriteLine($"Stato attuale: {(isConnected ? "Connected" : "Disconnected")} ({_serverUrl})");
            return isConnected;
        }

        public void Dispose()
        {
            _session?.Dispose();
        }

        public (bool Success, object? Value, int ByteLength, StatusCode Status, string VariableType) ReadVariable(string nodeId)
        {
            if (_session == null || !_session.Connected)
            {
                Console.WriteLine("Client non connesso!");
                return (false, null, 0, StatusCodes.BadNotConnected, "");
            }

            try
            {
                // Lettura sincrona
                var nodesToRead = new ReadValueIdCollection { new ReadValueId { NodeId = NodeId.Parse(nodeId), AttributeId = Attributes.Value } };
                var readResult = _session.ReadAsync(null, 0, TimestampsToReturn.Both, nodesToRead, CancellationToken.None).GetAwaiter().GetResult();
                var opcValue = readResult.Results.Count > 0 ? readResult.Results[0] : new DataValue(StatusCodes.BadNoData);

                if (StatusCode.IsGood(opcValue.StatusCode))
                {
                    object? value = opcValue.Value;
                    int byteLength = 0; // Default 0

                    if (value is byte[] byteArray)
                    {
                        byteLength = byteArray.Length;
                        //Console.WriteLine($"ByteString trovato (len: {byteLength}), raw: {BitConverter.ToString(byteArray)}");

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
                        //// Tipo nativo (Float/Int etc.)
                        //Console.WriteLine($"Tipo nativo: {value?.GetType().Name}");
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

                    //Console.WriteLine($"Valore finale: {value} (ByteLength: {byteLength}) | Timestamp: {opcValue.SourceTimestamp}");
                    return (true, value, byteLength, opcValue.StatusCode, opcValue.Value?.GetType().Name ?? "");
                }
                else
                {
                    Console.WriteLine($"Status error: {opcValue.StatusCode}");
                    return (false, null, 0, opcValue.StatusCode, "");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                return (false, null, 0, StatusCodes.BadUnexpectedError, "");
            }
        }

        public List<(string NodeId, bool Success, object? Value, int ByteLength, StatusCode Status, string VariableType)> ReadVariables(List<string> nodeIds)
        {
            var parsedIds = nodeIds.Select(n => NodeId.Parse(n)).ToList();
            var results = ReadVariables(parsedIds);
            
            var finalResults = new List<(string, bool, object?, int, StatusCode, string)>();
            for (int i = 0; i < results.Count; i++)
            {
                var r = results[i];
                finalResults.Add((nodeIds[i], r.Success, r.Value, r.ByteLength, r.Status, r.VariableType));
            }
            return finalResults;
        }

        public List<(bool Success, object? Value, int ByteLength, StatusCode Status, string VariableType)> ReadVariables(List<NodeId> nodeIds)
        {
            var results = new List<(bool, object?, int, StatusCode, string)>();

            if (_session == null || !_session.Connected)
            {
                Console.WriteLine("Client non connesso!");
                return nodeIds.Select(n => (false, (object?)null, 0, new StatusCode(StatusCodes.BadNotConnected), "")).ToList();
            }

            try
            {
                // Preparazione Bulk Read
                ReadValueIdCollection nodesToRead = new ReadValueIdCollection();
                nodesToRead.AddRange(nodeIds.Select(n => new ReadValueId { NodeId = n, AttributeId = Attributes.Value }));

                var readResult = _session.ReadAsync(null, 0, TimestampsToReturn.Both, nodesToRead, CancellationToken.None).GetAwaiter().GetResult();
                var opcValues = readResult.Results;

                for (int i = 0; i < opcValues.Count; i++)
                {
                    var opcValue = opcValues[i];

                    if (StatusCode.IsGood(opcValue.StatusCode))
                    {
                        object? value = opcValue.Value;
                        int byteLength = 0;

                        if (value is byte[] byteArray)
                        {
                            byteLength = byteArray.Length;
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
                            byteLength = value is float ? 4 : value is double ? 8 : value is short || value is ushort ? 2 : value is int ? 4 : 0;
                        }
                        results.Add((true, value, byteLength, opcValue.StatusCode, opcValue.Value?.GetType().Name ?? ""));
                    }
                    else
                    {
                        results.Add((false, null, 0, opcValue.StatusCode, ""));
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception Bulk Read: {ex.Message}");
                return nodeIds.Select(n => (false, (object?)null, 0, new StatusCode(StatusCodes.BadUnexpectedError), "")).ToList();
            }
            return results;
        }

        private EndpointDescription SelectEndpoint(string discoveryUrl, bool useSecurity)
        {
            // Usa DiscoveryClient standard per trovare gli endpoint
            using (var client = DiscoveryClient.Create(new Uri(discoveryUrl), EndpointConfiguration.Create(_config)))
            {
                var endpoints = client.GetEndpointsAsync(null, CancellationToken.None).GetAwaiter().GetResult();
                
                if (useSecurity)
                    return endpoints.OrderByDescending(e => e.SecurityLevel).First();
                else
                    return endpoints.FirstOrDefault(e => e.SecurityMode == MessageSecurityMode.None) ?? endpoints.First();
            }
        }
    }
}