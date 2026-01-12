using System;
using Opc.UaFx;
using Opc.UaFx.Client;

class Program
{
    static void Main(string[] args)
    {
        string serverUrl = "opc.tcp://10.69.131.1";
        string username = "OPC_UA";
        string password = "L3asOpc_uA";

        using (var client = new OpcClient(serverUrl))
        {
            client.Security.UserIdentity = new OpcClientIdentity(username, password);
            client.Connect();

            Console.WriteLine("Connesso!\n");
            Console.WriteLine("=== TUTTE LE VARIABILI ===\n");
            
            BrowseVariables(client, OpcObjectTypes.ObjectsFolder, 0);

            client.Disconnect();
        }
    }

    static void BrowseVariables(OpcClient client, OpcNodeId nodeId, int level)
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
                    
                    try
                    {
                        // Leggi il valore
                        var value = client.ReadNode(childNode.NodeId);
                        Console.WriteLine($"{indent}{childNode.DisplayName} = {value} | NodeId: {childNode.NodeId}");
                    }
                    catch
                    {
                        Console.WriteLine($"{indent}{childNode.DisplayName} (non leggibile) | NodeId: {childNode.NodeId}");
                    }
                }

                // Continua la ricorsione
                BrowseVariables(client, childNode.NodeId, level + 1);
            }
        }
        catch (Exception ex)
        {
            // Ignora errori di accesso
        }
    }
}
