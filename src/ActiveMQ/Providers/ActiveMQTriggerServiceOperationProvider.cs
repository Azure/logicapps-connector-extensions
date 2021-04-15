// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Microsoft.Azure.Workflows.ServiceProvider.Extensions.ActiveMQ
{
    using Apache.NMS;
    using Apache.NMS.AMQP;
    using Microsoft.Azure.Workflows.ServiceProviders.Abstractions;
    using Microsoft.Azure.Workflows.ServiceProviders.WebJobs.Abstractions.Providers;
    using Microsoft.WindowsAzure.ResourceStack.Common.Collections;
    using Microsoft.WindowsAzure.ResourceStack.Common.Extensions;
    using Microsoft.WindowsAzure.ResourceStack.Common.Swagger.Entities;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// This is the service operation provider class where you define all the operations and apis.
    /// </summary>
    [ServiceOperationsProvider(Id = ServiceId, Name = ServiceName)]
    public class ActiveMQTriggerServiceOperationProvider : IServiceOperationsTriggerProvider
    {
        private readonly TimeSpan messageReceiveTimeout = new TimeSpan(0, 0, 0, 1);

        public const string ServiceName = "activemq";

        public const string ServiceId = "/serviceProviders/activemq";

        private readonly List<ServiceOperation> serviceOperationsList;

        private readonly InsensitiveDictionary<ServiceOperation> apiOperationsList;

        public readonly ServiceOperation ReceiveMessagesTrigger = new ServiceOperation
        {
            Name = "ActiveMQ_ReceiveMessages",
            Id = "ActiveMQ Receive Messages",
            Type = "ActiveMQ ReceiveMessages",
            Properties = new ServiceOperationProperties
            {
                Api = AtiveMQTriggerApi.GetFlattenedApi(),
                Summary = "ActiveMQ Receive Messages",
                Description = "ActiveMQ Receive Messages",
                Visibility = Visibility.Important,
                OperationType = OperationType.ServiceProvider,
                BrandColor = 0x1C3A56,
                IconUri = new Uri(Microsoft.Azure.Workflows.ServiceProvider.Extensions.ActiveMQ.Properties.Resources.IconUri),
                Trigger = TriggerType.Batch,
            },
        };

        private static readonly ServiceOperationApi AtiveMQTriggerApi = new ServiceOperationApi
        {
            Name = "activemq",
            Id = "/serviceProviders/activemq",
            Type = DesignerApiType.ServiceProvider,
            Properties = new ServiceOperationApiProperties
            {
                BrandColor = 0xC4D5FF,
                IconUri = new Uri(Microsoft.Azure.Workflows.ServiceProvider.Extensions.ActiveMQ.Properties.Resources.IconUri),
                Description = "AtiveMQ",
                DisplayName = "AtiveMQ",
                Capabilities = new ApiCapability[] { ApiCapability.Triggers },
                ConnectionParameters = new ActiveMQConnectionParameters
                {
                    BrokerUri = new ConnectionStringParameters
                    {
                        Type = ConnectionStringType.SecureString,
                        ParameterSource = ConnectionParameterSource.AppConfiguration,
                        UIDefinition = new UIDefinition
                        {
                            DisplayName = "BrokerUri",
                            Description = "eg.  amqp://127.0.0.1:5672",
                            Tooltip = "eg.  amqp://127.0.0.1:5672",
                            Constraints = new Constraints
                            {
                                Required = "true",
                            },
                        },
                    },
                    UserName = new ConnectionStringParameters
                    {
                        Type = ConnectionStringType.SecureString,
                        ParameterSource = ConnectionParameterSource.AppConfiguration,
                        UIDefinition = new UIDefinition
                        {
                            DisplayName = "User Name",

                            Constraints = new Constraints
                            {
                                Required = "true",
                            },
                        },
                    },
                    ClientId = new ConnectionStringParameters
                    {
                        Type = ConnectionStringType.StringType,
                        ParameterSource = ConnectionParameterSource.AppConfiguration,
                        UIDefinition = new UIDefinition
                        {
                            DisplayName = "ClientId",

                            Constraints = new Constraints
                            {
                                Required = "true",
                            },
                        },
                    },
                    Password = new ConnectionStringParameters
                    {
                        Type = ConnectionStringType.SecureString,
                        ParameterSource = ConnectionParameterSource.AppConfiguration,
                        UIDefinition = new UIDefinition
                        {
                            DisplayName = "Password",

                            Constraints = new Constraints
                            {
                                Required = "true",
                            },
                        },
                    },
                },
            },
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="ActiveMQTriggerServiceOperationProvider"/> class.
        /// </summary>
        public ActiveMQTriggerServiceOperationProvider()
        {
            serviceOperationsList = new List<ServiceOperation>();
            apiOperationsList = new InsensitiveDictionary<ServiceOperation>();

            apiOperationsList.AddRange(new InsensitiveDictionary<ServiceOperation>
            {
                { "ReceiveMessagesrigger", ReceiveMessagesTrigger },
            });

            serviceOperationsList.AddRange(new List<ServiceOperation>
            {
                {
                    ReceiveMessagesTrigger.CloneWithManifest(new ServiceOperationManifest
                    {
                        ConnectionReference = new ConnectionReferenceFormat
                        {
                            ReferenceKeyFormat = ConnectionReferenceKeyFormat.ServiceProvider,
                        },
                        Settings = new OperationManifestSettings
                        {
                            SecureData = new OperationManifestSettingWithOptions<SecureDataOptions>(),
                            TrackedProperties = new OperationManifestSetting
                            {
                                Scopes = new OperationScope[] { OperationScope.Trigger },
                            },
                        },
                        InputsLocation = new InputsLocation[]
                        {
                            InputsLocation.Inputs,
                            InputsLocation.Parameters,
                        },
                        Outputs = new SwaggerSchema
                        {
                            Type = SwaggerSchemaType.Object,
                            Properties = new OrdinalDictionary<SwaggerSchema>
                            {
                                {
                                    "body", new SwaggerSchema
                                    {
                                        Type = SwaggerSchemaType.Array,
                                        Title = "Messages",
                                        Description = "Receive ActiveMQ messages as array ",
                                        Items = new SwaggerSchema
                                        {
                                            Type = SwaggerSchemaType.Object,
                                            Properties = new OrdinalDictionary<SwaggerSchema>
                                            {
                                                {
                                                    "contentData", new SwaggerSchema
                                                    {
                                                        Type = SwaggerSchemaType.String,
                                                        Title = "Content",
                                                        Format = "byte",
                                                        Description = "content",
                                                    }
                                                },
                                                {
                                                    "Properties", new SwaggerSchema
                                                    {
                                                        Type = SwaggerSchemaType.Object,
                                                        Title = "Properties",
                                                        Description = "ActiveMQ messages properties",
                                                        Properties = new OrdinalDictionary<SwaggerSchema>
                                                         {
                                                          {
                                                                "NMSMessageId", new SwaggerSchema
                                                                    {
                                                                        Type = SwaggerSchemaType.String,
                                                                        Title = "NMSMessageId",
                                                                        Format = "byte",
                                                                        Description = "NMSMessageId",
                                                                    }
                                                          },
                                                         },
                                                    }
                                                },
                                            },
                                        },
                                    }
                                },
                            },
                        },
                        Inputs = new SwaggerSchema
                        {
                            Type = SwaggerSchemaType.Object,
                            Properties = new OrdinalDictionary<SwaggerSchema>
                        {
                            {
                                "queue", new SwaggerSchema
                                {
                                    Type = SwaggerSchemaType.String,
                                    Title = "Queue name",
                                    Description = "Queue name",
                                }
                            },
                            {
                                 "MaximumNumber", new SwaggerSchema
                                {
                                    Type = SwaggerSchemaType.Number,
                                    Title = "Maximum number of messages",
                                }
                            },
                        },
                            Required = new string[]
                        {
                            "queue",
                            "MaximumNumber"
                        },
                        },
                        Connector = AtiveMQTriggerApi,
                        Trigger = TriggerType.Batch,
                        Recurrence = new RecurrenceSetting
                        {
                            Type = RecurrenceType.Basic,
                        },
                    })
                },
            });
        }

        /// <summary>
        /// Gets the connection.
        /// </summary>
        /// <param name="operationId">The operation id.</param>
        /// <param name="connectionParameters">The connection parameters.</param>
        public string GetBindingConnectionInformation(string operationId, InsensitiveDictionary<JToken> connectionParameters)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Trigger is not available.
        /// </summary>
        /// <returns></returns>
        public string GetFunctionTriggerType()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the operations.
        /// </summary>
        /// <param name="expandManifest">The expand anifest flag.</param>
        public IEnumerable<ServiceOperation> GetOperations(bool expandManifest)
        {
            return expandManifest ? serviceOperationsList : GetApiOperations();
        }

        /// <summary>
        /// Gets the operations.
        /// </summary>
        private IEnumerable<ServiceOperation> GetApiOperations()
        {
            return apiOperationsList.Values;
        }

        public ServiceOperationApi GetService()
        {
            return AtiveMQTriggerApi;
        }

        /// <summary>
        /// the InvokeOperation will be executed periodical to fitch the new ActiveMQ messages
        /// </summary>
        /// <param name="operationId"></param>
        /// <param name="connectionParameters"></param>
        /// <param name="serviceOperationRequest"></param>
        /// <returns></returns>
        public Task<ServiceOperationResponse> InvokeOperation(string operationId, InsensitiveDictionary<JToken> connectionParameters,
            ServiceOperationRequest serviceOperationRequest)
        {
            try
            {
                ServiceOpertionsProviderValidation.OperationId(operationId);

                ActiveMQTriggerParameters activeMQTriggerParameters = new ActiveMQTriggerParameters(connectionParameters, serviceOperationRequest);

                var connectionFactory = new NmsConnectionFactory(activeMQTriggerParameters.UserName, activeMQTriggerParameters.Password, activeMQTriggerParameters.BrokerUri);
                using (var connection = connectionFactory.CreateConnection())
                {
                    connection.ClientId = activeMQTriggerParameters.ClientId;

                    using (var session = connection.CreateSession(AcknowledgementMode.Transactional))
                    {
                        using (var queue = session.GetQueue(activeMQTriggerParameters.QueueName))
                        {
                            using (var consumer = session.CreateConsumer(queue))
                            {
                                connection.Start();

                                List<JObject> receiveMessages = new List<JObject>();

                                for (int i = 0; i < activeMQTriggerParameters.MaximumNumber; i++)
                                {
                                    var message = consumer.Receive(messageReceiveTimeout) as ITextMessage;
                                    if (message != null)
                                    {
                                        receiveMessages.Add(new JObject
                                    {
                                        { "contentData", message.Text },
                                        { "Properties", new JObject{ { "NMSMessageId", message.NMSMessageId } } },
                                    });
                                    }
                                    else
                                    {
                                        //the we will exit the loop if there are no message
                                        break;
                                    }
                                }
                                session.Commit();
                                session.Close();
                                connection.Close();

                                if (receiveMessages.Count == 0)
                                {
                                    return Task.FromResult((ServiceOperationResponse)new ActiveMQTriggerResponse(JObject.FromObject(new { message = "No messages" }), System.Net.HttpStatusCode.Accepted));
                                }
                                else
                                {
                                    return Task.FromResult((ServiceOperationResponse)new ActiveMQTriggerResponse(JArray.FromObject(receiveMessages), System.Net.HttpStatusCode.OK));
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                var error = e.Message;
                return Task.FromResult((ServiceOperationResponse)new ActiveMQTriggerResponse(JObject.FromObject(new { message = error }), System.Net.HttpStatusCode.InternalServerError));
            }
        }
    }
}