// ----------------------------------------------------------------------------------
//
// Copyright Microsoft Corporation
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// ----------------------------------------------------------------------------------

namespace Microsoft.WindowsAzure.Commands.Utilities.XblCompute
{
    using System.Linq;

    using Microsoft.WindowsAzure.Commands.Utilities.CloudGame.Contract;
    using Microsoft.WindowsAzure.Commands.Utilities.XblComputeClientHelper;
    using Microsoft.WindowsAzure.Commands.Utilities.Common;
    using Microsoft.WindowsAzure.Commands.Utilities.XblCompute.Contract;
    using Microsoft.WindowsAzure.ServiceManagement;
    using Microsoft.WindowsAzure.StorageClient;
    using System;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Xml;

    /// <summary>
    ///     Implements ICloudGameClient to use HttpClient for communication
    /// </summary>
    public class XblComputeClient : IXblComputeClient
    {
        public const string CloudGameVersion = "2013-09-01";
        private readonly HttpClient _httpClient;
        private readonly HttpClient _httpXmlClient;
        private readonly string _subscriptionId;

        /// <summary>
        ///     Creates new CloudGameClient.
        /// </summary>
        /// <param name="subscription">The Windows Azure subscription data object</param>
        /// <param name="logger">The logger action</param>
        /// <param name="httpClient">The HTTP Client to use to communicate with RDFE</param>
        /// <param name="httpXmlClient">The HTTP Client for processing XML data</param>
        public XblComputeClient(WindowsAzureSubscription subscription, Action<string> logger, HttpClient httpClient, HttpClient httpXmlClient)
        {
            _subscriptionId = subscription.SubscriptionId;
            Subscription = subscription;
            Logger = logger;
            _httpClient = httpClient;
            _httpXmlClient = httpXmlClient;
        }

        /// <summary>
        ///     Creates new CloudGameClient.
        /// </summary>
        /// <param name="subscription">The Windows Azure subscription data object</param>
        /// <param name="logger">The logger action</param>
        public XblComputeClient(WindowsAzureSubscription subscription, Action<string> logger)
            : this(subscription, 
                   logger, 
                   ClientHelper.CreateCloudGameHttpClient(subscription, CloudGameUriElements.ApplicationJsonMediaType), 
                   ClientHelper.CreateCloudGameHttpClient(subscription, CloudGameUriElements.ApplicationXmlMediaType))
        {
        }

        /// <summary>
        ///     Gets or sets the subscription.
        /// </summary>
        /// <value>
        ///     The subscription.
        /// </value>
        public WindowsAzureSubscription Subscription { get; set; }

        /// <summary>
        ///     Gets or sets the logger
        /// </summary>
        /// <value>
        ///     The logger.
        /// </value>
        public Action<string> Logger { get; set; }


        /// <summary>
        ///     Gets the game packages.
        /// </summary>
        /// <param name="xblComputeName">The cloud game name.</param>
        /// <returns></returns>
        public Task<XblPackageCollectionResponse> GetXblPackages(string xblComputeName)
        {
            var url = _httpClient.BaseAddress + String.Format(CloudGameUriElements.GameImagesResourcePath, xblComputeName);
            return _httpClient.GetAsync(url, Logger).ContinueWith(tr => ClientHelper.ProcessJsonResponse<XblPackageCollectionResponse>(tr));
        }

