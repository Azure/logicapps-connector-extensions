
$PackageName="ServiceProviders.ActiveMQ"
$extensionPath="..\ServiceProviders.ActiveMQ\bin\Debug\"
#$extensionPath="..\ServiceProviders.ActiveMQ\bin\Release\netcoreapp3.1\publish"

#delete the packeg from the cache folder 
Remove-Item "C:\Users\mobarqaw\.nuget\packages\$PackageName"
dotnet add package "Microsoft.Azure.Workflows.WebJobs.Extension" -v 1.0.1.1-preview -s https://api.nuget.org/v3/index.json
dotnet add package "$PackageName" --version 1.0.0  --source $extensionPath
