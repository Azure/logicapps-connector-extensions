// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Azure.Storage.Blobs;
using FluentFTP;
using Microsoft.Azure.Workflows.ServiceProvider.Extensions.FTP.Providers;
using Microsoft.Azure.Workflows.ServiceProviders.Abstractions;
using Microsoft.WindowsAzure.ResourceStack.Common.Json;
using Microsoft.WindowsAzure.ResourceStack.Common.Memory;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Connectors.FTPCore
{
    public class FTPConfig
    {
        public FTPConfig()
        {

        }
        public FTPConfig(string host, string username, string password, bool useSSL, bool implicitMode, bool useSelfSignedCert, bool activeMode, bool useBinaryMode)
        {
            UseSSL = useSSL;
            Host = host;
            UserName = username;
            Password = password;
            ImplicitMode = implicitMode;
            ActiveMode = activeMode;
            UseSelfSignedCert = useSelfSignedCert;
            UseBinaryMode = useBinaryMode;
        }

        public string Host { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public bool UseSSL{ get; set; }
        public bool ActiveMode { get; set; }
        public bool ImplicitMode { get; set; }
        public bool UseSelfSignedCert { get; set; }
        public bool UseBinaryMode { get; set; }

    }
    public class FileList
    {
        public string Name{ get; set; }
        public string FullName { get; set; }
    }

    public class FTP
    {
        private FTPConfig _config;
        public FTP(FTPConfig config)
        {
            _config = config;
        }

        public FTP(string host, string username, string password, bool useSSL, bool implicitMode, bool useSelfSignedCert, bool activeMode, bool useBinaryMode)
        {
            _config = new FTPConfig
            {   UseSSL = useSSL,
                Host = host,
                UserName = username,
                Password = password,
                ImplicitMode = implicitMode,
                ActiveMode = activeMode,
                UseSelfSignedCert = useSelfSignedCert,
                UseBinaryMode = useBinaryMode
            };
        }
        public static void Client_ValidateCertificate(FtpClient control, FtpSslValidationEventArgs e)
        {
            e.Accept = true;
        }

        public void ConfigureClient(FtpClient client, FTPConfig config)
        {
            if (config.UseSSL)
            {
                if (config.UseSelfSignedCert)
                {
                    client.ValidateCertificate += Client_ValidateCertificate;
                }

                if (config.ImplicitMode)
                {
                    client.EncryptionMode = FtpEncryptionMode.Implicit;
                }
                else
                {
                    client.EncryptionMode = FtpEncryptionMode.Explicit;
                }
            }

            if(config.ActiveMode)
            {
                client.DataConnectionType = FtpDataConnectionType.AutoActive;
            }

            client.Credentials = new NetworkCredential(config.UserName, config.Password);
        }
        public async Task<ServiceOperationResponse> FluentUploadFile(string path,string data, FtpClient client)
        {
            string content = string.Empty;
            try
            {
                Stream responseStream = await client.OpenWriteAsync(path);

                if (_config.UseBinaryMode)
                {
                    byte[] binaryData = Convert.FromBase64String(data);

                    client.UploadDataType = FtpDataType.Binary;
                    using (BinaryWriter writer = new BinaryWriter(responseStream))
                    {
                        writer.Write(binaryData);
                    }
                }
                else
                {
                    using (StreamWriter writer = new StreamWriter(responseStream))
                    {
                        await writer.WriteAsync(data);
                    }
                }
                CancellationToken token = new CancellationToken();
                var reply = await client.GetReplyAsync(token).ConfigureAwait(continueOnCapturedContext: false);
                return new ServiceOperationResponse(body: reply.ToJToken(), statusCode: HttpStatusCode.OK);
            }
            catch
            {
                throw;
            }
        }

        public async Task<ServiceOperationResponse> FluentGetFile(string path, FtpClient client)
        {
            try
            {
                using (var bufferedMemoryStream = new BufferedMemoryStream())
                using (var zippedMemoryStream = new BufferedMemoryStream())
                //using (var responseStream = await client.OpenReadAsync(path).ConfigureAwait(continueOnCapturedContext: false))
                {
                    await client.DownloadAsync(bufferedMemoryStream, path).ConfigureAwait(continueOnCapturedContext: false);
                    if (bufferedMemoryStream.Length >= 104857600)
                    {
                        using (var gs = new GZipStream(zippedMemoryStream, CompressionMode.Compress))
                        {
                            bufferedMemoryStream.CopyTo(gs);
                            return new ServiceOperationResponse(body: ServiceOperationsProviderUtilities.CreateContentEnvelope(null, zippedMemoryStream.ToArray()), statusCode: HttpStatusCode.OK);
                        }
                    }
                    else
                    {
                        return new ServiceOperationResponse(body: ServiceOperationsProviderUtilities.CreateContentEnvelope(null, bufferedMemoryStream.ToArray()), statusCode: HttpStatusCode.OK);
                    }
                    //in case of compression : using (var gs = new GZipStream(zippedMemoryStream, CompressionMode.Compress))
                    //{
                    //    bufferedMemoryStream.CopyTo(gs);
                    //    return new ServiceOperationResponse(body: ServiceOperationsProviderUtilities.CreateContentEnvelope(null, zippedMemoryStream.ToArray()), statusCode: HttpStatusCode.OK);
                    //}
                }
            }
            catch 
            {
                throw;
            }
        }

        
        public async Task<string> FluentCopyFileToBlob(string path, string storageConnectionString, string targetContainer, FtpClient client)
        {
            string content = string.Empty;
            string file = System.IO.Path.GetFileName(path);

            try
            {
                using (Stream responseStream = await client.OpenReadAsync(path))
                {
                    BlobServiceClient blobServiceClient = new BlobServiceClient(storageConnectionString);
                    
                    BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(targetContainer);
                    if (_config.UseBinaryMode)
                    {
                        client.DownloadDataType = FtpDataType.Binary;
                    }
                    else
                    {
                        client.DownloadDataType = FtpDataType.ASCII;
                    }
                    BlobClient blobClient = containerClient.GetBlobClient(file);
                    await blobClient.UploadAsync(responseStream, overwrite: true);

                }
            }
            catch
            {
                throw;
            }

            return (content);

        }

        public async Task<string> FluentDeleteFile(string path, FtpClient client)
        {
            string resp = "success";
            try
            {
                if (!path.StartsWith("/"))
                {
                    path = "/" + path;
                }
                await client.DeleteFileAsync(path);
                return (resp);
            }
            catch
            {
                throw;
            }
        }

        public async Task<List<FileList>> FluentListFiles(string path, FtpClient client)
        {
            string resp = string.Empty;
            try
            {
                client.DownloadDataType = FtpDataType.ASCII;

                if (!path.StartsWith("/"))
                {
                    path = "/" + path;
                }
                var list = await client.GetListingAsync(path);
                
                List<FileList> fileList = new List<FileList>();
                foreach (var file in list)
                {
                    fileList.Add(new FileList { Name = file.Name, FullName = file.FullName });
                }

                return (fileList);
            }
            catch
            {
                throw;
            }
        }
    }
}
