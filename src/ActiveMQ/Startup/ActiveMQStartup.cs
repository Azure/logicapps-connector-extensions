// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Microsoft.Azure.Workflows.ServiceProvider.Extensions.ActiveMQ;

[assembly: Microsoft.Azure.WebJobs.Hosting.WebJobsStartup(typeof(ActiveMQStartup))]

namespace Microsoft.Azure.Workflows.ServiceProvider.Extensions.ActiveMQ
{
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Hosting;
    using Microsoft.Azure.Workflows.ServiceProvider.Extensions.ActiveMQ;
    using Microsoft.Extensions.DependencyInjection.Extensions;

    /// <summary>
    /// ActiveMQ startup class.
    /// </summary>
    public class ActiveMQStartup : IWebJobsStartup
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