        /// <summary>
        ///     Upload package components to a XblCompute instance.
        /// </summary>
        /// <param name="xblComputeName">The XblCompute Instance Name.</param>
        /// <param name="packageName">The name of the package</param>
        /// <param name="maxPlayers">The max number of players allowed</param>
        /// <param name="assetId">The id of a previously uploaded asset file</param>
        /// <param name="cspkgFileName">The name of the local cspkg file name</param>
        /// <param name="cspkgStream">The cspkg file stream</param>
        /// <param name="cscfgFileName">The name of the local cscfg file name</param>
        /// <param name="cscfgStream">The game cscfg file stream</param>
        /// <returns>
        /// True if successful
        /// </returns>
        public Task<bool> NewXblPackage(
            string xblComputeName,
            string packageName,
            int maxPlayers,
            string assetId,
            string cspkgFileName,
            Stream cspkgStream,
            string cscfgFileName,
            Stream cscfgStream)
        {
            Guid assetIdGuid;
            var haveAsset = Guid.TryParse(assetId, out assetIdGuid);
            var requestMetadata = new XblPackageRequest()
            {
                CspkgFilename = cspkgFileName,
                CscfgFilename = cscfgFileName,
                MaxAllowedPlayers = maxPlayers,
                MinRequiredPlayers = 1,
                Name = packageName,
                AssetId = haveAsset ? assetId : null        // GSRM currently requires NULL or a valid Guid
            };

            XblPackagePostResponse responseMetadata;
            using (var multipartFormContent = new MultipartFormDataContent())
            {
                multipartFormContent.Add(new StringContent(ClientHelper.ToJson(requestMetadata)), "metadata");
                multipartFormContent.Add(new StreamContent(cscfgStream), "packageconfig");

                var url = _httpClient.BaseAddress + String.Format(CloudGameUriElements.GameImagesResourcePath, xblComputeName);
                responseMetadata = _httpClient.PostAsync(url, multipartFormContent).ContinueWith(tr => ClientHelper.ProcessJsonResponse<XblPackagePostResponse>(tr)).Result;
           }

            try
            {
                // Use the pre-auth URL received in the response to upload the cspkg file. Wait for it to complete
                var cloudblob = new CloudBlob(responseMetadata.CspkgPreAuthUrl);
                Task.Factory.FromAsync(
                    (callback, state) => cloudblob.BeginUploadFromStream(cspkgStream, callback, state),
                    cloudblob.EndUploadFromStream,
                    TaskCreationOptions.None).Wait();
            }
            catch (StorageException)
            {
                var errorMessage = string.Format("Failed to upload cspkg for cloud game. gameId {0} cspkgName {1}", xblComputeName, cspkgFileName);
                throw ClientHelper.CreateExceptionFromJson(HttpStatusCode.Ambiguous, errorMessage);
            }

            var result = false;
            using (var multipartFormContent = new MultipartFormDataContent())
            {
                multipartFormContent.Add(new StringContent(ClientHelper.ToJson(requestMetadata)), "metadata");
                var url = _httpClient.BaseAddress + String.Format(CloudGameUriElements.GameImageResourcePath, xblComputeName, responseMetadata.GsiId);
                result = _httpClient.PutAsync(url, multipartFormContent).ContinueWith(
                    tr =>
                    {
                        var message = tr.Result;
                        if (message.IsSuccessStatusCode)
                        {
                            return true;
                        }

                        // Error result, so throw an exception
                        throw new ServiceManagementClientException(message.StatusCode,
                            new ServiceManagementError
                            {
                                Code = message.StatusCode.ToString()
                            },
                            string.Empty);
                    }).Result;
            }

            return Task<bool>.Factory.StartNew(() => result);
        }

        /// <summary>
        ///     Remove the xblPackage.
        /// </summary>
        /// <param name="xblComputeName">The XblCompute Instance Name.</param>
        /// <param name="xblPackageId">The package id.</param>
        /// <returns></returns>
        public Task<bool> RemoveXblPackage(string xblComputeName, System.Guid xblPackageId)
        {
            var url = _httpClient.BaseAddress + String.Format(CloudGameUriElements.GameImageResourcePath, xblComputeName, xblPackageId);
            return _httpClient.DeleteAsync(url).ContinueWith(
                tr =>
                    {
                        var message = tr.Result;
                        if (message.IsSuccessStatusCode)
                        {
                            return true;
                        }

                        // Error result, so throw an exception
                        throw new ServiceManagementClientException(
                            message.StatusCode,
                            new ServiceManagementError { Code = message.StatusCode.ToString() },
                            string.Empty);
                    });
        }

        /// <summary>
        ///     Gets the Xblgame modes.
        /// </summary>
        /// <param name="xblComputeName">The XblCompute Instance Name.</param>
        /// <returns></returns>
        public Task<XblGameModeCollectionResponse> GetXblGameModes(string xblComputeName)
        {
            var url = _httpClient.BaseAddress + String.Format(CloudGameUriElements.GameModesResourcePath, xblComputeName);
            return _httpClient.GetAsync(url, Logger).ContinueWith(tr => ClientHelper.ProcessJsonResponse<XblGameModeCollectionResponse>(tr));
        }

        /// <summary>
        ///     Creates a Xbl game mode.
        /// </summary>
        /// <param name="xblComputeName">The XblCompute Instance Name.</param>
        /// <param name="xblGameModeName">The game mode name</param>
        /// <param name="xblGameModeFileName">The game mode oringal filename</param>
        /// <param name="xblGameModeStream">The game mode stream.</param>
        /// <returns></returns>
        public Task<NewXblGameModeResponse> NewXblGameMode(
            string xblComputeName, 
            string xblGameModeName, 
            string xblGameModeFileName, 
            Stream xblGameModeStream)
        {
            var url = _httpClient.BaseAddress + String.Format(CloudGameUriElements.GameModesResourcePath, xblComputeName);
            var multipartContent = new MultipartFormDataContent();
            {
                var newGameMode = new GameMode()
                {
                    Name = xblGameModeName,
                    FileName = xblGameModeFileName
                };
                multipartContent.Add(new StringContent(ClientHelper.ToJson(newGameMode)), "metadata");
                multipartContent.Add(new StreamContent(xblGameModeStream), "variant");
                return _httpClient.PostAsync(url, multipartContent).ContinueWith(tr => ClientHelper.ProcessJsonResponse<NewXblGameModeResponse>(tr));
            }
        }

