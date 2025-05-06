// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Microsoft.Azure.Workflows.ServiceProvider.Extensions.SftpCustom
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
    [Extension("SftpCustomServiceProvider", configurationSection: "SftpCustomServiceProvider")]
    public class SftpCustomServiceProvider : IExtensionConfigProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SftpCustomServiceProvider"/> class.
        /// </summary>
        /// <param name="serviceOperationsProvider">Service operations provider.</param>
        /// <param name="operationsProvider">Operation provider.</param>
        public SftpCustomServiceProvider(
            ServiceOperationsProvider serviceOperationsProvider,
            SftpCustomServiceOperationProvider operationsProvider)
        {
            serviceOperationsProvider.RegisterService(serviceName: SftpCustomServiceOperationProvider.ServiceName, serviceOperationsProviderId: SftpCustomServiceOperationProvider.ServiceId, serviceOperationsProviderInstance: operationsProvider);
        }

        /// <summary>
        /// You can add any custom implementation in Initialize method.
        /// </summary>
        /// <param name="context">Context.</param>
        public void Initialize(ExtensionConfigContext context) { }
    }
}