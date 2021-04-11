// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Microsoft.Azure.Workflows.ServiceProvider.Extensions.ActiveMQ.Tests
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Host.TestCommon;
    using Microsoft.Azure.Workflows.ServiceProviders.Abstractions;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Microsoft.WindowsAzure.ResourceStack.Common.Extensions;
    using Xunit;

    /// <summary>
    /// ActiveMQ end to end tests.
    /// </summary>
    [Trait("Category", "E2E")]
    public class ActiveMQEndToEndTests
    {
        /// <summary>
        /// Logger provider.
        /// </summary>
        private readonly TestLoggerProvider loggerProvider = new TestLoggerProvider();

        /// <summary>
        /// ActiveMQ EndToEnd Test.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ActiveMQEndToEnd()
        {
            using var host = await this.StartHostAsync();
            var alllogs = this.loggerProvider.GetAllLogMessages();
            var logsList = alllogs.ToList();
            Assert.True(logsList.Where(item => item.FormattedMessage.ContainsInsensitively("Job host started")).Count() >= 1);
            var services = (ActiveMQTriggerServiceOperationProvider)host.Services.GetRequiredService(typeof(ActiveMQTriggerServiceOperationProvider));
            Assert.NotNull(services);
         
            var service = services.GetService();
            Assert.Equal("/serviceProviders/activemq", service.Id);
            Assert.Equal("activemq", service.Name);
            var operations = services.GetOperations(false);
            Assert.Single(operations.ToList());
            Assert.Equal("ActiveMQ_ReceiveMessages", services.GetOperations(false).FirstOrDefault().Name);
        }

        /// <summary>
        /// Start web host.
        /// </summary>
        /// <returns>IHost task.</returns>
        private async Task<IHost> StartHostAsync()
        {
            var localSettings = new Dictionary<string, string>
            {
                ["AzureWebJobsStorage"] = "UseDevelopmentStorage=true",
            };

            var host = new HostBuilder()
                 .ConfigureWebJobs(builder =>
                 {
                     
                     var serviceOperationProvider = new ServiceOperationsProvider();
                     var operationProvider = new ActiveMQTriggerServiceOperationProvider();
                     builder.AddExtension(new ActiveMQServiceProvider(serviceOperationProvider, operationProvider));
                     builder.Services.TryAddSingleton<ActiveMQTriggerServiceOperationProvider>();
                 })
                  .ConfigureLogging(b =>
                  {
                      b.AddProvider(this.loggerProvider);
                  })
                 .ConfigureAppConfiguration(c =>
                 {
                     c.AddInMemoryCollection(new[]
                    {
                        new KeyValuePair<string, string>("AzureWebJobsStorage", "UseDevelopmentStorage=true"),
                        new KeyValuePair<string, string>("FUNCTIONS_WORKER_RUNTIME", "dotnet"),
                    });
                 })
                 .Build();

            await host.StartAsync();
            return host;
        }
    }
}