        /// <summary>
        ///     Remove the game mode.
        /// </summary>
        /// <param name="xblComputeName">The XblCompute Instance Name.</param>
        /// <param name="xblGameModeId">The xbl Game Mode id.</param>
        /// <returns></returns>
        public Task<bool> RemoveXblGameMode(string xblComputeName, System.Guid xblGameModeId)
        {
            var url = _httpClient.BaseAddress + String.Format(CloudGameUriElements.GameModeResourcePath, xblComputeName, xblGameModeId);
            return _httpClient.DeleteAsync(url).ContinueWith(
                tr =>
                    {
                        var message = tr.Result;
                        if (message.IsSuccessStatusCode)
                        {
                            return true;
                        }

                        // Error result, so throw an exception
                        throw new ServiceManagementClientException(
                            message.StatusCode,
                            new ServiceManagementError { Code = message.StatusCode.ToString() },
                            string.Empty);
                    });
        }

        /// <summary>
        ///     Gets the XblCcertificates.
        /// </summary>
        /// <param name="xblComputeName">The XblCompute Instance Name.</param>
        /// <returns></returns>
        public Task<XblCertificateCollectionResponse> GetXblCertificates(string xblComputeName)
        {
            var url = _httpClient.BaseAddress + String.Format(CloudGameUriElements.CertificatesResourcePath, xblComputeName);
            return _httpClient.GetAsync(url, Logger).ContinueWith(tr => ClientHelper.ProcessJsonResponse<XblCertificateCollectionResponse>(tr));
        }

        /// <summary>
        ///     Creates a XblCertificate.
        /// </summary>
        /// <param name="xblComputeName">The XblCompute Instance Name.</param>
        /// <param name="certificateName">The certificate name.</param>
        /// <param name="certificateFileName">The certificate filename.</param>
        /// <param name="certificatePassword">The certificate password.</param>
        /// <param name="certificateStream">The certificate stream.</param>
        /// <returns></returns>
        public Task<XblCertificatePostResponse> NewXblCertificate(string xblComputeName, 
            string certificateName, 
            string certificateFileName, 
            string certificatePassword,
            Stream certificateStream)
        {
            var url = _httpClient.BaseAddress + String.Format(CloudGameUriElements.CertificatesResourcePath, xblComputeName);

            var certificate = new XblCertificateRequest()
            {
                Name = certificateName,
                Filename = certificateFileName,
                Password = certificatePassword
            };

            var multipartContent = new MultipartFormDataContent();
            {
                multipartContent.Add(new StringContent(ClientHelper.ToJson(certificate)), "metadata");
                multipartContent.Add(new StreamContent(certificateStream), "certificate");
                return _httpClient.PostAsync(url, multipartContent).ContinueWith(tr => ClientHelper.ProcessJsonResponse<XblCertificatePostResponse>(tr));
            }
        }

        /// <summary>
        ///     Remove the game certificate.
        /// </summary>
        /// <param name="xblComputeName">The XblCompute Instance Name.</param>
        /// <param name="xblCertificateId">The certificate id to be removed.</param>
        /// <returns></returns>
        public Task<bool> RemoveXblCertificate(string xblComputeName, System.Guid xblCertificateId)
        {
            var url = _httpClient.BaseAddress + String.Format(CloudGameUriElements.CertificateResourcePath, xblComputeName, xblCertificateId);
            return _httpClient.DeleteAsync(url).ContinueWith(
                tr =>
                {
                    var message = tr.Result;
                    if (message.IsSuccessStatusCode)
                    {
                        return true;
                    }

                    // Error result, so throw an exception
                    throw new ServiceManagementClientException(
                        message.StatusCode,
                        new ServiceManagementError { Code = message.StatusCode.ToString() },
                        string.Empty);
                });
        }

        /// <summary>
        ///     Gets the game assets.
        /// </summary>
        /// <param name="xblComputeName">The cloud game id.</param>
        /// <returns></returns>
        public Task<XblAssetCollectionResponse> GetXblAssets(string xblComputeName)
        {
            var url = _httpClient.BaseAddress + String.Format(CloudGameUriElements.GameAssetsResourcePath, xblComputeName);
            return _httpClient.GetAsync(url, Logger).ContinueWith(tr => ClientHelper.ProcessJsonResponse<XblAssetCollectionResponse>(tr));
        }

