// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Microsoft.Azure.Workflows.ServiceProvider.Extensions.ActiveMQ
{
    using Microsoft.Azure.Workflows.ServiceProviders.Abstractions;

    /// <summary>
    /// ActiveMQConnectionParameters API connection parameters
    /// </summary>
    public class ActiveMQConnectionParameters : ConnectionParameters
    {/// <summary>
     /// BrokerUri ActiveMQ BrokerUri.
     /// </summary>
        public ConnectionStringParameters BrokerUri { get; set; }

        /// <summary>
        /// ClientId for the ActiveMQ client.
        /// </summary>
        public ConnectionStringParameters ClientId { get; set; }

        /// <summary>
        /// ActiveMQ Queue name.
        /// </summary>
        public ConnectionStringParameters QueueName { get; set; }

        /// <summary>
        /// The user name that will be used to connect to the BrokerUri.
        /// </summary>
        public ConnectionStringParameters UserName { get; set; }

        /// <summary>
        /// Password.
        /// </summary>
        public ConnectionStringParameters Password { get; set; }
    }
}