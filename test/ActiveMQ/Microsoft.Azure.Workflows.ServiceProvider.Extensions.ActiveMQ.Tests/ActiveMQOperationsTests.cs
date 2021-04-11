// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Microsoft.Azure.Workflows.ServiceProvider.Extensions.ActiveMQ.Tests
{
    using Microsoft.Azure.Workflows.ServiceProviders.Abstractions;
    using Microsoft.WindowsAzure.ResourceStack.Common.Json;
    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// ActiveMQ operation tests.
    /// </summary>
    public class ActiveMQOperationsTests
    {
        /// <summary>
        /// output logger for test.
        /// </summary>
        private readonly ITestOutputHelper outputLogger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ActiveMQOperationsTests"/> class.
        /// </summary>
        /// <param name="output">Output</param>
        public ActiveMQOperationsTests(ITestOutputHelper output)
        {
            this.outputLogger = output;
        }

        /// <summary>
        /// Operation connection parameters test.
        /// </summary>
        [Fact]
        public void OperationConnectionParametersTest()
        {
            var operations = new ActiveMQTriggerServiceOperationProvider();
            var connectionParameters = operations.GetService().Properties.ConnectionParameters as ConnectionParameters;
            var connectionStringParameters = "{\"brokerUri\": {\"type\": \"securestring\",\"parameterSource\": \"AppConfiguration\",        \"uiDefinition\": {            \"displayName\": \"BrokerUri\",            \"tooltip\": \"eg.  amqp://127.0.0.1:5672\",            \"constraints\": {                \"required\": \"true\"            },            \"description\": \"eg.  amqp://127.0.0.1:5672\"        }    },    \"clientId\": {        \"type\": \"string\",        \"parameterSource\": \"AppConfiguration\",        \"uiDefinition\": {            \"displayName\": \"ClientId\",            \"constraints\": {                \"required\": \"true\"            }        }    },    \"userName\": {        \"type\": \"securestring\",        \"parameterSource\": \"AppConfiguration\",        \"uiDefinition\": {            \"displayName\": \"User Name\",            \"constraints\": {                \"required\": \"true\"            }        }    },    \"password\": {        \"type\": \"securestring\",        \"parameterSource\": \"AppConfiguration\",        \"uiDefinition\": {            \"displayName\": \"Password\",            \"constraints\": {                \"required\": \"true\"            }        }    }}";

            var connectionParametersJsonSrting = connectionParameters.ToJson().Replace(" ", "");
            var expectedConnectionParametersJsonSrting = connectionStringParameters.Replace(" ", "");

            Assert.Equal(connectionParametersJsonSrting, expectedConnectionParametersJsonSrting);
            this.outputLogger.WriteLine("Connection parameters are matched");
        }

        /// <summary>
        /// Operation connection parameters test.
        /// </summary>
        [Fact]
        public void OperationTriggerCapabilityTest()
        {
            var operations = new ActiveMQTriggerServiceOperationProvider();
            var apiCapability = operations.GetService().Properties.Capabilities as ApiCapability[];

            Assert.Single(apiCapability);
            Assert.Equal(ApiCapability.Triggers, apiCapability[0]);
            this.outputLogger.WriteLine("Connector supports only trigger");
        }
    }
}
