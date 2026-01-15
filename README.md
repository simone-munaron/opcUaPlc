## Add solution on dotnet
dotnet new sln -n opcUaPlc
dotnet sln add opcUaPlc.csproj

## OpcUaConfigReader.cs
### Use for read configuration on opcUaConfig.json
var (serverUrl, username, password) = OpcConfigReader.ReadConfig(@"C:\Prj\opcUaPlc\opcUaConfig\opcUaConfig.json");


