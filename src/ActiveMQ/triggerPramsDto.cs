//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

using Microsoft.Azure.Workflows.ServiceProviders.Abstractions;
using Microsoft.WindowsAzure.ResourceStack.Common.Collections;
using Microsoft.WindowsAzure.ResourceStack.Common.Extensions;
using Newtonsoft.Json.Linq;

namespace Microsoft.Azure.Workflows.ServiceProvider.Extensions.ActiveMQ
{
    internal class triggerPramsDto
    {
        private InsensitiveDictionary<JToken> connectionParameters;
        private ServiceOperationRequest serviceOperationRequest;

        public string BrokerUri { get; }
        public string ClientId { get; }
        public string UserName { get; }
        public string Password { get; }
        public int MaximumNo { get; }

        public string QueueName { get; }

        public triggerPramsDto(InsensitiveDictionary<JToken> connectionParameters, ServiceOperationRequest serviceOperationRequest)
        {
            this.connectionParameters = connectionParameters;
            this.serviceOperationRequest = serviceOperationRequest;

            BrokerUri = ServiceOperationsProviderUtilities.GetParameterValue("BrokerUri", connectionParameters).ToValue<string>();
            ClientId = ServiceOperationsProviderUtilities.GetParameterValue("ClientId", connectionParameters).ToValue<string>();
            UserName = ServiceOperationsProviderUtilities.GetParameterValue("UserName", connectionParameters).ToValue<string>();
            Password = ServiceOperationsProviderUtilities.GetParameterValue("Password", connectionParameters).ToValue<string>();

            MaximumNo = serviceOperationRequest.Parameters["MaximumNo"].ToValue<int>();
            QueueName = serviceOperationRequest.Parameters["queue"].ToValue<string>();
        }
    }
}