// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Microsoft.Azure.Workflows.ServiceProvider.Extensions.ActiveMQ
{
    using Microsoft.Azure.Workflows.ServiceProviders.Abstractions;
    using Microsoft.WindowsAzure.ResourceStack.Common.Collections;
    using Microsoft.WindowsAzure.ResourceStack.Common.Extensions;
    using Newtonsoft.Json.Linq;

    internal class TriggerPramsDto
    {
        private InsensitiveDictionary<JToken> connectionParameters;
        private ServiceOperationRequest serviceOperationRequest;

        public string BrokerUri { get; }
        public string ClientId { get; }
        public string UserName { get; }
        public string Password { get; }
        public int MaximumNumber { get; }

        public string QueueName { get; }

        public TriggerPramsDto(InsensitiveDictionary<JToken> connectionParameters, ServiceOperationRequest serviceOperationRequest)
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