// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Microsoft.Azure.Workflows.ServiceProvider.Extensions.AzureBlob
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Threading.Tasks;
    using Microsoft.Azure.Workflows.ServiceProviders.Abstractions;
    using Microsoft.Azure.Workflows.ServiceProviders.WebJobs.Abstractions.Providers;
    using Microsoft.WindowsAzure.ResourceStack.Common.Collections;
    using Microsoft.WindowsAzure.ResourceStack.Common.Extensions;
    using Microsoft.WindowsAzure.ResourceStack.Common.Swagger.Entities;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// This is the service operation provider class where you define all the operations and apis.
    /// </summary>
    [ServiceOperationsProvider(Id = AzureBlobServiceOperationProvider.ServiceId, Name = AzureBlobServiceOperationProvider.ServiceName)]
    public class AzureBlobServiceOperationProvider : IServiceOperationsTriggerProvider
    {
        /// <summary>
        /// The service name.
        /// </summary>
        public const string ServiceName = "AzureBlob";

        /// <summary>
        /// The service id.
        /// </summary>
        public const string ServiceId = "/serviceProviders/AzureBlob";

        /// <summary>
        /// Gets or sets service Operations.
        /// </summary>
        private readonly List<ServiceOperation> serviceOperationsList;

        /// <summary>
        /// The set of all API Operations.
        /// </summary>
        private readonly InsensitiveDictionary<ServiceOperation> apiOperationsList;

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureBlobServiceOperationProvider"/> class.
        /// </summary>
        public AzureBlobServiceOperationProvider()
        {
            this.serviceOperationsList = new List<ServiceOperation>();
            this.apiOperationsList = new InsensitiveDictionary<ServiceOperation>();

            this.apiOperationsList.AddRange(new InsensitiveDictionary<ServiceOperation>
            {
                { "receiveBlob", this.GetReceiveDocumentServiceOperation() },
            });

            this.serviceOperationsList.AddRange(new List<ServiceOperation>
            {
                { this.GetReceiveDocumentServiceOperation().CloneWithManifest(this.GetServiceOperationManifest()) },
            });
        }

        /// <summary>
        /// Get binding connection information, needed for Azure function triggers.
        /// </summary>
        /// <param name="operationId">Operation id.</param>
        /// <param name="connectionParameters">Connection parameters.</param>
        /// <returns>string.</returns>
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
        /// If the registration of service provider is successful then this should resturn the trigger type.
        /// </summary>
        /// <returns>string.</returns>
        public string GetFunctionTriggerType()
        {
            return "blobTrigger";
        }

        /// <summary>
        /// Get operations.
        /// </summary>
        /// <param name="expandManifest">Expand manifest generation.</param>
        /// <returns>Service operation list.</returns>
        public IEnumerable<ServiceOperation> GetOperations(bool expandManifest)
        {
            return expandManifest ? this.serviceOperationsList : this.GetApiOperations();
        }

        /// <summary>
        /// Get service operation.
        /// </summary>
        /// <returns>Service operation api.</returns>
        public ServiceOperationApi GetService()
        {
            return this.GetServiceOperationApi();
        }

        /// <summary>
        /// The AzureBlob service provider is only trigger based connector hence skipped the implementation.
        /// </summary>
        /// <param name="operationId">Operation Id.</param>
        /// <param name="connectionParameters">Connection parameters.</param>
        /// <param name="serviceOperationRequest">Service operation request.</param>
        /// <returns>Service operation response.</returns>
        public Task<ServiceOperationResponse> InvokeOperation(string operationId, InsensitiveDictionary<JToken> connectionParameters, ServiceOperationRequest serviceOperationRequest)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Implements the Swagger schema defining the input , output and scope.
        /// </summary>
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
                                Title = "Receive blob",
                                Description = "Receive blob from Azure Blob storage",
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
                            "path", new SwaggerSchema
                            {
                                Type = SwaggerSchemaType.String,
                                Title = "Container name",
                                Description = "Container name",
                            }
                        },
                    },
                    Required = new string[]
                    {
                        "path",
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
        /// Gets the api operations.
        /// </summary>
        private IEnumerable<ServiceOperation> GetApiOperations()
        {
            return this.apiOperationsList.Values;
        }

        /// <summary>
        /// The receive documents operation which define the connection and top level design.
        /// </summary>
        private ServiceOperation GetReceiveDocumentServiceOperation()
        {
            return new ServiceOperation
            {
                Name = "receiveBlob",
                Id = "receiveBlob",
                Type = "receiveBlob",
                Properties = new ServiceOperationProperties
                {
                    Api = this.GetServiceOperationApi().GetFlattenedApi(),
                    Summary = "Receive Blob",
                    Description = "Receive blob from Azure Blob",
                    Visibility = Visibility.Important,
                    OperationType = OperationType.ServiceProvider,
                    BrandColor = 0xC4D5FF,
                    IconUri = new Uri("https://raw.githubusercontent.com/Azure/logicapps-connector-extensions/CosmosDB/src/CosmosDB/icon.png"),
                    Trigger = TriggerType.Batch,
                },
            };
        }

        /// <summary>
        /// The Azure Azure Blob API which defines the connection.
        /// </summary>
        private ServiceOperationApi GetServiceOperationApi()
        {
            return new ServiceOperationApi
            {
                Name = "AzureBlob",
                Id = ServiceId,
                Type = DesignerApiType.ServiceProvider,
                Properties = new ServiceOperationApiProperties
                {
                    BrandColor = 0xC4D5FF,
                    Description = "Connect to Azure Azure Blob to receive document.",
                    DisplayName = "Azure Blob",
                    IconUri = new Uri("https://raw.githubusercontent.com/Azure/logicapps-connector-extensions/CosmosDB/src/CosmosDB/icon.png"),
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
                                Description = "Azure Azure Blob Connection String",
                                Tooltip = "Provide Azure Azure Blob Connection String",
                                Constraints = new Constraints
                                {
                                    Required = "true",
                                },
                            },
                        },
                    },
                },
            };
         }
    }
}