        /// <summary>
        ///     Creates a new Xbl game asset.
        /// </summary>
        /// <param name="xblComputeName">The XblCompute Instance Name.</param>
        /// <param name="xblAssetName">The asset name.</param>
        /// <param name="xblAssetFileName">The asset filename.</param>
        /// <param name="xblAssetStream">The asset filestream.</param>
        /// <returns></returns>
        public Task<string> NewXblAsset(
            string xblComputeName, 
            string xblAssetName, 
            string xblAssetFileName, 
            Stream xblAssetStream)
        {
            // Call in to get an AssetID and preauthURL to use for upload of the asset
            var newGameAssetRequest = new XblAssetRequest()
            {
                Filename = xblAssetFileName,
                Name = xblAssetName
            };

            var multipartFormContent = new MultipartFormDataContent
            {
                {
                    new StringContent(ClientHelper.ToJson(newGameAssetRequest)),"metadata"
                }
            };

            var url = _httpClient.BaseAddress + String.Format(CloudGameUriElements.GameAssetsResourcePath, xblComputeName);
            var postAssetResult = _httpClient.PostAsync(url, multipartFormContent).ContinueWith(tr => ClientHelper.ProcessJsonResponse<XblAssetPostResponse>(tr)).Result;

            try
            {
                var cloudblob = new CloudBlob(postAssetResult.AssetPreAuthUrl);
                Task.Factory.FromAsync(
                    (callback, state) => cloudblob.BeginUploadFromStream(xblAssetStream, callback, state),
                    cloudblob.EndUploadFromStream,
                    TaskCreationOptions.None).Wait();
            }
            catch (StorageException)
            {
                var errorMessage = string.Format("Failed to upload asset file for XblCompute instance to azure storage. gameId {0}, assetId {1}", xblComputeName, postAssetResult.AssetId);
                throw ClientHelper.CreateExceptionFromJson(HttpStatusCode.Ambiguous, errorMessage);
            }

            var multpartFormContentMetadata = new MultipartFormDataContent
            {
                {
                    new StringContent(ClientHelper.ToJson(newGameAssetRequest)),"metadata"
                }
            };

            url = _httpClient.BaseAddress + String.Format(CloudGameUriElements.GameAssetResourcePath, xblComputeName, postAssetResult.AssetId);
            _httpClient.PutAsync(url, multpartFormContentMetadata).ContinueWith(
                tr =>
                    {
                        var message = tr.Result;
                        if (message.IsSuccessStatusCode)
                        {
                            return true;
                        }

                        // Error result, so throw an exception
                        throw new ServiceManagementClientException(
                            message.StatusCode,
                            new ServiceManagementError { Code = message.StatusCode.ToString() },
                            string.Empty);
                    }).Wait();

            // Return the Asset info
            return Task<string>.Factory.StartNew(() => postAssetResult.AssetId);
        }

        /// <summary>
        ///     Remove the xbl asset.
        /// </summary>
        /// <param name="xblComputeName">The XblCompute Instance Name.</param>
        /// <param name="xblAssetId">The asset id to be removed.</param>
        /// <returns></returns>
        public Task<bool> RemoveXblAsset(string xblComputeName, System.Guid xblAssetId)
        {
            string url = _httpClient.BaseAddress + String.Format(CloudGameUriElements.GameAssetResourcePath, xblComputeName, xblAssetId);
            return _httpClient.DeleteAsync(url).ContinueWith(
                tr =>
                    {
                        var message = tr.Result;
                        if (message.IsSuccessStatusCode)
                        {
                            return true;
                        }

                        // Error result, so throw an exception
                        throw new ServiceManagementClientException(
                            message.StatusCode,
                            new ServiceManagementError { Code = message.StatusCode.ToString() },
                            string.Empty);
                    });
        }

