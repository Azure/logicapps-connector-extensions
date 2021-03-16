// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Microsoft.Azure.Workflows.ServiceProvider.Extensions.CosmosDB.Tests
{
    using Microsoft.Azure.Workflows.ServiceProviders.Abstractions;
    using Microsoft.WindowsAzure.ResourceStack.Common.Json;
    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// Cosmos DB operation tests.
    /// </summary>
    public class CosmosDBOperationsTests
    {
        /// <summary>
        /// output logger for test.
        /// </summary>
        private readonly ITestOutputHelper outputLogger;

        /// <summary>
        /// Initializes a new instance of the <see cref="CosmosDBOperationsTests"/> class.
        /// </summary>
        /// <param name="output">Output</param>
        public CosmosDBOperationsTests(ITestOutputHelper output)
        {
            this.outputLogger = output;
        }

        /// <summary>
        /// Operation connection parameters test.
        /// </summary>
        [Fact]
        public void OperationConnectionParametersTest()
        {
            var operations = new CosmosDBServiceOperationProvider();
            var connectionParameters = operations.GetService().Properties.ConnectionParameters as ConnectionParameters;
            var connectionStringParameters = new ConnectionStringParameters
            {
                Type = ConnectionStringType.SecureString,
                ParameterSource = ConnectionParameterSource.AppConfiguration,
                UIDefinition = new UIDefinition
                {
                    DisplayName = "Connection String",
                    Tooltip = "Provide Azure Cosmos db Connection String",
                    Constraints = new Constraints
                    {
                        Required = "true",
                    },
                    Description = "Azure Cosmos db Connection String",
                },
            };

            Assert.Equal(connectionParameters.ConnectionString.ToJson(), connectionStringParameters.ToJson());
            this.outputLogger.WriteLine("Connection parameters are matched");
        }
    }
}
