using Microsoft.Azure.Documents;
using Microsoft.Azure.WebJobs.Description;
using Microsoft.Azure.WebJobs.Host.Config;
using Microsoft.Azure.Workflows.ServiceProviders.Abstractions;
using Microsoft.WindowsAzure.ResourceStack.Common.Extensions;
using Microsoft.WindowsAzure.ResourceStack.Common.Json;
using Microsoft.WindowsAzure.ResourceStack.Common.Storage.Cosmos;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace Microsoft.Azure.Workflows.ServiceProvider.Extensions.CosmosDB
{
    [Extension("CosmosDbServiceProvider", configurationSection: "CosmosDbServiceProvider")]
    public class CosmosDbServiceProvider : IExtensionConfigProvider
    {
        public CosmosDbServiceProvider(ServiceOperationsProvider serviceOperationsProvider,
            CosmosDbServiceOperationProvider operationsProvider)
        {
            serviceOperationsProvider.RegisterService(serviceName: CosmosDbServiceOperationProvider.ServiceName, serviceOperationsProviderId: CosmosDbServiceOperationProvider.ServiceId, serviceOperationsProviderInstance: operationsProvider);
        }

        public void Initialize(ExtensionConfigContext context)
        {
            context.AddConverter<IReadOnlyList<Document>, JObject[]>(ConvertDocumentToJObject);
        }

        public static JObject[] ConvertDocumentToJObject(IReadOnlyList<Document> data)
        {
            List<JObject> jobjects = new List<JObject>();
            foreach(var doc in data)
            {
                jobjects.Add((JObject)doc.ToJToken());
            }
            return jobjects.ToArray();
        }
    }
}
