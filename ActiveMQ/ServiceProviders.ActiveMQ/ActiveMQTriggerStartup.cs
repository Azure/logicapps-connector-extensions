//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

[assembly: Microsoft.Azure.WebJobs.Hosting.WebJobsStartup(typeof(ServiceProviders.ActiveMQ.Extension.ActiveMQTriggerStartup))]

namespace ServiceProviders.ActiveMQ.Extension
{
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Hosting;
    using Microsoft.Extensions.DependencyInjection.Extensions;

    /// <summary>
    /// Custom test startup class.
    /// </summary>
    public class ActiveMQTriggerStartup : IWebJobsStartup
    {
        /// <summary>
        /// Configures the service provider here.
        /// </summary>
        /// <param name="builder">The builder.</param>
        public void Configure(IWebJobsBuilder builder)
        {
            builder.AddExtension<ActiveMQServiceProvider>();
            builder.Services.TryAddSingleton<ActiveMQTriggerServiceOperationProvider>();
        }
    }
}