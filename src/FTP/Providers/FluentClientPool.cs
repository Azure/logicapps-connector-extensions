using Connectors.FTPCore;
using FluentFTP;
using Microsoft.Azure.Workflows.ServiceProviders.Abstractions;
using Microsoft.WindowsAzure.ResourceStack.Common.Extensions;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Azure.Workflows.ServiceProvider.Extensions.FTP.Providers
{
    public class FluentClientPool
    {
        private static IServiceProviderLogger logger;

        /// <summary>
        /// Gets or sets the load balanced service bus message sender cache.
        /// </summary>
        private LimitedCapacityAsyncCache<FluentClientCacheKey, FtpClient> Cache { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FtpClientPool"/> class.
        /// </summary>
        /// <param name="capacity">The capacity of the cache.</param>
        /// <param name="logger">The service provider logger.</param>
        public FluentClientPool(int capacity, IServiceProviderLogger logger)
        {
            FluentClientPool.logger = logger;
            this.Cache = new LimitedCapacityAsyncCache<FluentClientCacheKey, FtpClient>(
                maximumCapacity: capacity,
                valueFactory: cacheKey => FluentClientPool.CreateFtpClient(cacheKey),
                valueDisposer: ftpClient => FluentClientPool.CloseFtpCLient(ftpClient, logger));
        }

        /// <summary>
        /// Gets the message sender.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="entityName">The entity name.</param>
        public Task<IAsyncCacheItem<FtpClient>> GetFtpClient(string host, string user, string password, bool useSsl, bool implicitMode, bool useSelfSignedCert, bool activeMode, bool useBinaryMode)
        {
            return this.Cache.GetCacheItem(new FluentClientCacheKey
            {
                UserName = user,
                Password = password,
                Host = host,
                UseSSL = useSsl,
                UseSelfSignedCert = useSelfSignedCert,
                ActiveMode = activeMode,
                ImplicitMode = implicitMode,
                UseBinaryMode = useBinaryMode
            });
        }

        /// <summary>
        /// Creates messsage sender.
        /// </summary>
        /// <param name="cacheKey">The cache key.</param>
        private static Task<FtpClient> CreateFtpClient(FluentClientCacheKey cacheKey)
        {
            

            var client = new FtpClient(cacheKey.Host)
            {
                Credentials = new NetworkCredential(cacheKey.UserName, cacheKey.Password)
            };

            if (cacheKey.UseSSL)
            {
                if (cacheKey.UseSelfSignedCert)
                {
                    client.ValidateCertificate += Client_ValidateCertificate;
                }

                if (cacheKey.ImplicitMode)
                {
                    client.EncryptionMode = FtpEncryptionMode.Implicit;
                }
                else
                {
                    client.EncryptionMode = FtpEncryptionMode.Explicit;
                }
            }

            if (cacheKey.ActiveMode)
            {
                client.DataConnectionType = FtpDataConnectionType.AutoActive;
            }

            client.DownloadDataType = cacheKey.UseBinaryMode ? FtpDataType.Binary : FtpDataType.ASCII;
            client.SocketKeepAlive = true;
            client.OnLogEvent = OnFTPLogEvent;
            client.RetryAttempts = 5;
            client.DataConnectionType = FtpDataConnectionType.EPSV;
            
            return Task.FromResult(client);
        }

        private static void OnFTPLogEvent(FtpTraceLevel ftpTraceLevel, string logMessage)
        {
            FluentClientPool.logger.Debug("FluentClientAppPool", "OnFTPLogEvent", logMessage);
        }


        private static void Client_ValidateCertificate(FtpClient control, FtpSslValidationEventArgs e)
        {
            e.Accept = true;
        }

        /// <summary>
        /// Close messsage sender.
        /// </summary>
        /// <param name="FtpClient">The message sender to close.</param>
        /// <param name="logger">The service provider logger.</param>
        private static async Task CloseFtpCLient(FtpClient FtpClient, IServiceProviderLogger logger)
        {
            try
            {
                await FtpClient.DisconnectAsync().ConfigureAwait(continueOnCapturedContext: false);
            }
            catch (Exception ex) when (!ex.IsFatal())
            {
                logger.Error(
                    serviceProviderName: FTPServiceOperationProvider.ServiceName,
                    operationName: "FtpClientPool.CloseFtpCLient",
                    message: "An exception occurred while closing the message sender after evicting from cache.",
                    exception: ex);
            }
        }
    }
}
