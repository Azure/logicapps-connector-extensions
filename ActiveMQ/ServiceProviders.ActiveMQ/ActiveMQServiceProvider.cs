//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace ServiceProviders.ActiveMQ
{
    using Microsoft.Azure.WebJobs.Description;
    using Microsoft.Azure.WebJobs.Host.Config;
    using Microsoft.Azure.Workflows.ServiceProviders.Abstractions;
    using ServiceProviders.ActiveMQ.Extension;

    /// <summary>
    /// Custom test service provider.
    /// </summary>
    [Extension("ActiveMQServiceProvider", configurationSection: "ActiveMQServiceProvider")]
    public class ActiveMQServiceProvider : IExtensionConfigProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ActiveMQServiceProvider"/> class.
        /// </summary>
        /// <param name="serviceOperationsProvider">The service provider.</param>
        /// <param name="operationsProvider">The operations provider.</param>
        public ActiveMQServiceProvider(
            ServiceOperationsProvider serviceOperationsProvider,
            ActiveMQTriggerServiceOperationProvider operationsProvider)
        {
            serviceOperationsProvider.RegisterService(serviceName: ActiveMQTriggerServiceOperationProvider.ServiceName, serviceOperationsProviderId: ActiveMQTriggerServiceOperationProvider.ServiceId, serviceOperationsProviderInstance: operationsProvider);
        }

        /// <summary>
        /// Initialize.
        /// </summary>
        /// <param name="context">The context.</param>
        public void Initialize(ExtensionConfigContext context)
        {
        }
    }
}