        /// <summary>
        ///     Creates a new Xbl game code file.
        /// </summary>
        /// <param name="xblComputeName">The XblCompute Instance Name.</param>
        /// <param name="xblGameServiceId">The Xbl Game service Id this code file maps to.</param>
        /// <param name="xblCodeFileName">The code file name.</param>
        /// <param name="xblCodeFileFileName">The code file filename.</param>
        /// <param name="xblCodeFileIsActive">Whether the code file should be activated or not.</param>
        /// <param name="xblCodeFileStream">The code file filestream.</param>
        /// <returns>The code file Id if successful, and error if unsuccessful.</returns>
        public Task<string> NewXblCodeFile(
            string xblComputeName,
            string xblGameServiceId,
            string xblCodeFileName,
            string xblCodeFileFileName,
            bool xblCodeFileIsActive,
            Stream xblCodeFileStream)
        {
            // Call in to get a CodefileId and preauthURL to use for upload of the code file
            var newCodeFileRequest = new XblCodeFileRequest()
            {
                Filename = xblCodeFileFileName,
                Name = xblCodeFileName,
                Active = xblCodeFileIsActive
            };

            var multipartFormContent = new MultipartFormDataContent
            {
                {
                    new StringContent(ClientHelper.ToJson(newCodeFileRequest)),
                    "metadata"
                }
            };

            var url = _httpClient.BaseAddress + String.Format(CloudGameUriElements.GameCodeFilesResourcePath, xblComputeName, xblGameServiceId);
            var postCodeFileResult = _httpClient.PostAsync(url, multipartFormContent).ContinueWith(tr => ClientHelper.ProcessJsonResponse<XblCodeFilePostResponse>(tr)).Result;

            try
            {
                var cloudblob = new CloudBlob(postCodeFileResult.CodeFilePreAuthUrl);
                Task.Factory.FromAsync(
                    (callback, state) => cloudblob.BeginUploadFromStream(xblCodeFileStream, callback, state),
                    cloudblob.EndUploadFromStream,
                    TaskCreationOptions.None).Wait();
            }
            catch (StorageException)
            {
                var errorMessage = string.Format("Failed to upload code file file for XblCompute instance to azure storage. gameId {0}, gsiId, {1} CodeFileId {2}", xblComputeName, xblGameServiceId, postCodeFileResult.CodeFileId);
                throw ClientHelper.CreateExceptionFromJson(HttpStatusCode.Ambiguous, errorMessage);
            }

            var multpartFormContentMetadata = new MultipartFormDataContent
            {
                {
                    new StringContent(ClientHelper.ToJson(newCodeFileRequest)),"metadata"
                }
            };

            url = _httpClient.BaseAddress + String.Format(CloudGameUriElements.GameCodeFileResourcePath, xblComputeName, xblGameServiceId, postCodeFileResult.CodeFileId);
            _httpClient.PutAsync(url, multpartFormContentMetadata).ContinueWith(
                tr =>
                {
                    var message = tr.Result;
                    if (message.IsSuccessStatusCode)
                    {
                        return true;
                    }

                    // Error result, so throw an exception
                    throw new ServiceManagementClientException(
                        message.StatusCode,
                        new ServiceManagementError { Code = message.StatusCode.ToString() },
                        string.Empty);
                }).Wait();

            // Return the CodeFile info
            return Task<string>.Factory.StartNew(() => postCodeFileResult.CodeFileId);
        }

        /// <summary>
        ///     Gets the game code files.
        /// </summary>
        /// <param name="xblComputeName">The cloud game name.</param>
        /// <param name="xblGameServiceId">The game service Id.</param>
        /// <returns>A collection of Code files.</returns>
        public Task<XblCodeFileCollectionResponse> GetXblCodeFiles(string xblComputeName, string xblGameServiceId)
        {
            var url = _httpClient.BaseAddress + String.Format(CloudGameUriElements.GameCodeFilesResourcePath, xblComputeName, xblGameServiceId);
            return _httpClient.GetAsync(url, Logger).ContinueWith(tr => ClientHelper.ProcessJsonResponse<XblCodeFileCollectionResponse>(tr));
        }

        /// <summary>
        ///     Sets values on the code file that need to change.
        /// </summary>
        /// <param name="xblComputeName">The XblCompute Instance Name.</param>
        /// <param name="xblGameServiceId">Game service Id for code file.</param>
        /// <param name="xblCodeFileId">The Id of the code file to change.</param>
        /// /// <param name="xblCodeFileName">The code file name.</param>
        /// <param name="xblCodeFileFileName">The code file filename.</param>
        /// <param name="xblCodeFileIsActive">Whether the code file should be activated or not.</param>
        /// <returns>True if the values were set; otherwise an error.</returns>
        public Task<bool> SetXblCodeFile(
            string xblComputeName,
            string xblGameServiceId,
            string xblCodeFileId,
            string xblCodeFileName,
            string xblCodeFileFileName,
            bool xblCodeFileIsActive)
        {
            // Create the new code file metadata
            var newCodeFileRequest = new XblCodeFileRequest()
            {
                Name = xblCodeFileName,
                Filename = xblCodeFileFileName,
                Active = xblCodeFileIsActive
            };

            var multpartFormContentMetadata = new MultipartFormDataContent
            {
                {
                    new StringContent(ClientHelper.ToJson(newCodeFileRequest)),
                    "metadata"
                }
            };

            var url = _httpClient.BaseAddress + String.Format(CloudGameUriElements.GameCodeFileResourcePath, xblComputeName, xblGameServiceId, xblCodeFileId);
            return _httpClient.PutAsync(url, multpartFormContentMetadata).ContinueWith(
                tr =>
                {
                    var message = tr.Result;
                    if (message.IsSuccessStatusCode)
                    {
                        return true;
                    }

                    // Error result, so throw an exception
                    throw new ServiceManagementClientException(
                        message.StatusCode,
                        new ServiceManagementError { Code = message.StatusCode.ToString() },
                        string.Empty);
                });
        }

