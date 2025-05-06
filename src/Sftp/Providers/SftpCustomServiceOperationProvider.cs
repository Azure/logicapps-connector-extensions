// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Microsoft.Azure.Workflows.ServiceProvider.Extensions.SftpCustom
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using Microsoft.Azure.Workflows.ServiceProvider.Extensions.SftpCustom.Entities;
    using Microsoft.Azure.Workflows.ServiceProviders.Abstractions;
    using Microsoft.Azure.Workflows.ServiceProviders.Abstractions.NetStandard.Logging;
    using Microsoft.Azure.Workflows.ServiceProviders.Sftp.Clients;
    using Microsoft.Azure.Workflows.ServiceProviders.Sftp.Entities;
    using Microsoft.Azure.Workflows.ServiceProviders.Sftp.Providers;
    using Microsoft.Azure.Workflows.ServiceProviders.WebJobs.Abstractions.Providers;
    using Microsoft.WindowsAzure.ResourceStack.Common.Collections;
    using Microsoft.WindowsAzure.ResourceStack.Common.Extensions;
    using Microsoft.WindowsAzure.ResourceStack.Common.Swagger.Entities;
    using Newtonsoft.Json.Linq;
    using WinSCP;

    /// <summary>
    /// This is the service operation provider class where you define all the operations and apis.
    /// </summary>
    [ServiceOperationsProvider(Id = SftpCustomServiceOperationProvider.ServiceId, Name = SftpCustomServiceOperationProvider.ServiceName)]
    public class SftpCustomServiceOperationProvider : IServiceOperationsTriggerProvider
    {
        /// <summary>
        /// The service name.
        /// </summary>
        public const string ServiceName = "sftpCustom";

        /// <summary>
        /// The service id.
        /// </summary>
        public const string ServiceId = "/serviceProviders/sftpCustom";

        private const string GetFileContentOperationId = "getFileContent";

        /// <summary>
        /// Gets the maximum number of bytes of file that can be read for Get File Content V(2) action.
        /// </summary>
        /// <remarks>Default value is 2 GB.</remarks>
        private static long MaxFileSizeReadableV2 => 1024L * 1024 * 1024L * 2;

        /// <summary>
        /// Gets the service provider logger.
        /// </summary>
        public static IServiceProviderLogger Logger { get; private set; }

        /// <summary>
        /// Gets or sets service Operations.
        /// </summary>
        private readonly List<ServiceOperation> serviceOperationsList;

        /// <summary>
        /// The set of all API Operations.
        /// </summary>
        private readonly InsensitiveDictionary<ServiceOperation> apiOperationsList;

        /// <summary>
        /// Initializes a new instance of the <see cref="SftpCustomServiceOperationProvider"/> class.
        /// </summary>
        public SftpCustomServiceOperationProvider()
        {
            //SftpCustomServiceOperationProvider.Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.serviceOperationsList = new List<ServiceOperation>();
            this.apiOperationsList = new InsensitiveDictionary<ServiceOperation>();

            this.apiOperationsList.AddRange(new InsensitiveDictionary<ServiceOperation>
            {
                { GetFileContentOperationId, SftpCustomServiceOperationProvider.GetFileContentOperation },
            });

            this.serviceOperationsList.AddRange(new List<ServiceOperation>
            {
                { SftpCustomServiceOperationProvider.GetFileContentOperation.CloneWithManifest(this.GetServiceOperationManifest()) },
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
            throw new NotImplementedException();
        }

        /// <summary>
        /// If the registration of service provider is successful then this should resturn the trigger type.
        /// </summary>
        /// <returns>string.</returns>
        public string GetFunctionTriggerType()
        {
            throw new NotImplementedException();
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
            return SftpCustomServiceOperationProvider.GetServiceOperationApi();
        }

        /// <summary>
        /// The CosmosDB service provider is only trigger based connector hence skipped the implementation.
        /// </summary>
        /// <param name="operationId">Operation Id.</param>
        /// <param name="connectionParameters">Connection parameters.</param>
        /// <param name="serviceOperationRequest">Service operation request.</param>
        /// <returns>Service operation response.</returns>
        public Task<ServiceOperationResponse> InvokeOperation(string operationId, InsensitiveDictionary<JToken> connectionParameters, ServiceOperationRequest serviceOperationRequest)
        {
            switch (operationId)
            {
                case GetFileContentOperationId:
                    return Task.FromResult(this.GetFileContent(connectionParameters, serviceOperationRequest));
                default:
                    throw new NotImplementedException($"Operation {operationId} is not implemented.");
            }
        }

        private ServiceOperationResponse GetFileContent(InsensitiveDictionary<JToken> connectionParameters, ServiceOperationRequest request)
        {
            var operationId = GetFileContentOperationId;
            var hostname = ServiceOperationsProviderUtilities.GetRequiredNonEmptyParameterValue("/serviceProviders/sftpCustom", operationId, "sshHostAddress", connectionParameters).ToValue<string>();
            var username = ServiceOperationsProviderUtilities.GetRequiredNonEmptyParameterValue("/serviceProviders/sftpCustom", operationId, "username", connectionParameters).ToValue<string>();
            var password = ServiceOperationsProviderUtilities.GetRequiredNonEmptyParameterValue("/serviceProviders/sftpCustom", operationId, "password", connectionParameters).ToValue<string>();
            var portNumber = ServiceOperationsProviderUtilities.GetParameterValue(
              parameterName: SftpApiOperationsDataProvider.PortNumberParameterKey,
              parameters: request.Parameters)
              ?.ToValue<int>() ?? 22;

            var targetFileFullPath = string.Empty;
            var session = (Session)null;
            try
            {
                var filePath = ServiceOperationsProviderUtilities
                .GetRequiredParameterValue(
                    serviceId: ServiceId,
                    operationId: GetFileContentOperationId,
                    parameterName: SftpApiOperationsDataProvider.FilePathInput,
                    parameters: request.Parameters)
                .ToString();
                var inferFileContent = ServiceOperationsProviderUtilities
                   .GetParameterValue(
                       parameterName: SftpApiOperationsDataProvider.InferContentType,
                       parameters: request.Parameters)
                   ?.ToValue<bool>() ?? true;
                var rootDirectory = this.GetRootDirectory(connectionParameters);
                targetFileFullPath = SftpCustomServiceOperationProvider.CombinePaths(rootDirectory, filePath);

                session = this.CreateSession(hostname, username, password, portNumber);

                var metadata = session.GetFileInfo(targetFileFullPath);
                var size = metadata.Length;

                if (metadata.IsDirectory)
                {
                    throw new ServiceOperationsProviderException(
                        httpStatus: HttpStatusCode.BadRequest,
                        errorCode: ServiceOperationsErrorResponseCode.BadRequest,
                        errorMessage: $"Can't read file because target path {filePath} is a directory.");
                }

                if (size > SftpCustomServiceOperationProvider.MaxFileSizeReadableV2)
                {
                    throw new ServiceOperationsProviderException(
                        httpStatus: HttpStatusCode.RequestEntityTooLarge,
                        errorCode: ServiceOperationsErrorResponseCode.ServiceOperationMessageTooLarge,
                        errorMessage: $"Can't get the file at {filePath}. The file size exceeds the default size limit.");
                }

                var fileContentType = "application/octet-stream";
                if (inferFileContent)
                {
                    MimeTypes.TryGetMimeType(metadata.Name, out fileContentType);
                }

                var wrapperStream = new SftpWrapperStream
                {
                    AllowStreamSeek = false,
                    ServiceProviderOperationStream = session.GetFile(targetFileFullPath),
                    HandleStreamAccessException = exception => throw new ServiceOperationsProviderException(
                        httpStatus: HttpStatusCode.BadGateway,
                        errorCode: ServiceOperationsErrorResponseCode.ServiceOperationFailed,
                        errorMessage: $"Get file content failed unexpectedly",
                        innerException: exception),
                };

                wrapperStream.SetLength(size);

                return new ServiceOperationResponse(new ServiceProviderStreamContent
                {
                    ContentType = new MediaTypeHeaderValue(fileContentType),
                    ResponseStream = wrapperStream,

                    // (NOTE:anandgmenon): Handle to dispose  the SFTP client after the transfer is complete. This is required to prevent memory leaks.
                    CloseRead = () => Task.Run(() => session.Dispose()),
                });
            }
            catch (Exception ex) when (!ex.IsFatal())
            {
                session?.Dispose();

                if (ex is ServiceOperationsProviderException)
                {
                    throw;
                }

                throw new ServiceOperationsProviderException(
                            httpStatus: HttpStatusCode.BadGateway,
                            errorCode: ServiceOperationsErrorResponseCode.ServiceOperationFailed,
                            errorMessage: $"Get file content failed unexpectedly",
                            innerException: ex);
            }
        }

        private Session CreateSession(string hostName, string username, string password, int port)
        {
            // Setup session options
            var sessionOptions = new SessionOptions
            {
                Protocol = Protocol.Sftp,
                HostName = hostName,
                UserName = username,
                Password = password,
                PortNumber = port,
                SshHostKeyPolicy = SshHostKeyPolicy.GiveUpSecurityAndAcceptAny,
            };

            var session = new Session();
            session.Open(sessionOptions);

            return session;
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
                        Scopes = new OperationScope[] { OperationScope.Action },
                    },
                    RetryPolicy = new OperationManifestSetting
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
                            Type = SwaggerSchemaType.Object,
                            Title = "Body of the Get File content action",
                            Properties = new OrdinalDictionary<SwaggerSchema>
                            {
                                {
                                    "fileName", new SwaggerSchema
                                    {
                                        Type = SwaggerSchemaType.String,
                                        Title = "File name",
                                        Description = "The file name",
                                    }
                                },
                                {
                                    "content", new SwaggerSchema
                                    {
                                        Title = "File content",
                                        Description = "The file content",
                                    }
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
                        SftpApiOperationsDataProvider.FilePathInput, new SwaggerSchema
                        {
                            Type = SwaggerSchemaType.String,
                            Title = "File path",
                            Description = "The file path",
                        }
                    },
                    {
                        SftpApiOperationsDataProvider.InferContentType, new SwaggerSchema
                        {
                            Type = SwaggerSchemaType.Boolean,
                            Title = "Infer Content Type",
                            Description = "Infer content type? Yes/No",
                            Default = true,
                        }
                    },
                },
                    Required = new string[]
                {
                    SftpApiOperationsDataProvider.FilePathInput,
                },
                },
                Connector = SftpCustomServiceOperationProvider.GetServiceOperationApi(),
                StatusBadge = StatusBadges.ProductionBadge,
            };
        }

        /// <summary>
        /// Get the root directory parameter value from connection parameters.
        /// </summary>
        /// <param name="connectionParameters">Connection parameters.</param>
        /// <returns>Root directory.</returns>
        private string GetRootDirectory(InsensitiveDictionary<JToken> connectionParameters)
        {
            var directory = ServiceOperationsProviderUtilities.GetParameterValue(
                parameterName: SftpApiOperationsDataProvider.RootDirectoryParameterKey,
                parameters: connectionParameters)?.ToString()?.Trim();

            if (!string.IsNullOrEmpty(directory))
            {
                this.ValidateRootDirectory(directory);

                return (directory.EndsWith('/') || directory.EndsWith('\\'))
                    ? directory.Substring(
                        startIndex: 0,
                        length: directory.Length - 1)
                    : directory;
            }

            return directory;
        }

        /// <summary>
        /// Validates the root directory.
        /// </summary>
        /// <param name="rootDirectory">The root directory.</param>
        private void ValidateRootDirectory(string rootDirectory)
        {
            if (!rootDirectory.StartsWith('\\') && !rootDirectory.StartsWith('/') && !(rootDirectory.Length > 1 && rootDirectory[1] == ':'))
            {
                throw new ServiceOperationsProviderException(
                    httpStatus: HttpStatusCode.BadRequest,
                    errorCode: ServiceOperationsErrorResponseCode.BadRequest,
                    errorMessage: "Invalid root directory");
            }
        }

        /// <summary>
        /// Gets the api operations.
        /// </summary>
        private IEnumerable<ServiceOperation> GetApiOperations()
        {
            return this.apiOperationsList.Values;
        }

        /// <summary>
        /// Get file content V2 action.
        /// </summary>
        private static readonly ServiceOperation GetFileContentOperation = new ServiceOperation
        {
            Name = GetFileContentOperationId,
            Id = GetFileContentOperationId,
            Type = GetFileContentOperationId,
            Properties = new ServiceOperationProperties
            {
                Api = SftpCustomServiceOperationProvider.GetServiceOperationApi().GetFlattenedApi(),
                Summary = "Get file content",
                Description = "Get the content from the specified file",
                Visibility = Visibility.Important,
                OperationType = OperationType.ServiceProvider,
                BrandColor = 0xe8bb00,
                IconUri = new Uri("https://logicapps.azureedge.net/icons/sftpwithssh/icon.png"),
                Annotation = new ServiceOperationAnnotation
                {
                    Status = StatusAnnotation.Production,
                    Family = SftpCustomServiceOperationProvider.ServiceId,
                },
            },
        };

        /// <summary>
        /// The Azure cosmos db API which defines the connection.
        /// </summary>
        private static ServiceOperationApi GetServiceOperationApi()
        {
            return new ServiceOperationApi
            {
                Name = SftpCustomServiceOperationProvider.ServiceName,
                Id = SftpCustomServiceOperationProvider.ServiceId,
                Type = DesignerApiType.ServiceProvider,
                Properties = new ServiceOperationApiProperties
                {
                    BrandColor = 0xe8bb00,
                    Description = Resources.Sftp_ServiceOperationApiDescription,
                    DisplayName = "SFTP Custom",
                    IconUri = new Uri("https://logicapps.azureedge.net/icons/sftpwithssh/icon.png"),
                    Capabilities = new ApiCapability[] { ApiCapability.Actions },
                    ConnectionParameters = new SftpConnectionParameters
                    {
                        SshHostAddress = new ConnectionStringParameters
                        {
                            Type = ConnectionStringType.StringType,
                            ParameterSource = ConnectionParameterSource.AppConfiguration,
                            UIDefinition = new UIDefinition
                            {
                                DisplayName = "SSH host address",
                                Description = "The IP or host name for the SSH server",
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
                                Tooltip = "The username for the SFTP server.",
                                Description = "The username for the server.",
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
                                Tooltip = "The password for the SFTP server.",
                                Description = "The password for the SFTP server.",
                            },
                        },
                        PortNumber = new ConnectionStringParameters
                        {
                            Type = ConnectionStringType.IntegerType,
                            ParameterSource = ConnectionParameterSource.AppConfiguration,
                            UIDefinition = new UIDefinition
                            {
                                DisplayName = "Port number",
                                Description = "The port number for the SFTP server",
                                Tooltip = "The port number for the SFTP server",
                            },
                        },
                        RootDirectory = new ConnectionStringParameters
                        {
                            Type = ConnectionStringType.StringType,
                            ParameterSource = ConnectionParameterSource.AppConfiguration,
                            UIDefinition = new UIDefinition
                            {
                                DisplayName = "Root directory",
                                Tooltip = "The root directory of the SFTP server",
                                Description = "The root directory of the SFTP server",
                            },
                        }
                    }
                },
            };
        }

        private static string CombinePaths(string rootDirectory, string filePath)
        {
            if (!string.IsNullOrEmpty(rootDirectory))
            {
                return rootDirectory + SftpCustomServiceOperationProvider.GetPathSeparator(rootDirectory) + filePath;
            }

            return !string.IsNullOrEmpty(filePath) && (filePath.StartsWith('\\') || filePath.StartsWith('/'))
                ? filePath
                : SftpCustomServiceOperationProvider.GetPathSeparator(filePath) + filePath;
        }

        private static char GetPathSeparator(string path)
        {
            return string.IsNullOrEmpty(path) ? '\\' : path.Contains('/', StringComparison.Ordinal) ? '/' : '\\';
        }
    }
}
