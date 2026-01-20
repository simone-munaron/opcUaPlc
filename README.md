# Add solution on dotnet
    dotnet new sln -n opcUaPlc
    dotnet sln add opcUaPlc.csproj





# OpcUaConfigReader.cs
### Description
    Use for read configuration on opcUaConfig.json:
    Input: string configFilePath
    Output: bool success, string serverUrl, string username, string password, string errorMessage
### Example file config
`opcUaConfig.json`

    {
      "serverUrl": "opc.tcp://10.69.131.1",
      "username": "user",
      "password": "password"
    }
### Example call
    var (success, serverUrl, username, password, errorMessage) = OpcConfigReader.ReadConfig(@"C:\Prj\opcUaPlc\opcUaConfig\opcUaConfig.json");





# OpcUaConnection.cs
### Description
    Use for start, stop, check the connection status and read variables
    Use always global init
### Example global init
    _OpcUaConnection = new OpcUaConnection(serverUrl, username, password);
### Example start connection
    status = _OpcUaConnection.Start();
### Example stop connection
    status = _OpcUaConnection.Stop();
### Example connection status
    status = _OpcUaConnection.Status();
### Example read variable
    var (success, value, length, statusCode, variableType) = _OpcUaConnection.ReadVariable("ns=3;s=\"IFM\".\"IOLink_SV4200\"[2].\"Sts\".\"Flow\"");