        /// <summary>
        ///     Remove the xbl code file.
        /// </summary>
        /// <param name="xblComputeName">The XblCompute Instance Name.</param>
        /// <param name="xblGameServiceId">The game service Id.</param>
        /// <param name="xblCodeFileId">The code file to be removed.</param>
        /// <returns>True if successfully removed, otherwise and error.</returns>
        public Task<bool> RemoveXblCodeFile(
            string xblComputeName,
            string xblGameServiceId,
            string xblCodeFileId)
        {
            string url = _httpClient.BaseAddress + String.Format(CloudGameUriElements.GameCodeFileResourcePath, xblComputeName, xblGameServiceId, xblCodeFileId);
            return _httpClient.DeleteAsync(url).ContinueWith(
                tr =>
                {
                    var message = tr.Result;
                    if (message.IsSuccessStatusCode)
                    {
                        return true;
                    }

                    // Error result, so throw an exception
                    throw new ServiceManagementClientException(
                        message.StatusCode,
                        new ServiceManagementError { Code = message.StatusCode.ToString() },
                        string.Empty);
                });
        }

        /// <summary>
        ///     Gets the Xbl Compute summary report.
        /// </summary>
        /// <param name="xblComputeName">The XblCompute Instance Name.</param>
        /// <returns></returns>
        public Task<DashboardSummary> GetXblComputeSummaryReport(string xblComputeName)
        {
            var url = _httpClient.BaseAddress + String.Format(CloudGameUriElements.DashboardSummaryPath, xblComputeName);
            return _httpClient.GetAsync(url, Logger).ContinueWith(tr => ClientHelper.ProcessJsonResponse<DashboardSummary>(tr));
        }

        /// <summary>
        /// Gets the XblCompute deployments report.
        /// </summary>
        /// <param name="xblComputeName">The XblCompute Instance Name.</param>
        /// <returns></returns>
        public Task<XblComputeDeploymentData> GetXblComputeDeploymentsReport(string xblComputeName)
        {
            var url = _httpClient.BaseAddress + String.Format(CloudGameUriElements.DeploymentsReportPath, xblComputeName);
            return _httpClient.GetAsync(url, Logger).ContinueWith(tr => ClientHelper.ProcessJsonResponse<XblComputeDeploymentData>(tr));
        }

        /// <summary>
        /// Gets the XblCompute servicepools report.
        /// </summary>
        /// <param name="xblComputeName">The XblCompute Instance Name.</param>
        /// <returns></returns>
        public Task<XblComputePoolData> GetXblComputePoolsReport(string xblComputeName)
        {
            var url = _httpClient.BaseAddress + String.Format(CloudGameUriElements.ServicepoolsReportPath, xblComputeName);
            return _httpClient.GetAsync(url, Logger).ContinueWith(tr => ClientHelper.ProcessJsonResponse<XblComputePoolData>(tr));
        }

