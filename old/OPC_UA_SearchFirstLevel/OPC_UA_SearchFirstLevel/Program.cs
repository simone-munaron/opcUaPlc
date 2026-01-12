using System;
using System.Collections.Generic;
using System.Linq;
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

            Console.WriteLine("Connesso al server OPC UA!\n");
            Console.WriteLine("Premi INVIO per iniziare la navigazione...");
            Console.ReadLine();

            InteractiveBrowse(client, OpcObjectTypes.ObjectsFolder, "Root > Objects", 0);

            client.Disconnect();
            Console.WriteLine("\nDisconnesso dal server.");
        }
    }

    static void InteractiveBrowse(OpcClient client, OpcNodeId currentNodeId, string path, int depth)
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine($"=== NAVIGAZIONE INTERATTIVA ===");
            Console.WriteLine($"Percorso: {path}");
            Console.WriteLine($"Profondità: Livello {depth}\n");

            try
            {
                var nodeInfo = client.BrowseNode(currentNodeId);
                var children = nodeInfo.Children().ToList();

                if (children.Count == 0)
                {
                    Console.WriteLine("Nessun nodo figlio disponibile.");
                }
                else
                {
                    int objCount = children.Count(c => c.Category == OpcNodeCategory.Object);
                    int varCount = children.Count(c => c.Category == OpcNodeCategory.Variable);
                    int methodCount = children.Count(c => c.Category == OpcNodeCategory.Method);
                    int otherCount = children.Count - objCount - varCount - methodCount;

                    Console.WriteLine($"Trovati {children.Count} nodi:");
                    Console.WriteLine($"  • Oggetti: {objCount}");
                    Console.WriteLine($"  • Variabili: {varCount}");
                    Console.WriteLine($"  • Metodi: {methodCount}");
                    if (otherCount > 0)
                        Console.WriteLine($"  • Altri: {otherCount}");
                    Console.WriteLine();

                    for (int i = 0; i < children.Count; i++)
                    {
                        var child = children[i];
                        string icon = GetNodeIcon(child.Category);
                        Console.WriteLine($"{i + 1,4}. {icon} [{child.Category}] {child.DisplayName}");
                        
                        if (child.Category == OpcNodeCategory.Variable)
                        {
                            try
                            {
                                var value = client.ReadNode(child.NodeId);
                                string valueStr = value?.ToString() ?? "(null)";
                                if (valueStr.Length > 60)
                                    valueStr = valueStr.Substring(0, 57) + "...";
                                Console.WriteLine($"       Valore: {valueStr}");
                            }
                            catch
                            {
                                Console.WriteLine($"       Valore: (non leggibile)");
                            }
                        }
                    }
                }

                Console.WriteLine("\n" + new string('─', 60));
                Console.WriteLine("0. ← Torna indietro");
                Console.WriteLine("A. Mostra tutti i valori delle variabili");
                Console.WriteLine("Q. Esci dal programma");
                Console.Write("\nSeleziona un nodo (numero) o comando: ");

                string input = Console.ReadLine()?.Trim();

                if (input?.ToUpper() == "Q")
                    return;

                if (input == "0")
                    return;

                if (input?.ToUpper() == "A")
                {
                    ShowAllVariables(client, children);
                    continue;
                }

                if (int.TryParse(input, out int selection) && selection > 0 && selection <= children.Count)
                {
                    var selectedNode = children[selection - 1];
                    string newPath = $"{path} > {selectedNode.DisplayName}";

                    if (selectedNode.Category == OpcNodeCategory.Variable)
                    {
                        ShowVariableDetails(client, selectedNode, newPath, depth + 1);
                    }
                    else if (selectedNode.Category == OpcNodeCategory.Method)
                    {
                        ShowMethodDetails(selectedNode, newPath);
                    }
                    else
                    {
                        InteractiveBrowse(client, selectedNode.NodeId, newPath, depth + 1);
                    }
                }
                else
                {
                    Console.WriteLine("\n✗ Selezione non valida. Premi INVIO per continuare...");
                    Console.ReadLine();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n✗ Errore: {ex.Message}");
                Console.WriteLine("Premi INVIO per continuare...");
                Console.ReadLine();
                return;
            }
        }
    }

    static void ShowVariableDetails(OpcClient client, OpcNodeInfo variableNode, string path, int depth)
    {
        Console.Clear();
        Console.WriteLine("=== DETTAGLI VARIABILE ===\n");
        Console.WriteLine($"Percorso: {path}");
        Console.WriteLine($"Profondità: Livello {depth}\n");
        Console.WriteLine(new string('─', 60));
        Console.WriteLine($"Nome Display:  {variableNode.DisplayName}");
        Console.WriteLine($"NodeId:        {variableNode.NodeId}");
        Console.WriteLine($"Categoria:     {variableNode.Category}");

        // Mostra proprietà specifiche di OpcVariableNodeInfo
        if (variableNode is OpcVariableNodeInfo varInfo)
        {
            Console.WriteLine($"DataTypeId:    {varInfo.DataTypeId}");
            Console.WriteLine($"AccessLevel:   {varInfo.AccessLevel}");
            Console.WriteLine($"ArrayLength:   {varInfo.ArrayLength}");
            
            if (varInfo.ArrayDimensions != null && varInfo.ArrayDimensions.Length > 0)
            {
                Console.WriteLine($"ArrayDimensions: [{string.Join(", ", varInfo.ArrayDimensions)}]");
            }
        }

        Console.WriteLine(new string('─', 60));

        try
        {
            var value = client.ReadNode(variableNode.NodeId);
            Console.WriteLine($"\n✓ Valore corrente: {value}");
            Console.WriteLine($"  Tipo C#:         {value?.GetType().Name ?? "N/A"}");
            
            if (value != null)
            {
                Console.WriteLine($"  ToString():      {value}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n✗ Errore nella lettura: {ex.Message}");
        }

        Console.WriteLine("\n" + new string('─', 60));
        Console.WriteLine("R. Rileggi valore");
        Console.WriteLine("INVIO. Torna indietro");
        Console.Write("\nComando: ");
        
        string cmd = Console.ReadLine()?.ToUpper();
        if (cmd == "R")
        {
            ShowVariableDetails(client, variableNode, path, depth);
        }
    }

    static void ShowMethodDetails(OpcNodeInfo methodNode, string path)
    {
        Console.Clear();
        Console.WriteLine("=== DETTAGLI METODO ===\n");
        Console.WriteLine($"Percorso: {path}\n");
        Console.WriteLine(new string('─', 60));
        Console.WriteLine($"Nome:      {methodNode.DisplayName}");
        Console.WriteLine($"NodeId:    {methodNode.NodeId}");
        Console.WriteLine($"Categoria: {methodNode.Category}");
        Console.WriteLine(new string('─', 60));
        Console.WriteLine("\nI metodi richiedono parametri specifici per essere chiamati.");
        Console.WriteLine("\nPremi INVIO per tornare...");
        Console.ReadLine();
    }

    static void ShowAllVariables(OpcClient client, List<OpcNodeInfo> nodes)
    {
        Console.Clear();
        Console.WriteLine("=== TUTTE LE VARIABILI ===\n");

        var variables = nodes.Where(n => n.Category == OpcNodeCategory.Variable).ToList();

        if (variables.Count == 0)
        {
            Console.WriteLine("Nessuna variabile trovata in questo livello.");
        }
        else
        {
            Console.WriteLine($"Trovate {variables.Count} variabili:\n");
            
            foreach (var variable in variables)
            {
                Console.Write($"• {variable.DisplayName,-40} = ");
                try
                {
                    var value = client.ReadNode(variable.NodeId);
                    Console.WriteLine(value);
                }
                catch
                {
                    Console.WriteLine("(non leggibile)");
                }
            }
        }

        Console.WriteLine("\nPremi INVIO per tornare...");
        Console.ReadLine();
    }

    static string GetNodeIcon(OpcNodeCategory category)
    {
        return category switch
        {
            OpcNodeCategory.Object => "📁",
            OpcNodeCategory.Variable => "📊",
            OpcNodeCategory.Method => "⚙️",
            OpcNodeCategory.ObjectType => "📂",
            OpcNodeCategory.VariableType => "📈",
            _ => "❓"
        };
    }
}
