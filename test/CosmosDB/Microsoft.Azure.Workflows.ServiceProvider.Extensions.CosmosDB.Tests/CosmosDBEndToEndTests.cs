// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Microsoft.Azure.Workflows.ServiceProvider.Extensions.CosmosDB.Tests
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
    /// Cosmos DB end to end tests.
    /// </summary>
    [Trait("Category", "E2E")]
    public class CosmosDBEndToEndTests
    {
        /// <summary>
        /// Logger provider.
        /// </summary>
        private readonly TestLoggerProvider loggerProvider = new TestLoggerProvider();

        /// <summary>
        /// CosmsoDB EndToEnd Test.
        /// </summary>
        [Fact]
        public async Task CosmosDBEndToEnd()
        {
            using (var host = await this.StartHostAsync())
            {
                var alllogs = this.loggerProvider.GetAllLogMessages();
                var logsList = alllogs.ToList();
                Assert.True(logsList.Where(item => item.FormattedMessage.ContainsInsensitively("Job host started")).Count() >= 1);
                var services = (CosmosDBServiceOperationProvider)host.Services.GetRequiredService(typeof(CosmosDBServiceOperationProvider));
                Assert.NotNull(services);
                var triggerType = services.GetFunctionTriggerType();
                Assert.Equal("cosmosDBTrigger", triggerType);
                var service = services.GetService();
                Assert.Equal("/serviceProviders/cosmosdb", service.Id);
                Assert.Equal("cosmosdb", service.Name);
                var operations = services.GetOperations(false);
                Assert.Single(operations.ToList());
                Assert.Equal("receiveDocument", services.GetOperations(false).FirstOrDefault().Name);
            }
        }

        /// <summary>
        /// Start web host.
        /// </summary>
        /// <returns>IHost task.</returns>
        private async Task<IHost> StartHostAsync()
        {
            var localSettings = new Dictionary<string, string>();
            localSettings["AzureWebJobsStorage"] = "UseDevelopmentStorage=true";

            var host = new HostBuilder()
                 .ConfigureWebJobs(builder =>
                 {
                     builder.AddCosmosDB();
                     var serviceOperationProvider = new ServiceOperationsProvider();
                     var operationProvider = new CosmosDBServiceOperationProvider();
                     builder.AddExtension(new CosmosDBServiceProvider(serviceOperationProvider, operationProvider));
                     builder.Services.TryAddSingleton<CosmosDBServiceOperationProvider>();
                 })
                  .ConfigureLogging(b =>
                  {
                      b.AddProvider(this.loggerProvider);
                  })
                 .ConfigureAppConfiguration(c =>
                 {
                     c.AddInMemoryCollection(new[] {
                        new KeyValuePair<string, string>("AzureWebJobsStorage", "UseDevelopmentStorage=true"),
                        new KeyValuePair<string, string>("FUNCTIONS_WORKER_RUNTIME","dotnet"), });
                 })
                 .Build();

            await host.StartAsync();
            return host;
        }
    }
}