        /// <summary>
        ///     Creates a new XblCompute resource.
        ///     Caller must supplier either an existing Variant schema ID, or the schemaName, Filename, and content stream
        /// </summary>
        /// <param name="titleId">The title ID within the subscription to use (in Decimal form)</param>
        /// <param name="selectionOrder">The selection order to use</param>
        /// <param name="sandboxes">A comma seperated list of sandbox names</param>
        /// <param name="resourceSetIds">A comma seperated list of resource set IDs</param>
        /// <param name="name">The name of the Cloud Game</param>
        /// <param name="schemaId">The SchemaID of an existing Variant Schema</param>
        /// <param name="schemaName">The name of the game mode schema to sue</param>
        /// <param name="schemaFileName">The local schema file name (only used for reference)</param>
        /// <param name="schemaStream">The schema data as a file stream.</param>
        /// <returns>The cloud task for completion</returns>
        public Task<bool> NewXblCompute(
            string titleId,
            int selectionOrder,
            string sandboxes,
            string resourceSetIds,
            string name,
            string schemaId,
            string schemaName,
            string schemaFileName, 
            Stream schemaStream)
        {      
            // Idempotent call to do a first time registration of the cloud service wrapping container.
            ClientHelper.RegisterCloudService(_httpClient, _httpXmlClient);

            XblGameModeSchemaRequest xblGameModeSchemaRequestData = null;
            if (!String.IsNullOrEmpty(schemaId))
            {
                Guid variantSchemaId;
                if (!Guid.TryParse(schemaId, out variantSchemaId))
                {
                    throw new ServiceManagementClientException(HttpStatusCode.BadRequest,
                        new ServiceManagementError { Code = HttpStatusCode.BadRequest.ToString() },
                        "Invalid Variant Schema ID provided. Must be a Guid");                                   
                }
            }
            else
            {
                // Schema ID not provided, so must have schemaName, etc.
                if (String.IsNullOrEmpty(schemaName) || String.IsNullOrEmpty(schemaFileName) || schemaStream == null)
                {
                    throw new ServiceManagementClientException(HttpStatusCode.BadRequest,
                        new ServiceManagementError { Code = HttpStatusCode.BadRequest.ToString() },
                        "Invalid Variant Schema values provided.");                                                      
                }

                string schemaContent;
                using (var streamReader = new StreamReader(schemaStream))
                {
                    schemaContent = streamReader.ReadToEnd();
                }

                xblGameModeSchemaRequestData = new XblGameModeSchemaRequest()
                {
                    Metadata = new XblGameModeSchema()
                    {
                        Name = schemaName,
                        Filename = schemaFileName,
                        TitleId = titleId
                    },
                    Content = schemaContent
                };
            }

            var xblCompute = new XblCompute()
            {
                Name = name,
                ResourceSets = resourceSetIds,
                Sandboxes = sandboxes,
                SchemaName = schemaName,
                TitleId = titleId,
                SelectionOrder = selectionOrder
            };

            var putGameRequest = new XblComputeRequest()
            {
                XblCompute = xblCompute
            };

            // If a schemaID is provided, use that in the request, otherwise, add the schema data contract to the put request
            if (!String.IsNullOrEmpty(schemaId))
            {
                xblCompute.SchemaId = schemaId;
            }
            else
            {
                putGameRequest.XblGameModeSchema = xblGameModeSchemaRequestData;
            }

            var doc = new XmlDocument();
            var resource = new Resource()
            {
                Name = name,
                ETag = Guid.NewGuid().ToString(),       // BUGBUG What should this ETag value be?
                Plan = string.Empty,
                ResourceProviderNamespace = CloudGameUriElements.NamespaceName,
                Type = CloudGameUriElements.XboxLiveComputeResourceType,
                SchemaVersion = CloudGameUriElements.SchemaVersion,
                IntrinsicSettings = new XmlNode[]
                {
                    doc.CreateCDataSection(ClientHelper.ToJson(putGameRequest))
                }
            }; 

            var url = _httpClient.BaseAddress + String.Format(CloudGameUriElements.CloudGameResourcePath, name);
            _httpClient.PutAsXmlAsync(url, resource).ContinueWith(
                tr =>
                    {
                        var message = tr.Result;
                        if (!message.IsSuccessStatusCode)
                        {
                            // Error result, so throw an exception
                            throw new ServiceManagementClientException(message.StatusCode,
                                new ServiceManagementError
                                {
                                    Code = message.StatusCode.ToString()
                                },
                                string.Empty);
                        }

                        return true;
                    }).Wait();

            // Poll RDFE to see if the XblCompute instance has been created
            var created = false;
            var numRetries = 0;
            do
            {
                var xblComputeInstances = GetXblComputeInstances().Result;
                if (xblComputeInstances.Any(xblComputeInstance => xblComputeInstance.Name == name))
                {
                    created = true;
                }
            }
            while (!created && (numRetries++ < 10));               
            return Task<bool>.Factory.StartNew(() => created);
        }

        /// <summary>
        /// Removes a Game Service
        /// </summary>
        /// <param name="xblComputeName">The XblCompute Instance Name.</param>
        /// <returns></returns>
        public Task<bool> RemoveXblCompute(string xblComputeName)
        {
            var url = _httpClient.BaseAddress + String.Format(CloudGameUriElements.CloudGameResourcePath, xblComputeName);
            return _httpClient.DeleteAsync(url).ContinueWith(
                tr =>
                    {
                        var message = tr.Result;
                        if (message.IsSuccessStatusCode)
                        {
                            return true;
                        }

                        // Error result, so throw an exception
                        throw new ServiceManagementClientException(
                            message.StatusCode,
                            new ServiceManagementError { Code = message.StatusCode.ToString() },
                            string.Empty);
                    });
        }

        /// <summary>
        /// Gets the XblCompute instances for the Azure Game Services resource in the current subscription
        /// </summary>
        /// <returns></returns>
        public Task<XblComputeColletion> GetXblComputeInstances()
        {
            var url = _httpClient.BaseAddress + String.Format(CloudGameUriElements.GetCloudServicesResourcePath);
            return _httpXmlClient.GetAsync(url, Logger).ContinueWith(tr => ClientHelper.ProcessCloudServiceResponse(tr));

        }

