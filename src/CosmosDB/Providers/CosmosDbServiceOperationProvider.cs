using Microsoft.Azure.WebJobs.Description;
using Microsoft.Azure.WebJobs.Host.Config;
using Microsoft.Azure.Workflows.ServiceProviders.Abstractions;
using Microsoft.Azure.Workflows.ServiceProviders.WebJobs.Abstractions.Providers;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.WindowsAzure.ResourceStack.Common.Collections;
using Microsoft.WindowsAzure.ResourceStack.Common.Extensions;
using Microsoft.WindowsAzure.ResourceStack.Common.Storage.Cosmos;
using Microsoft.WindowsAzure.ResourceStack.Common.Swagger.Entities;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Azure.Workflows.ServiceProvider.Extensions.CosmosDB
{
    /// <summary>
    /// This is the main class where you define all the operations and apis.
    /// </summary>
    [ServiceOperationsProvider(Id = CosmosDbServiceOperationProvider.ServiceId, Name = CosmosDbServiceOperationProvider.ServiceName)]
    public class CosmosDbServiceOperationProvider : IServiceOperationsTriggerProvider
    {
        /// <summary>
        /// The service name.
        /// </summary>
        public const string ServiceName = "cosmosdb";

        /// <summary>
        /// The service id.
        /// </summary>
        public const string ServiceId = "/serviceProviders/cosmosdb";

        /// <summary>
        /// Gets or sets service Operations.
        /// </summary>
        private readonly List<ServiceOperation> serviceOperationsList;

        /// <summary>
        /// The set of all API Operations.
        /// </summary>
        private readonly InsensitiveDictionary<ServiceOperation> apiOperationsList;

        public CosmosDbServiceOperationProvider()
        {
            this.serviceOperationsList = new List<ServiceOperation>();
            this.apiOperationsList = new InsensitiveDictionary<ServiceOperation>();

            this.apiOperationsList.AddRange(new InsensitiveDictionary<ServiceOperation>
            {
                { "receiveDocument", this.GetReceiveDocumentServiceOperation() },
            });

            this.serviceOperationsList.AddRange(new List<ServiceOperation>
            {
                {  this.GetReceiveDocumentServiceOperation().CloneWithManifest(this.GetServiceOperationManifest()) }
            });
        }

        public string GetBindingConnectionInformation(string operationId, InsensitiveDictionary<JToken> connectionParameters)
        {
            return ServiceOperationsProviderUtilities
                    .GetRequiredParameterValue(
                        serviceId: ServiceId,
                        operationId: operationId,
                        parameterName: "connectionString",
                        parameters: connectionParameters)?
                    .ToValue<string>();
        }

        /// <summary>
        /// Implements the Swagger schema defining the input , output and scope.
        /// </summary>
        /// <returns></returns>
        private ServiceOperationManifest GetServiceOperationManifest()
        {
            return new ServiceOperationManifest
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
                                Title = "Receive document",
                                Description = "Receive document description",
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
                                                    Title = "documentProperties",
                                                    AdditionalProperties = new JObject
                                                    {
                                                        { "type", "object" },
                                                        { "properties", new JObject { } },
                                                        { "required", new JObject { } },
                                                    },
                                                    Description = "document data properties",
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
                            "databaseName", new SwaggerSchema
                            {
                                Type = SwaggerSchemaType.String,
                                Title = "database name",
                                Description = "database name",
                            }
                        },
                        {
                             "collectionName", new SwaggerSchema
                            {
                                Type = SwaggerSchemaType.String,
                                Title = "collection name",
                                Description = "collection name",
                            }
                        },
                        {
                             "connectionStringSetting", new SwaggerSchema
                            {
                                Type = SwaggerSchemaType.String,
                                Title = "Connection String",
                                Default = "CosmosDB-connectionString",
                                Description = "Connection-string",
                            }
                        },
                    },
                    Required = new string[]
                    {
                        "databaseName",
                        "collectionName"
                    },
                },
                Connector = this.GetServiceOperationApi(),
                Trigger = TriggerType.Batch,
                Recurrence = new RecurrenceSetting
                {
                    Type = RecurrenceType.None,
                },
            };
        }

        /// <summary>
        /// If the registration of service provider is successful then this should resturn the trigger type.
        /// </summary>
        /// <returns></returns>
        public string GetFunctionTriggerType()
        {
            return "cosmosDBTrigger";
        }

        public IEnumerable<ServiceOperation> GetOperations(bool expandManifest)
        {
            return expandManifest ? serviceOperationsList : GetApiOperations();
        }

        /// <summary>
        /// Gets the operations.
        /// </summary>
        private IEnumerable<ServiceOperation> GetApiOperations()
        {
            return this.apiOperationsList.Values;
        }

        public ServiceOperationApi GetService()
        {
            return this.GetServiceOperationApi();
        }

        /// <summary>
        /// The CosmosDB service provider is only trigger based connector hence skipped the implementation.
        /// </summary>
        /// <param name="operationId"></param>
        /// <param name="connectionParameters"></param>
        /// <param name="serviceOperationRequest"></param>
        /// <returns></returns>
        public Task<ServiceOperationResponse> InvokeOperation(string operationId, InsensitiveDictionary<JToken> connectionParameters, ServiceOperationRequest serviceOperationRequest)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The receive documents operation which define the connection and top level design.
        /// </summary>
        private ServiceOperation GetReceiveDocumentServiceOperation()
        {
            return new ServiceOperation
            {
                Name = "receiveDocument",
                Id = "receiveDocument",
                Type = "receiveDocument",
                Properties = new ServiceOperationProperties
                {
                    Api = this.GetServiceOperationApi().GetFlattenedApi(),
                    Summary = "receive document",
                    Description = "receive document",
                    Visibility = Visibility.Important,
                    OperationType = OperationType.ServiceProvider,
                    BrandColor = 0x1C3A56,
                    IconUri = new Uri("https://raw.githubusercontent.com/praveensri/LogicAppCustomConnector/main/ServiceProviders.CosmosDb.Extensions/icon.png"),
                    Trigger = TriggerType.Batch,
                },
            };
        }

        /// <summary>
        /// The Azure cosmos db API which defines the connection.
        /// </summary>
        private ServiceOperationApi GetServiceOperationApi()
        {
            return new ServiceOperationApi
            {
                Name = "cosmosdb",
                Id = ServiceId,
                Type = DesignerApiType.ServiceProvider,
                Properties = new ServiceOperationApiProperties
                {
                    BrandColor = 0xC4D5FF,
                    Description = "Connect to Azure Cosmos db to receive document.",
                    DisplayName = "Cosmos Db",
                    IconUri = new Uri("https://raw.githubusercontent.com/praveensri/LogicAppCustomConnector/main/ServiceProviders.CosmosDb.Extensions/icon.png"),
                    Capabilities = new ApiCapability[] { ApiCapability.Triggers },
                    ConnectionParameters = new ConnectionParameters
                    {
                        ConnectionString = new ConnectionStringParameters
                        {
                            Type = ConnectionStringType.SecureString,
                            ParameterSource = ConnectionParameterSource.AppConfiguration,
                            UIDefinition = new UIDefinition
                            {
                                DisplayName = "Connection String",
                                Description = "Azure Cosmos db Connection String",
                                Tooltip = "Provide Azure Cosmos db Connection String",
                                Constraints = new Constraints
                                {
                                    Required = "true",
                                }
                            }
                        }
                    }
                }
            };
         }
    }
}
