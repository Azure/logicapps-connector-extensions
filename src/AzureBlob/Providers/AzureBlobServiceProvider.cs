// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Microsoft.Azure.Workflows.ServiceProvider.Extensions.AzureBlob
{
    using System.Collections.Generic;
    using Microsoft.Azure.Storage.Blob;
    using Microsoft.Azure.WebJobs.Description;
    using Microsoft.Azure.WebJobs.Host.Config;
    using Microsoft.Azure.Workflows.ServiceProviders.Abstractions;
    using Microsoft.WindowsAzure.ResourceStack.Common.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Service Provider class which injects all the service operation provider..
    /// </summary>
    [Extension("AzureBlobServiceProvider", configurationSection: "AzureBlobServiceProvider")]
    public class AzureBlobServiceProvider : IExtensionConfigProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AzureBlobServiceProvider"/> class.
        /// </summary>
        /// <param name="serviceOperationsProvider">Service operations provider.</param>
        /// <param name="operationsProvider">Operation provider.</param>
        public AzureBlobServiceProvider(
            ServiceOperationsProvider serviceOperationsProvider,
            AzureBlobServiceOperationProvider operationsProvider)
        {
            serviceOperationsProvider.RegisterService(serviceName: AzureBlobServiceOperationProvider.ServiceName, serviceOperationsProviderId: AzureBlobServiceOperationProvider.ServiceId, serviceOperationsProviderInstance: operationsProvider);
        }

        /// <summary>
        /// Convert the array of Azure blob Document to generic JObject array.
        /// </summary>
        /// <param name="blob">Document List.</param>
        /// <returns>JObject array.</returns>
        public static JObject[] ConvertBlobClientToJObject(Microsoft.Azure.Storage.Blob.CloudBlockBlob blob)
        {
            List<JObject> jobjects = new List<JObject>();

           jobjects.Add(item: (JObject)blob.ToJToken());
           
            return jobjects.ToArray();
        }

        /// <summary>
        /// You can add any custom implementation in Initialize method.
        /// </summary>
        /// <param name="context">Context.</param>
        public void Initialize(ExtensionConfigContext context)
        {
            context.AddConverter<Microsoft.Azure.Storage.Blob.CloudBlockBlob, JObject[]>(ConvertBlobClientToJObject);
        }
    }
}
