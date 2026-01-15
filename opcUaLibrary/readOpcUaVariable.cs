using System;
using System.IO;
using System.Reflection;
using System.Text.Json;
using Opc.UaFx;
using Opc.UaFx.Client;

namespace opcUaPlc  // ← Namespace del progetto
{
    public class readOpcUaVariable
    {
        public static void ReadVariable(string serverUrl, string username, string password)
        {
            Console.Write("Inserisci il NodeId da leggere: ");
            string nodeId = Console.ReadLine() ?? string.Empty;

            try
            {
                using (var client = new OpcClient(serverUrl))
                {
                    client.Security.UserIdentity = new OpcClientIdentity(username, password);
                    client.Connect();

                    Console.WriteLine("\nConnesso al server OPC UA!");
                    Console.WriteLine($"Lettura di: {nodeId}\n");

                    OpcValue value = client.ReadNode(nodeId);

                    if (value.Status.IsGood)
                    {
                        if (value.Value is byte[] byteArray)
                        {
                            Console.WriteLine($"Tipo: byte array (lunghezza: {byteArray.Length})");
                            Console.WriteLine($"Byte raw: {BitConverter.ToString(byteArray)}\n");

                            if (byteArray.Length == 4)
                            {
                                float floatValue = BitConverter.ToSingle(byteArray, 0);
                                Console.WriteLine($"Valore (Float): {floatValue}");
                            }
                            else if (byteArray.Length == 8)
                            {
                                double doubleValue = BitConverter.ToDouble(byteArray, 0);
                                Console.WriteLine($"Valore (Double): {doubleValue}");
                            }
                            else if (byteArray.Length == 2)
                            {
                                short int16Value = BitConverter.ToInt16(byteArray, 0);
                                ushort uint16Value = BitConverter.ToUInt16(byteArray, 0);
                                Console.WriteLine($"Valore (Int16): {int16Value}");
                                Console.WriteLine($"Valore (UInt16): {uint16Value}");
                            }
                            else
                            {
                                Console.WriteLine("Lunghezza byte array non gestita");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Valore: {value.Value}");
                            Console.WriteLine($"Tipo: {value.Value?.GetType().Name}");
                        }

                        Console.WriteLine($"Timestamp: {value.SourceTimestamp}");
                    }
                    else
                    {
                        Console.WriteLine($"Errore nella lettura: {value.Status}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore: {ex.Message}");
            }

            Console.WriteLine("\nPremi un tasto per uscire...");
            Console.ReadKey();
        }
    }
}
