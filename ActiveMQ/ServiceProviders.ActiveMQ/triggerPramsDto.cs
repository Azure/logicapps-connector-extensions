//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

using Microsoft.Azure.Workflows.ServiceProviders.Abstractions;
using Microsoft.WindowsAzure.ResourceStack.Common.Collections;
using Microsoft.WindowsAzure.ResourceStack.Common.Extensions;
using Newtonsoft.Json.Linq;

namespace ServiceProviders.ActiveMQ.Extension
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

            this.BrokerUri = ServiceOperationsProviderUtilities.GetParameterValue("BrokerUri", connectionParameters).ToValue<string>();
            this.ClientId = ServiceOperationsProviderUtilities.GetParameterValue("ClientId", connectionParameters).ToValue<string>();
            this.UserName = ServiceOperationsProviderUtilities.GetParameterValue("UserName", connectionParameters).ToValue<string>();
            this.Password = ServiceOperationsProviderUtilities.GetParameterValue("Password", connectionParameters).ToValue<string>();

            this.MaximumNo = serviceOperationRequest.Parameters["MaximumNo"].ToValue<int>();
            this.QueueName = serviceOperationRequest.Parameters["queue"].ToValue<string>();
        }
    }
}