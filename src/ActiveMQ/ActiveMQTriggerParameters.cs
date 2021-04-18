// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Microsoft.Azure.Workflows.ServiceProvider.Extensions.ActiveMQ
{
    using Microsoft.Azure.Workflows.ServiceProviders.Abstractions;
    using Microsoft.WindowsAzure.ResourceStack.Common.Collections;
    using Microsoft.WindowsAzure.ResourceStack.Common.Extensions;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Combined the needed proprieties form the API and the action  in one class.
    /// </summary>
    internal class ActiveMQTriggerParameters
    {
        private InsensitiveDictionary<JToken> connectionParameters;
        private ServiceOperationRequest serviceOperationRequest;

        /// <summary>
        /// BrokerUri ActiveMQ BrokerUri.
        /// </summary>
        public string BrokerUri { get; }

        /// <summary>
        /// ClientId for the ActiveMQ client.
        /// </summary>
        public string ClientId { get; }

        /// <summary>
        /// The user name that will be used to connect to the BrokerUri.
        /// </summary>
        public string UserName { get; }

        /// <summary>
        /// Password.
        /// </summary>
        public string Password { get; }

        /// <summary>
        /// Maximum Number of messages to be received  in one trigger.
        /// </summary>
        public int MaximumNumber { get; }

        /// <summary>
        /// ActiveMQ Queue name.
        /// </summary>
        public string QueueName { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ActiveMQTriggerParameters"/> class.
        /// </summary>
        /// <param name="connectionParameters"></param>
        /// <param name="serviceOperationRequest"></param>
        public ActiveMQTriggerParameters(InsensitiveDictionary<JToken> connectionParameters, ServiceOperationRequest serviceOperationRequest)
        {
            this.connectionParameters = connectionParameters;
            this.serviceOperationRequest = serviceOperationRequest;

            BrokerUri = ServiceOperationsProviderUtilities.GetParameterValue("BrokerUri", connectionParameters).ToValue<string>();
            ClientId = ServiceOperationsProviderUtilities.GetParameterValue("ClientId", connectionParameters).ToValue<string>();
            UserName = ServiceOperationsProviderUtilities.GetParameterValue("UserName", connectionParameters).ToValue<string>();
            Password = ServiceOperationsProviderUtilities.GetParameterValue("Password", connectionParameters).ToValue<string>();

            MaximumNumber = serviceOperationRequest.Parameters["MaximumNumber"].ToValue<int>();
            QueueName = serviceOperationRequest.Parameters["queue"].ToValue<string>();
        }
    }
}