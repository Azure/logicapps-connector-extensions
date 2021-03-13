// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Microsoft.Azure.Workflows.ServiceProvider.Extensions.CosmosDB.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Cosmos;
    using Microsoft.Azure.WebJobs.Host.TestCommon;
    using Microsoft.Extensions.Configuration;
    using Moq;
    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// Cosmos DB End2End tests.
    /// </summary>
    public class CosmosDBEndToEndTests
    {
        private readonly ITestOutputHelper outputLogger;

        /// <summary>
        /// Initializes a new instance of the <see cref="CosmosDBEndToEndTests"/> class.
        /// </summary>
        /// <param name="output">Output</param>
        public CosmosDBEndToEndTests(ITestOutputHelper output)
        {
            this.outputLogger = output;

            var config = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .AddTestSettings()
                .Build();

            // Add all test configuration to the environment as WebJobs requires a few of them to be in the environment
            foreach (var kv in config.AsEnumerable())
            {
                Environment.SetEnvironmentVariable(kv.Key, kv.Value);
            }

            config.GetConnectionStringOrSetting(ServiceBus.Constants.DefaultConnectionStringName);


            CosmosClient client = new CosmosClient(conn)
            this.Cleanup().GetAwaiter().GetResult();
        }

        [Fact]
        public void Test1()
        {

        }

        private async Task Cleanup()
        {
            var tasks = new List<Task>
            {
                CleanUpEntity(FirstQueueName),
                CleanUpEntity(SecondQueueName),
                CleanUpEntity(BinderQueueName),
                CleanUpEntity(FirstQueueName, _secondaryConnectionString),
                CleanUpEntity(EntityNameHelper.FormatSubscriptionPath(TopicName, TopicSubscriptionName1)),
                CleanUpEntity(EntityNameHelper.FormatSubscriptionPath(TopicName, TopicSubscriptionName2))
            };

            await Task.WhenAll(tasks);
        }
    }
}
