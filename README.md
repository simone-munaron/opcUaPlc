## Add solution on dotnet
dotnet new sln -n opcUaPlc
dotnet sln add opcUaPlc.csproj

## OpcUaConfigReader.cs
#### Description
Use for read configuration on opcUaConfig.json:
Input: string configFilePath
Output: string serverUrl, string username, string password

#### Example
var (serverUrl, username, password) = OpcConfigReader.ReadConfig(@"C:\Prj\opcUaPlc\opcUaConfig\opcUaConfig.json");
