//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------;

namespace ServiceProviders.ActiveMQ.Extension
{
    using Microsoft.Azure.Workflows.ServiceProviders.Abstractions;
    using Newtonsoft.Json.Linq;
    using System.Net;
    using System.Threading.Tasks;

    /// <summary>
    /// The custom trigger response.
    /// </summary>
    public class ActiveMQTriggerResponse : ServiceOperationResponse
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CustomTriggerResponse"/> class.
        /// </summary>
        /// <param name="body">The body.</param>
        /// <param name="statusCode">The HTTP status code.</param>
        public ActiveMQTriggerResponse(JToken body, HttpStatusCode statusCode)
            : base(body, statusCode)
        {
        }

        /// <summary>
        /// Completes the operation.
        /// </summary>
        public override Task CompleteOperation()
        {
            return Task.FromResult<object>(null);
        }

        /// <summary>
        /// Fails the operation.
        /// </summary>
        public override Task FailOperation()
        {
            return Task.FromResult<object>(null);
        }
    }
}