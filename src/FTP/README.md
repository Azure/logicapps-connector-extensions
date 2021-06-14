**Azure Logic Apps Single Tenant FTP Connector**

Sample FTP Connector that implements the following operations:

1. Download File
2. Upload file
3. List files
4. Delete file
5. Support for FTPS
6. Self signed cert (bypass checks for testing)
7. Active/Passive mode
8. Implict/Explicit mode

Based on the following sample from the Logic Apps team:
https://techcommunity.microsoft.com/t5/integrations-on-azure/azure-logic-apps-running-anywhere-built-in-connector/ba-p/1921272

And this repo:
https://github.com/Azure/logicapps-connector-extensions/tree/CosmosDB/src/CosmosDB

This only works with a nuget based Logic App, please see:
https://docs.microsoft.com/en-us/azure/logic-apps/create-single-tenant-workflows-visual-studio-code#enable-built-in-connector-authoring

To deploy, run deploy.ps1 (in tools), passing in the path of the nuget package. E.g.,

& "deploy.ps1" "C:\Dev\Logic Apps\Custom Connector\FTP\src\FTP\bin\Debug"

This sample is provided as-is and is not an official Microsoft supported connector.