// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Microsoft.Azure.Workflows.ServiceProvider.Extensions.CosmosDB
{
    using System.Collections.Generic;
    using Microsoft.Azure.Documents;
    using Microsoft.Azure.WebJobs.Description;
    using Microsoft.Azure.WebJobs.Host.Config;
    using Microsoft.Azure.Workflows.ServiceProviders.Abstractions;
    using Microsoft.WindowsAzure.ResourceStack.Common.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Service Provider class which injects all the service operation provider..
    /// </summary>
    [Extension("CosmosDBServiceProvider", configurationSection: "CosmosDBServiceProvider")]
    public class CosmosDBServiceProvider : IExtensionConfigProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CosmosDBServiceProvider"/> class.
        /// </summary>
        /// <param name="serviceOperationsProvider">Service operations provider.</param>
        /// <param name="operationsProvider">Operation provider.</param>
        public CosmosDBServiceProvider(
            ServiceOperationsProvider serviceOperationsProvider,
            CosmosDBServiceOperationProvider operationsProvider)
        {
            serviceOperationsProvider.RegisterService(serviceName: CosmosDBServiceOperationProvider.ServiceName, serviceOperationsProviderId: CosmosDBServiceOperationProvider.ServiceId, serviceOperationsProviderInstance: operationsProvider);
        }

        /// <summary>
        /// Convert the array of Cosmos Document to generic JObject array.
        /// </summary>
        /// <param name="data">Document List.</param>
        /// <returns>JObject array.</returns>
        public static JObject[] ConvertDocumentToJObject(IReadOnlyList<Document> data)
        {
            List<JObject> jobjects = new List<JObject>();

            foreach (var doc in data)
            {
                jobjects.Add(item: (JObject)doc.ToJToken());
            }

            return jobjects.ToArray();
        }

        /// <summary>
        /// You can add any custom implementation in Initialize method.
        /// </summary>
        /// <param name="context">Context.</param>
        public void Initialize(ExtensionConfigContext context)
        {
            context.AddConverter<IReadOnlyList<Document>, JObject[]>(ConvertDocumentToJObject);
        }
    }
}
