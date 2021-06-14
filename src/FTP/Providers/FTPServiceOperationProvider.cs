// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.
using Connectors.FTPCore;

namespace Microsoft.Azure.Workflows.ServiceProvider.Extensions.FTP
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using FluentFTP;
    using Microsoft.Azure.Workflows.ServiceProvider.Extensions.FTP.Providers;
    using Microsoft.Azure.Workflows.ServiceProviders.Abstractions;
    using Microsoft.Azure.Workflows.ServiceProviders.WebJobs.Abstractions.Providers;
    using Microsoft.WindowsAzure.ResourceStack.Common.Collections;
    using Microsoft.WindowsAzure.ResourceStack.Common.Extensions;
    using Microsoft.WindowsAzure.ResourceStack.Common.Json;
    using Microsoft.WindowsAzure.ResourceStack.Common.Swagger.Entities;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public class FTPConnectionParameters : ConnectionParameters
    {
        /// <summary>
        /// Gets or sets the FTP connection server name.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public ConnectionStringParameters ServerName { get; set; }

        /// <summary>
        /// Gets or sets the FTP connection username.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public ConnectionStringParameters Username { get; set; }

        /// <summary>
        /// Gets or sets the FTP connection password.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public ConnectionStringParameters Password { get; set; }

        [JsonProperty(Required = Required.Default)]
        public ConnectionStringParameters UseSSL { get; set; }

        [JsonProperty(Required = Required.Default)]
        public ConnectionStringParameters ImplicitMode { get; set; }

        [JsonProperty(Required = Required.Default)]
        public ConnectionStringParameters ActiveMode { get; set; }

        [JsonProperty(Required = Required.Default)]
        public ConnectionStringParameters UseSelfSignedCert { get; set; }

        [JsonProperty(Required = Required.Default)]
        public ConnectionStringParameters UseBinaryMode { get; set; }

    }
    /// <summary>
    /// This is the service operation provider class where you define all the operations and apis.
    /// </summary>
    [ServiceOperationsProvider(Id = FTPServiceOperationProvider.ServiceId, Name = FTPServiceOperationProvider.ServiceName)]
    public class FTPServiceOperationProvider : IServiceOperationsTriggerProvider
    {
        /// <summary>
        /// The service name.
        /// </summary>
        public const string ServiceName = "FTPbuiltin";
        public const string ServiceOperationGetFile = "FTPGetFile";
        public const string ServiceOperationListFile = "FTPList";
        public const string ServiceOperationUploadFile = "FTPUploadFile";
        public const string ServiceOperationDeleteFile = "FTPDeleteFile";
        public const string ServiceOperationCopyFileToBlob = "FTPCopyFileToBlob";

        /// <summary>
        /// The message sender pool.
        /// </summary>
        private static FluentClientPool ftpClientPoolInstance;

        /// <summary>
        /// The service provider logger.
        /// </summary>
        private readonly IServiceProviderLogger logger;

        /// <summary>
        /// The service bus message sender poool.
        /// </summary>
        private readonly FluentClientPool ftpClientPool;

        /// <summary>
        /// The service id.
        /// </summary>
        public const string ServiceId = "/serviceProviders/FTPbuiltin";

        /// <summary>
        /// Gets or sets service Operations.
        /// </summary>
        private readonly List<ServiceOperation> serviceOperationsList;

        /// <summary>
        /// The set of all API Operations.
        /// </summary>
        private readonly InsensitiveDictionary<ServiceOperation> apiOperationsList;

        /// <summary>
        /// Constructor for Service operation provider.
        /// </summary>
        
        public FTPServiceOperationProvider(IServiceProviderLogger logger)
        {
            this.logger = logger;

            this.ftpClientPool = Interlocked.CompareExchange(ref FTPServiceOperationProvider.ftpClientPoolInstance, new FluentClientPool(capacity: 64, logger: this.logger), null) ??
                FTPServiceOperationProvider.ftpClientPoolInstance;

            this.serviceOperationsList = new List<ServiceOperation>();
            this.apiOperationsList = new InsensitiveDictionary<ServiceOperation>();
            
            this.apiOperationsList.AddRange(new InsensitiveDictionary<ServiceOperation>
            {
                { ServiceOperationListFile, FTPListOperation() },
                { ServiceOperationGetFile,FTPGetFile() },
                { ServiceOperationUploadFile,FTPUploadFile() },
                { ServiceOperationDeleteFile,FTPDeleteFile() },
                { ServiceOperationCopyFileToBlob,FTPCopyFileToBlob() },


            });
            
            this.serviceOperationsList.AddRange(new List<ServiceOperation>
            {
                                { FTPListOperation().CloneWithManifest(FTPListOperationServiceOperationManifest()) },
                                { FTPGetFile().CloneWithManifest(FTPGetOperationServiceOperationManifest()) },
                                { FTPUploadFile().CloneWithManifest(FTPUploadOperationServiceOperationManifest()) },
                                { FTPDeleteFile().CloneWithManifest(FTPDeleteOperationServiceOperationManifest()) },
                                { FTPCopyFileToBlob().CloneWithManifest(FTPCopyFileToBlobOperationServiceOperationManifest()) }




            });
        }
        public string GetFunctionTriggerType()
        {
            return "ftpTrigger";
        }

        /// <summary>
        /// Get binding connection information, needed for Azure function triggers.
        /// </summary>
        /// <param name="operationId"></param>
        /// <param name="connectionParameters"></param>
        /// <returns></returns>
        public string GetBindingConnectionParameter(string operationId, InsensitiveDictionary<JToken> connectionParameters, string paramName)
        {
            return ServiceOperationsProviderUtilities
                    .GetRequiredParameterValue(
                        serviceId: ServiceId,
                        operationId: operationId,
                        parameterName: paramName,
                        parameters: connectionParameters)?
                    .ToValue<string>();
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
        /// Get operations.
        /// </summary>
        /// <param name="expandManifest">Expand manifest generation.</param>
        public IEnumerable<ServiceOperation> GetOperations(bool expandManifest)
        {
            return expandManifest ? serviceOperationsList : GetApiOperations();
        }

        /// <summary>
        /// Gets the api operations.
        /// </summary>
        private IEnumerable<ServiceOperation> GetApiOperations()
        {
            return this.apiOperationsList.Values;
        }

        /// <summary>
        /// Get service operation.
        /// </summary>
        public ServiceOperationApi GetService()
        {
            return this.GetServiceOperationApi();
        }

        /// <summary>
        /// The Azure cosmos db API which defines the connection.
        /// </summary>
        private ServiceOperationApi GetServiceOperationApi()
        {
            ServiceOperationApi api = 
            new ServiceOperationApi
            {
                Name = "FTPbuiltin",
                Id = ServiceId,
                Type = DesignerApiType.ServiceProvider,
                Properties = new ServiceOperationApiProperties
                {
                    
                    BrandColor = 0xC4D5FF,
                    Description = "Connect to FTP.",
                    DisplayName = "FTP",
                    IconUri = new Uri("https://dphrstorage.blob.core.windows.net/icons/FTP.png"),
                    
                    Capabilities = new ApiCapability[] { ApiCapability.Actions},

                    ConnectionParameters = new FTPConnectionParameters
                    {
                        ServerName = new ConnectionStringParameters
                        {
                            Type = ConnectionStringType.StringType,
                            ParameterSource = ConnectionParameterSource.AppConfiguration,
                            UIDefinition = new UIDefinition
                            {
                                DisplayName = "FTP Server",
                                Description = "FTP Server Address",
                                Tooltip = "Provide FTP Server Address",
                                Constraints = new Constraints
                                {
                                    Required = "true",
                                },
                            },
                        },
                        Username = new ConnectionStringParameters
                        {
                            Type = ConnectionStringType.StringType,
                            ParameterSource = ConnectionParameterSource.AppConfiguration,
                            UIDefinition = new UIDefinition
                            {
                                DisplayName = "Username",
                                Description = "Username",
                                Tooltip = "Provide FTP Username",
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
                                Description = "Password",
                                Tooltip = "Provide FTP Password",
                                Constraints = new Constraints
                                {
                                    Required = "true",
                                },
                            },
                        },
                        UseSSL = new ConnectionStringParameters
                        {
                            Type = ConnectionStringType.BooleanType,
                            ParameterSource = ConnectionParameterSource.AppConfiguration,
                            UIDefinition = new UIDefinition
                            {
                                DisplayName = "Use SSL",
                                Description = "Use SSL",
                                Tooltip = "Specify whether to use SSL",
                                Constraints = new Constraints
                                {
                                    Required = "false",
                                },
                            },
                        },
                        ImplicitMode = new ConnectionStringParameters
                        {
                            Type = ConnectionStringType.BooleanType,
                            ParameterSource = ConnectionParameterSource.AppConfiguration,
                            UIDefinition = new UIDefinition
                            {
                                DisplayName = "Implicit Mode",
                                Description = "Implicit Mode (default explicit)",
                                Tooltip = "Implicit Mode (default explicit)",
                                Constraints = new Constraints
                                {
                                    Required = "false",
                                },
                            },
                        },
                        ActiveMode = new ConnectionStringParameters
                        {
                            Type = ConnectionStringType.BooleanType,
                            ParameterSource = ConnectionParameterSource.AppConfiguration,
                            UIDefinition = new UIDefinition
                            {
                                DisplayName = "Use Active Mode",
                                Description = "Use Active  Mode",
                                Tooltip = "Specify to Use active mode",
                                Constraints = new Constraints
                                {
                                    Required = "false",
                                },
                            },
                        },
                        UseSelfSignedCert = new ConnectionStringParameters
                        {
                            Type = ConnectionStringType.BooleanType,
                            ParameterSource = ConnectionParameterSource.AppConfiguration,
                            UIDefinition = new UIDefinition
                            {
                                DisplayName = "Use Self Signed Cert",
                                Description = "Use Self Signed Cert (skip validation)",
                                Tooltip = "Use Self Signed Cert (skip validation)",
                                Constraints = new Constraints
                                {
                                    Required = "false",
                                },
                            },
                        },
                        UseBinaryMode = new ConnectionStringParameters
                        {
                            Type = ConnectionStringType.BooleanType,
                            ParameterSource = ConnectionParameterSource.AppConfiguration,
                            UIDefinition = new UIDefinition
                            {
                                DisplayName = "Use binary transfer",
                                Description = "Use binary transfer (base64 encoded)",
                                Tooltip = "Use binary transfer (base64 encoded)",
                                Constraints = new Constraints
                                {
                                    Required = "false",
                                },
                            },
                        },
                    }
                }
            };

            return (api);
        }

        private ServiceOperationManifest FTPListOperationServiceOperationManifest()
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
                        Scopes = new OperationScope[] { OperationScope.Action },
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
                                Title = "FTP File List",
                                Description = "FTP File List",
                                Items = new SwaggerSchema
                                {
                                    Type = SwaggerSchemaType.Object,
                                    Properties = new OrdinalDictionary<SwaggerSchema>
                                    {
                                        {
                                            "Name", new SwaggerSchema
                                            {
                                                Type = SwaggerSchemaType.String,
                                                Title = "Name",
                                                Format = "string",
                                                Description = "Name",
                                            }
                                        },
                                        {
                                            "FullName", new SwaggerSchema
                                            {
                                                Type = SwaggerSchemaType.String,
                                                Title = "FullName",
                                                Format = "string",
                                                Description = "Full Name",
                                            }
                                        }

                                        },
                                    },
                                }
                            },

                    }
                },
                Inputs = new SwaggerSchema
                {
                    Type = SwaggerSchemaType.Object,
                    Properties = new OrdinalDictionary<SwaggerSchema>
                {
                    {
                        "inputParam", new SwaggerSchema
                        {
                            Type = SwaggerSchemaType.String,
                            Title = "Folder",
                            Description = "FTP Folder",
                        }
                    }
                },
                    Required = new string[]
                   {
                   "inputParam",
                   },
                },
                Connector = this.GetServiceOperationApi(),
            };
        }



        private ServiceOperationManifest FTPCopyFileToBlobOperationServiceOperationManifest()
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
                        Scopes = new OperationScope[] { OperationScope.Action },
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
                        { "body", new SwaggerSchema
                            {
                                Type = SwaggerSchemaType.String,
                                Title = "Body",
                                Description = "Body"
                            }
                        }
                    }
                },
                Inputs = new SwaggerSchema
                {
                    Type = SwaggerSchemaType.Object,
                    Properties = new OrdinalDictionary<SwaggerSchema>
                    {
                        {
                            "inputParam", new SwaggerSchema
                            {
                                Type = SwaggerSchemaType.String,
                                Title = "File path",
                                Description = "Destination file path",
                            }
                        },
                        {
                            "storageconnectionstring", new SwaggerSchema
                            {
                                Type = SwaggerSchemaType.String,
                                Title = "Storage Connection String",
                                Description = "Storage Connection String",
                            }
                        },
                        {
                            "targetcontainer", new SwaggerSchema
                            {
                                Type = SwaggerSchemaType.String,
                                Title = "Destination Container",
                                Description = "Destination Container to copy file to",
                            }
                        }
                    },
                    Required = new string[]
                   {
                       "inputParam",
                       "storageconnectionstring",
                       "targetcontainer"
                   },
                },
                Connector = this.GetServiceOperationApi(),
            };
        }

        private ServiceOperationManifest FTPDeleteOperationServiceOperationManifest()
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
                        Scopes = new OperationScope[] { OperationScope.Action },
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
                        { "body", new SwaggerSchema
                            {
                                Type = SwaggerSchemaType.String,
                                Title = "Result",
                                Description = "Result"
                            }
                        }
                    }
                },
                Inputs = new SwaggerSchema
                {
                    Type = SwaggerSchemaType.Object,
                    Properties = new OrdinalDictionary<SwaggerSchema>
                    {
                        {
                            "inputParam", new SwaggerSchema
                            {
                                Type = SwaggerSchemaType.String,
                                Title = "File path",
                                Description = "Destination file path",
                            }
                        }
                    },
                    Required = new string[]
                   {
                   "inputParam"                   },
                },
                Connector = this.GetServiceOperationApi(),
            };
        }


        private ServiceOperationManifest FTPUploadOperationServiceOperationManifest()
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
                        Scopes = new OperationScope[] { OperationScope.Action },
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
                        { "body", new SwaggerSchema
                            {
                                Type = SwaggerSchemaType.String,
                                Title = "Body",
                                Description = "Body"
                            }
                        }
                    }
                },
                Inputs = new SwaggerSchema
                {
                    Type = SwaggerSchemaType.Object,
                    Properties = new OrdinalDictionary<SwaggerSchema>
                    {
                        {
                            "inputParam", new SwaggerSchema
                            {
                                Type = SwaggerSchemaType.String,
                                Title = "File path",
                                Description = "Destination file path",
                            }
                        },
                        {
                            "content", new SwaggerSchema
                            {
                                Type = SwaggerSchemaType.String,
                                Title = "Content",
                                Description = "Content to upload",
                            }
                        }

                    },
                    Required = new string[]
                   {
                   "inputParam",
                   "content"
                   },
                },
                Connector = this.GetServiceOperationApi(),
            };
        }

        private ServiceOperationManifest FTPGetOperationServiceOperationManifest()
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
                        Scopes = new OperationScope[] { OperationScope.Action },
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
                        { "body", new SwaggerSchema
                            {
                                Type = SwaggerSchemaType.String,
                                Title = "Body",
                                Description = "Body"
                            } 
                        }                    
                    }
                },
                Inputs = new SwaggerSchema
                {
                    Type = SwaggerSchemaType.Object,
                    Properties = new OrdinalDictionary<SwaggerSchema>
                {
                    {
                        "inputParam", new SwaggerSchema
                        {
                            Type = SwaggerSchemaType.String,
                            Title = "Filename",
                            Description = "File to download",
                        }
                    }
                },
                    Required = new string[]
                   {
                   "inputParam",
                   },
                },
                Connector = this.GetServiceOperationApi(),
            };
        }

        /// <summary>
        /// The debug action.
        /// </summary>
        public ServiceOperation FTPListOperation()
        {
            ServiceOperation Operation = new ServiceOperation
            {
                Name = ServiceOperationListFile,
                Id = ServiceOperationListFile,
                Type = ServiceOperationListFile,
                Properties = new ServiceOperationProperties
                {
                    Api = this.GetServiceOperationApi().GetFlattenedApi(),
                    Summary = "List Files",
                    Description = "List Files",
                    Visibility = Visibility.Important,
                    //This is the magic setting that makes it an Action
                    OperationType = OperationType.ServiceProvider,
                    BrandColor = 0x1C3A56,
                    IconUri = new Uri("https://dphrstorage.blob.core.windows.net/icons/FTP.png")

                },
            };
            return (Operation);
        }
        public ServiceOperation FTPGetFile()
        {
            ServiceOperation Operation = new ServiceOperation
            {
                Name = ServiceOperationGetFile,
                Id = ServiceOperationGetFile,
                Type = ServiceOperationGetFile,
                Properties = new ServiceOperationProperties
                {
                    Api = this.GetServiceOperationApi().GetFlattenedApi(),
                    Summary = "Get File",
                    Description = "Get File",
                    Visibility = Visibility.Important,
                    //This is the magic setting that makes it an Action
                    OperationType = OperationType.ServiceProvider,
                    BrandColor = 0x1C3A56,
                    IconUri = new Uri("https://dphrstorage.blob.core.windows.net/icons/FTP.png")
                },
            };
            return (Operation);
        }

        
        public ServiceOperation FTPCopyFileToBlob()
        {
            ServiceOperation Operation = new ServiceOperation
            {
                Name = ServiceOperationCopyFileToBlob,
                Id = ServiceOperationCopyFileToBlob,
                Type = ServiceOperationCopyFileToBlob,
                Properties = new ServiceOperationProperties
                {
                    Api = this.GetServiceOperationApi().GetFlattenedApi(),
                    Summary = "Copy File to Blob",
                    Description = "Copy File to Blob",
                    Visibility = Visibility.Important,
                    //This is the magic setting that makes it an Action
                    OperationType = OperationType.ServiceProvider,
                    BrandColor = 0x1C3A56,
                    IconUri = new Uri("https://dphrstorage.blob.core.windows.net/icons/FTP.png")
                },
            };
            return (Operation);
        }

        public ServiceOperation FTPDeleteFile()
        {
            ServiceOperation Operation = new ServiceOperation
            {
                Name = ServiceOperationDeleteFile,
                Id = ServiceOperationDeleteFile,
                Type = ServiceOperationDeleteFile,
                Properties = new ServiceOperationProperties
                {
                    Api = this.GetServiceOperationApi().GetFlattenedApi(),
                    Summary = "Delete File",
                    Description = "Delete File",
                    Visibility = Visibility.Important,
                    //This is the magic setting that makes it an Action
                    OperationType = OperationType.ServiceProvider,
                    BrandColor = 0x1C3A56,
                    IconUri = new Uri("https://dphrstorage.blob.core.windows.net/icons/FTP.png")
                },
            };
            return (Operation);
        }

        public ServiceOperation FTPUploadFile()
        {
            ServiceOperation Operation = new ServiceOperation
            {
                Name = ServiceOperationUploadFile,
                Id = ServiceOperationUploadFile,
                Type = ServiceOperationUploadFile,
                Properties = new ServiceOperationProperties
                {
                    Api = this.GetServiceOperationApi().GetFlattenedApi(),
                    Summary = "Upload File",
                    Description = "Upload File",
                    Visibility = Visibility.Important,
                    //This is the magic setting that makes it an Action
                    OperationType = OperationType.ServiceProvider,
                    BrandColor = 0x1C3A56,
                    IconUri = new Uri("https://dphrstorage.blob.core.windows.net/icons/FTP.png")
                },
            };
            return (Operation);
        }


        public async Task<ServiceOperationResponse> InvokeOperation(string operationId, InsensitiveDictionary<JToken> connectionParameters, ServiceOperationRequest serviceOperationRequest)
        {
            string path = string.Empty;
            string outputParam = string.Empty;
            string serverName = string.Empty;
            string userName = string.Empty;
            string password = string.Empty;
            bool useSSL = false;
            bool implicitMode = false;
            bool activeMode = false;
            bool useSelfSignedCert = false;
            bool useBinaryMode = false;
            try
                {
                    var folderToken = serviceOperationRequest.Parameters.GetValueOrDefault("inputParam");

                    if (folderToken != null)
                    {
                        path = folderToken.ToString();
                    }

                    if (folderToken == null || string.IsNullOrEmpty(path))
                    {
                        throw new ApplicationException("Unable to obtain path");
                    }
                    serverName = GetBindingConnectionParameter(operationId, connectionParameters, "servername");
                    userName = GetBindingConnectionParameter(operationId, connectionParameters, "username");
                    password= GetBindingConnectionParameter(operationId, connectionParameters, "password");
                    useSSL = System.Convert.ToBoolean(GetBindingConnectionParameter(operationId, connectionParameters, "usessl"));
                    implicitMode = System.Convert.ToBoolean(GetBindingConnectionParameter(operationId, connectionParameters, "implicitmode"));
                    activeMode = System.Convert.ToBoolean(GetBindingConnectionParameter(operationId, connectionParameters, "activemode"));
                    useSelfSignedCert = System.Convert.ToBoolean(GetBindingConnectionParameter(operationId, connectionParameters, "useSelfsignedcert"));
                    useBinaryMode = System.Convert.ToBoolean(GetBindingConnectionParameter(operationId, connectionParameters, "usebinarymode"));

                    var ftpConfig = new FTPConfig(serverName, userName, password, useSSL, implicitMode, useSelfSignedCert, activeMode, useBinaryMode);
                    var FTP = new Connectors.FTPCore.FTP(ftpConfig);
                    switch (operationId)
                    {
                        case ServiceOperationGetFile:
                            await using (var ftpClient = await this.ftpClientPool.GetFtpClient(serverName, userName, password, useSSL, implicitMode, useSelfSignedCert, activeMode, useBinaryMode).ConfigureAwait(continueOnCapturedContext: false))
                            {
                                return await FTP.FluentGetFile(path, ftpClient.Value).ConfigureAwait(continueOnCapturedContext: false);
                            }

                        case ServiceOperationUploadFile:
                            string content;
                            var con = serviceOperationRequest.Parameters.GetValueOrDefault("content");

                            if (con == null)
                            {
                                throw new ApplicationException("Unable to read content");
                            }

                            content = con.ToString();
                            await using (var ftpClient = await this.ftpClientPool.GetFtpClient(serverName, userName, password, useSSL, implicitMode, useSelfSignedCert, activeMode, useBinaryMode).ConfigureAwait(continueOnCapturedContext: false))
                            {
                                var output = FTP.FluentUploadFile(path, content, ftpClient.Value);
                                JProperty fileProp = new JProperty("body", output.Result);
                                return new ServiceOperationResponse(fileProp.Value, System.Net.HttpStatusCode.OK);
                            }

                        case ServiceOperationDeleteFile:
                            await using (var ftpClient = await this.ftpClientPool.GetFtpClient(serverName, userName, password, useSSL, implicitMode, useSelfSignedCert, activeMode, useBinaryMode).ConfigureAwait(continueOnCapturedContext: false))
                            {
                                var output = FTP.FluentDeleteFile(path, ftpClient.Value);
                                JProperty fileProp = new JProperty("body", output.Result);
                                return new ServiceOperationResponse(fileProp.Value, System.Net.HttpStatusCode.OK);
                            }

                        case ServiceOperationCopyFileToBlob:
                            string storageConnectionString;
                            var storageConn = serviceOperationRequest.Parameters.GetValueOrDefault("storageconnectionstring");

                            if (storageConn == null)
                            {
                                throw new ApplicationException("Unable to read Storage Connection String");
                            }
                            storageConnectionString = storageConn.ToString();

                            string targetContainer;
                            var targetContainerParam = serviceOperationRequest.Parameters.GetValueOrDefault("targetcontainer");

                            if (targetContainerParam == null)
                            {
                                throw new ApplicationException("Unable to read Target Container");
                            }

                            targetContainer = targetContainerParam.ToString();

                            await using (var ftpClient = await this.ftpClientPool.GetFtpClient(serverName, userName, password, useSSL, implicitMode, useSelfSignedCert, activeMode, useBinaryMode).ConfigureAwait(continueOnCapturedContext: false))
                            {
                                var output = FTP.FluentCopyFileToBlob(path, storageConnectionString, targetContainer, ftpClient.Value);
                                JProperty fileProp = new JProperty("body", output.Result);
                                return new ServiceOperationResponse(fileProp.Value, System.Net.HttpStatusCode.OK);
                            }

                        case ServiceOperationListFile:
                            await using (var ftpClient = await this.ftpClientPool.GetFtpClient(serverName, userName, password, useSSL, implicitMode, useSelfSignedCert, activeMode, useBinaryMode).ConfigureAwait(continueOnCapturedContext: false))
                            {
                                var fileList = FTP.FluentListFiles(path, ftpClient.Value);
                                var obj = fileList.Result;

                                List<JObject> jobjects = new List<JObject>();
                                foreach (var doc in fileList.Result)
                                {
                                    jobjects.Add((JObject)doc.ToJToken());
                                }
                                JProperty prop = new JProperty("body", jobjects);

                                return new ServiceOperationResponse(prop.Value, System.Net.HttpStatusCode.OK);
                            }


                        default:
                            throw new NotImplementedException();
                    }
                }
                catch (FtpCommandException ex)
                {
                    HttpStatusCode code = GetStatusCode(ex.CompletionCode);
                    throw new ServiceOperationsProviderException(
                        httpStatus: code,
                        errorCode: ServiceOperationsErrorResponseCode.ServiceOperationFailed,
                        errorMessage: ex.Message + ", completion code: " + ex.CompletionCode,
                        null);
                }
                catch (Exception ex)
                {
                    throw new ServiceOperationsProviderException(
                        httpStatus: HttpStatusCode.InternalServerError,
                        errorCode: ServiceOperationsErrorResponseCode.ServiceOperationFailed,
                        errorMessage: ex.Message,
                        innerException: ex.InnerException
                        );
                }
        }

        public HttpStatusCode GetStatusCode(string code)
        {
            code = code.Trim();

            HttpStatusCode resp;

            if (code == "550") // file not found
            {
                resp = HttpStatusCode.NotFound;
            }
            else if (code.StartsWith("4")) // FTP temporary failure
            {
                resp = HttpStatusCode.ServiceUnavailable;
            }
            else
            {
                resp = HttpStatusCode.InternalServerError;
            }

            return (resp);
        }

    }

}
