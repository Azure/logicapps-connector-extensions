// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Microsoft.Azure.Workflows.ServiceProvider.Extensions.ActiveMQ
{
    using Microsoft.Azure.Workflows.ServiceProviders.Abstractions;
    /// <summary>
    /// ActiveMQConnectionParameters API connection parameters 
    /// </summary>
    public class ActiveMQConnectionParameters : ConnectionParameters
    {
        public ConnectionStringParameters BrokerUri { get; set; }

        public ConnectionStringParameters ClientId { get; set; }

        public ConnectionStringParameters QueueName { get; set; }

        public ConnectionStringParameters UserName { get; set; }

        public ConnectionStringParameters Password { get; set; }
    }
}