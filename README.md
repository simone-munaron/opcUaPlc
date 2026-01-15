# Add solution on dotnet
    dotnet new sln -n opcUaPlc
    dotnet sln add opcUaPlc.csproj



# OpcUaConfigReader.cs
### Description
    Use for read configuration on opcUaConfig.json:
    Input: string configFilePath
    Output: string serverUrl, string username, string password
### Example file config
`opcUaConfig.json`

    {
      "serverUrl": "opc.tcp://10.69.131.1",
      "username": "user",
      "password": "password"
    }
### Example call
    var (serverUrl, username, password) = OpcConfigReader.ReadConfig(@"C:\Prj\opcUaPlc\opcUaConfig\opcUaConfig.json");





# OpcUaConnection.cs
### Description
    Use for start, stop and check the connection status
    Use always global init
### Example global init
    _OpcUaConnection = new OpcUaConnection(serverUrl, username, password);
### Example start connection
    status = _OpcUaConnection.Start();
### Example stop connection
    status = _OpcUaConnection.Stop();
### Example connection status
    status = _OpcUaConnection.Status();
