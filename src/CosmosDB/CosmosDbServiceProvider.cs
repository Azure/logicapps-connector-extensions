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
    /// <summary>
    /// Service Provider class which injects all the service operation provider..
    /// </summary>
    [Extension("CosmosDbServiceProvider", configurationSection: "CosmosDbServiceProvider")]
    public class CosmosDbServiceProvider : IExtensionConfigProvider
    {
        /// <summary>
        /// Register the service provider.
        /// </summary>
        /// <param name="serviceOperationsProvider"></param>
        /// <param name="operationsProvider"></param>
        public CosmosDbServiceProvider(ServiceOperationsProvider serviceOperationsProvider,
            CosmosDbServiceOperationProvider operationsProvider)
        {
            serviceOperationsProvider.RegisterService(serviceName: CosmosDbServiceOperationProvider.ServiceName, serviceOperationsProviderId: CosmosDbServiceOperationProvider.ServiceId, serviceOperationsProviderInstance: operationsProvider);
        }

        /// <summary>
        /// You can add any custom implementation in Initialize method.
        /// </summary>
        /// <param name="context"></param>
        public void Initialize(ExtensionConfigContext context)
        {
            context.AddConverter<IReadOnlyList<Document>, JObject[]>(ConvertDocumentToJObject);
        }

        /// <summary>
        /// Cosmos DB trigger runtime provides the array of Document. In this method we convert it to generic JObject array.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
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