        /// <summary>
        /// Gets the AzureGameServicesProperties for the current subscription
        /// </summary>
        /// <returns>The task for completion.</returns>
        public Task<AzureGameServicesPropertiesResponse> GetAzureGameServicesProperties()
        {
            var url = _httpClient.BaseAddress + String.Format(CloudGameUriElements.ResourcePropertiesPath);

            var propertyList = this._httpXmlClient.GetAsync(url, this.Logger).ContinueWith(tr => ClientHelper.ProcessXmlResponse<ResourceProviderProperties>(tr)).Result;

            if (propertyList == null)
            {
                return null;
            }

            var property = propertyList.Find((prop) => prop.Key == "publisherInfo");
            if (property == null ||
                property.Value == null)
            {
                return null;
            }

            var result = ClientHelper.DeserializeJsonToObject<AzureGameServicesPropertiesResponse>(property.Value);
            return Task<AzureGameServicesPropertiesResponse>.Factory.StartNew(() => result);
        }

        /// <summary>
        /// Publishes the cloud game.
        /// </summary>
        /// <param name="xblComputeName">Name of the cloud game.</param>
        /// <param name="sandboxes">Optional, string delimitted list of sandboxes to deploy to</param>
        /// <param name="geoRegions">Optional, string delimitted list of geo regions to deploy to</param>
        /// <returns>The task for completion.</returns>
        public Task<bool> DeployXblCompute(string xblComputeName, string sandboxes, string geoRegions)
        {
            var url = _httpClient.BaseAddress + String.Format(CloudGameUriElements.DeployCloudGamePath, xblComputeName, sandboxes, geoRegions);
            return _httpClient.PutAsync(url, null).ContinueWith(
                tr =>
                {
                    var message = tr.Result;
                    if (!message.IsSuccessStatusCode)
                    {
                        // Error result, so throw an exception
                        throw new ServiceManagementClientException(
                            message.StatusCode,
                            new ServiceManagementError { Code = message.StatusCode.ToString() },
                            string.Empty);
                    }
                    return true;
                });
        }

        /// <summary>
        /// Stops the cloud game.
        /// </summary>
        /// <param name="xblComputeName">Name of the cloud game.</param>
        /// <returns>The task for completion.</returns>
        public Task<bool> StopXblCompute(string xblComputeName)
        {
            var url = _httpClient.BaseAddress + String.Format(CloudGameUriElements.StopCloudGamePath, xblComputeName);
            return _httpClient.PutAsync(url, null).ContinueWith(
                tr =>
                {
                    var message = tr.Result;
                    if (!message.IsSuccessStatusCode)
                    {
                        // Error result, so throw an exception
                        throw new ServiceManagementClientException(
                            message.StatusCode,
                            new ServiceManagementError { Code = message.StatusCode.ToString() },
                            string.Empty);
                    }
                    return true;
                });
        }

        /// <summary>
        /// Gets the list of available diagnostic log files for the specific instance
        /// </summary>
        /// <param name="xblComputeName">Name of the cloud game.</param>
        /// <param name="instanceId">The id of the instance to get log files for</param>
        /// <returns>A list of URIs to download individual log files</returns>
        public Task<XblEnumerateDiagnosticFilesResponse> GetLogFiles(string xblComputeName, string instanceId)
        {
            var url = _httpClient.BaseAddress + String.Format(CloudGameUriElements.LogFilePath, xblComputeName, instanceId);
            return _httpClient.GetAsync(url, Logger).ContinueWith(tr => ClientHelper.ProcessJsonResponse<XblEnumerateDiagnosticFilesResponse>(tr));
        }

        /// <summary>
        /// Gets the list of available diagnostic dump files for the specific instance
        /// </summary>
        /// <param name="xblComputeName">Name of the cloud game.</param>
        /// <param name="instanceId">The id of the instance to get dump files for</param>
        /// <returns>A list of URIs to download individual dump files</returns>
        public Task<XblEnumerateDiagnosticFilesResponse> GetDumpFiles(string xblComputeName, string instanceId)
        {
            var url = _httpClient.BaseAddress + String.Format(CloudGameUriElements.DumpFilePath, xblComputeName, instanceId);
            return _httpClient.GetAsync(url, Logger).ContinueWith(tr => ClientHelper.ProcessJsonResponse<XblEnumerateDiagnosticFilesResponse>(tr));
        }

        /// <summary>
        /// Gets the list of clusters
        /// </summary>
        /// <param name="xblComputeName">Name of the cloud game.</param>
        /// <param name="geoRegion">The regiond to enumerate clusters from</param>
        /// <param name="status">The status to filter on</param>
        /// <returns>A list of clusters that match the region and status filter</returns>
        public Task<XblEnumerateClustersResponse> GetClusters(string xblComputeName, string geoRegion, string status)
        {
            var url = _httpClient.BaseAddress + String.Format(CloudGameUriElements.EnumerateClustersPath, xblComputeName, geoRegion, status);
            return _httpClient.GetAsync(url, Logger).ContinueWith(tr => ClientHelper.ProcessJsonResponse<XblEnumerateClustersResponse>(tr));
        }
    }
}