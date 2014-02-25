// ----------------------------------------------------------------------------------
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

namespace Microsoft.WindowsAzure.Commands.Utilities.CloudGame
{
    using Common;
    using Contract;
    using ServiceManagement;
    using StorageClient;
    using System;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Threading;
    using System.Xml;
    using Utilities.Common;

    /// <summary>
    ///     Implements ICloudGameClient to use HttpClient for communication
    /// </summary>
    public class CloudGameClient : ICloudGameClient
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
        public CloudGameClient(WindowsAzureSubscription subscription, Action<string> logger, HttpClient httpClient, HttpClient httpXmlClient)
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
        public CloudGameClient(WindowsAzureSubscription subscription, Action<string> logger)
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
        /// Gets the VM packages.
        /// </summary>
        /// <param name="cloudGameName">The cloud game name.</param>
        /// <param name="platform">The cloud game platform.</param>
        /// <returns></returns>
        public async Task<VmPackageCollectionResponse> GetVmPackages(string cloudGameName, CloudGamePlatform platform)
        {
            var url = _httpClient.BaseAddress + String.Format(CloudGameUriElements.VmPackagesResourcePath, ClientHelper.GetPlatformString(platform), cloudGameName);
            var message = await _httpClient.GetAsync(url, Logger).ConfigureAwait(false);
            return await ClientHelper.ProcessJsonResponse<VmPackageCollectionResponse>(message).ConfigureAwait(false);
        }

        /// <summary>
        /// Upload VM package components to a cloud game.
        /// </summary>
        /// <param name="cloudGameName">The cloud game name.</param>
        /// <param name="platform">The cloud game platform.</param>
        /// <param name="packageName">The name of the package.</param>
        /// <param name="maxPlayers">The max number of players allowed.</param>
        /// <param name="assetId">The Id of a previously uploaded asset file.</param>
        /// <param name="cspkgFileName">The name of the local cspkg file name.</param>
        /// <param name="cspkgStream">The cspkg file stream.</param>
        /// <param name="cscfgFileName">The name of the local cscfg file name.</param>
        /// <param name="cscfgStream">The game cscfg file stream.</param>
        /// <returns>
        /// True if successful.
        /// </returns>
        public async Task<bool> NewVmPackage(
            string cloudGameName,
            CloudGamePlatform platform,
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
            var requestMetadata = new VmPackageRequest()
            {
                CspkgFilename = cspkgFileName,
                CscfgFilename = cscfgFileName,
                MaxAllowedPlayers = maxPlayers,
                MinRequiredPlayers = 1,
                Name = packageName,
                AssetId = haveAsset ? assetId : null
            };

            var platformResourceString = ClientHelper.GetPlatformString(platform);

            VmPackagePostResponse responseMetadata;
            using (var multipartFormContent = new MultipartFormDataContent())
            {
                multipartFormContent.Add(new StringContent(ClientHelper.ToJson(requestMetadata)), "metadata");
                multipartFormContent.Add(new StreamContent(cscfgStream), "packageconfig");

                var url = _httpClient.BaseAddress + String.Format(CloudGameUriElements.VmPackagesResourcePath, platformResourceString, cloudGameName);
                var responseMessage = await _httpClient.PostAsync(url, multipartFormContent).ConfigureAwait(false);
                responseMetadata = await ClientHelper.ProcessJsonResponse<VmPackagePostResponse>(responseMessage).ConfigureAwait(false);
            }

            try
            {
                // Use the pre-auth URL received in the response to upload the cspkg file. Wait for it to complete
                var cloudblob = new CloudBlob(responseMetadata.CspkgPreAuthUrl);
                await Task.Factory.FromAsync(
                    (callback, state) => cloudblob.BeginUploadFromStream(cspkgStream, callback, state),
                    cloudblob.EndUploadFromStream,
                    TaskCreationOptions.None).ConfigureAwait(false);
            }
            catch (StorageException)
            {
                var errorMessage = string.Format("Failed to upload cspkg for cloud game. gameId {0} platform {1} cspkgName {2}", cloudGameName, platformResourceString, cspkgFileName);
                throw ClientHelper.CreateExceptionFromJson(HttpStatusCode.Ambiguous, errorMessage);
            }

            using (var multipartFormContent = new MultipartFormDataContent())
            {
                multipartFormContent.Add(new StringContent(ClientHelper.ToJson(requestMetadata)), "metadata");
                var url = _httpClient.BaseAddress + String.Format(CloudGameUriElements.VmPackageResourcePath, platformResourceString, cloudGameName, responseMetadata.VmPackageId);
                var responseMessage = await _httpClient.PutAsync(url, multipartFormContent).ConfigureAwait(false);
                if (!responseMessage.IsSuccessStatusCode)
                {
                    // Error result, so throw an exception
                    throw new ServiceManagementClientException(responseMessage.StatusCode,
                        new ServiceManagementError
                        {
                            Code = responseMessage.StatusCode.ToString()
                        },
                        string.Empty);
                }
            }

            return true;
        }

        /// <summary>
        /// Remove the VM package.
        /// </summary>
        /// <param name="cloudGameName">The cloud game name.</param>
        /// <param name="platform">The cloud game platform.</param>
        /// <param name="vmPackageId">The VM package Id.</param>
        /// <returns></returns>
        public async Task<bool> RemoveVmPackage(string cloudGameName, CloudGamePlatform platform, Guid vmPackageId)
        {
            var url = _httpClient.BaseAddress + String.Format(CloudGameUriElements.VmPackageResourcePath, ClientHelper.GetPlatformString(platform), cloudGameName, vmPackageId);
            var responseMessage = await _httpClient.DeleteAsync(url).ConfigureAwait(false);

            var message = responseMessage;
            if (message.IsSuccessStatusCode)
            {
                return true;
            }

            // Error result, so throw an exception
            throw new ServiceManagementClientException(
                message.StatusCode,
                new ServiceManagementError { Code = message.StatusCode.ToString() },
                string.Empty);
        }

        /// <summary>
        /// Gets the game modes associated with a parent schema.
        /// </summary>
        /// <param name="gameModeSchemaId">The game mode schema Id.</param>
        /// <returns></returns>
        public async Task<GameModeCollectionResponse> GetGameModes(Guid gameModeSchemaId)
        {
            var url = _httpClient.BaseAddress + String.Format(CloudGameUriElements.GameModesResourcePath, gameModeSchemaId);
            var responseMessage = await _httpClient.GetAsync(url, Logger).ConfigureAwait(false);
            return await ClientHelper.ProcessJsonResponse<GameModeCollectionResponse>(responseMessage).ConfigureAwait(false);
        }

        /// <summary>
        /// Creates a Game Mode Schema.
        /// </summary>
        /// <param name="schemaName">The game mode schema name.</param>
        /// <param name="fileName">The game mode schema original filename.</param>
        /// <param name="schemaStream">The game mode schema stream.</param>
        /// <returns></returns>
        public async Task<NewGameModeSchemaResponse> NewGameModeSchema(string schemaName, string fileName, Stream schemaStream)
        {
            // Idempotent call to do a first time registration of the subscription-level wrapping container.
            await ClientHelper.RegisterAndCreateContainerResourceIfNeeded(_httpClient, _httpXmlClient).ConfigureAwait(false);

            var url = _httpClient.BaseAddress + String.Format(CloudGameUriElements.GameModeSchemasResourcePath);
            using (var multipartContent = new MultipartFormDataContent())
            {
                var newGameModeSchema = new GameModeSchema()
                {
                    Name = schemaName,
                    Filename = fileName
                };
                multipartContent.Add(new StringContent(ClientHelper.ToJson(newGameModeSchema)), "metadata");
                multipartContent.Add(new StreamContent(schemaStream), "variantSchema");

                var responseMessage = await _httpClient.PostAsync(url, multipartContent).ConfigureAwait(false);
                return await ClientHelper.ProcessJsonResponse<NewGameModeSchemaResponse>(responseMessage).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Remove the game mode schema.
        /// </summary>
        /// <param name="gameModeSchemaId">The game mode schema ID.</param>
        /// <returns></returns>
        /// <exception cref="Microsoft.WindowsAzure.ServiceManagement.ServiceManagementClientException"></exception>
        /// <exception cref="ServiceManagementError"></exception>
        public async Task<bool> RemoveGameModeSchema(Guid gameModeSchemaId)
        {
            var url = _httpClient.BaseAddress + String.Format(CloudGameUriElements.GameModeSchemaResourcePath, gameModeSchemaId);
            var responseMessage = await _httpClient.DeleteAsync(url).ConfigureAwait(false);
            if (!responseMessage.IsSuccessStatusCode)
            {
                throw new ServiceManagementClientException(
                responseMessage.StatusCode,
                new ServiceManagementError { Code = responseMessage.StatusCode.ToString() },
                string.Empty);

            }

            // Error result, so throw an exception
            return true;
        }

        /// <summary>
        /// Gets the game mode schemas.
        /// </summary>
        /// <param name="getDetails">If the list of child game modes should be included or not.</param>
        /// <returns></returns>
        public async Task<GameModeSchemaCollectionResponse> GetGameModeSchemas(bool getDetails = false)
        {
            var url = _httpClient.BaseAddress + String.Format(CloudGameUriElements.GameModeSchemasGetResourcePath, getDetails);
            var responseMessage = await _httpClient.GetAsync(url, Logger).ConfigureAwait(false);
            return await ClientHelper.ProcessJsonResponse<GameModeSchemaCollectionResponse>(responseMessage).ConfigureAwait(false);
        }

        /// <summary>
        /// Creates a Game Mode.
        /// </summary>
        /// <param name="gameModeSchemaId">The parent game mode schema identifier.</param>
        /// <param name="gameModeName">The game mode name.</param>
        /// <param name="gameModeFileName">The game mode original filename.</param>
        /// <param name="gameModeStream">The game mode stream.</param>
        /// <returns></returns>
        public async Task<NewGameModeResponse> NewGameMode(Guid gameModeSchemaId, string gameModeName, string gameModeFileName, Stream gameModeStream)
        {
            // Container resource should already be created if the (required) game mode schema exists,
            // so no need to check that again.
            var url = _httpClient.BaseAddress + String.Format(CloudGameUriElements.GameModesResourcePath, gameModeSchemaId);
            using (var multipartContent = new MultipartFormDataContent())
            {
                var newGameMode = new GameMode()
                {
                    Name = gameModeName,
                    FileName = gameModeFileName
                };
                multipartContent.Add(new StringContent(ClientHelper.ToJson(newGameMode)), "metadata");
                multipartContent.Add(new StreamContent(gameModeStream), "variant");

                var responseMessage = await _httpClient.PostAsync(url, multipartContent).ConfigureAwait(false);
                return await ClientHelper.ProcessJsonResponse<NewGameModeResponse>(responseMessage).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Remove the game mode.
        /// </summary>
        /// <param name="gameModeSchemaId">The game mode schema ID.</param>
        /// <param name="gameModeId">The game mode ID.</param>
        /// <returns></returns>
        public async Task<bool> RemoveGameMode(Guid gameModeSchemaId, Guid gameModeId)
        {
            var url = _httpClient.BaseAddress + String.Format(CloudGameUriElements.GameModeResourcePath, gameModeSchemaId, gameModeId);
            var responseMessage = await _httpClient.DeleteAsync(url).ConfigureAwait(false);
            if (!responseMessage.IsSuccessStatusCode)
            {
                throw new ServiceManagementClientException(
                responseMessage.StatusCode,
                new ServiceManagementError { Code = responseMessage.StatusCode.ToString() },
                string.Empty);

            }

            // Error result, so throw an exception
            return true;
        }

        /// <summary>
        /// Gets the certificates.
        /// </summary>
        /// <param name="cloudGameId">An optional cloud game ID to filter by.</param>
        /// <returns></returns>
        public async Task<CertificateCollectionResponse> GetCertificates(Guid? cloudGameId)
        {
            var url = _httpClient.BaseAddress + (
                cloudGameId.HasValue ? String.Format(CloudGameUriElements.CertificatesForGameResourcePath, cloudGameId.Value) :
                String.Format(CloudGameUriElements.CertificatesResourcePath));
            var message = await _httpClient.GetAsync(url, Logger).ConfigureAwait(false);
            return await ClientHelper.ProcessJsonResponse<CertificateCollectionResponse>(message).ConfigureAwait(false);
        }

        /// <summary>
        /// Creates a certificate.
        /// </summary>
        /// <param name="certificateName">The certificate name.</param>
        /// <param name="certificateFileName">The certificate filename.</param>
        /// <param name="certificatePassword">The certificate password.</param>
        /// <param name="certificateStream">The certificate stream.</param>
        /// <returns></returns>
        public async Task<CertificatePostResponse> NewCertificate(
            string certificateName,
            string certificateFileName,
            string certificatePassword,
            Stream certificateStream)
        {
            // Idempotent call to do a first time registration of the subscription-level wrapping container.
            await ClientHelper.RegisterAndCreateContainerResourceIfNeeded(_httpClient, _httpXmlClient).ConfigureAwait(false);

            var url = _httpClient.BaseAddress + String.Format(CloudGameUriElements.CertificatesResourcePath);

            var certificate = new CertificateRequest()
            {
                Name = certificateName,
                Filename = certificateFileName,
                Password = certificatePassword
            };

            var multipartContent = new MultipartFormDataContent();
            {
                multipartContent.Add(new StringContent(ClientHelper.ToJson(certificate)), "metadata");
                multipartContent.Add(new StreamContent(certificateStream), "certificate");
                var message = await _httpClient.PostAsync(url, multipartContent).ConfigureAwait(false);
                return await ClientHelper.ProcessJsonResponse<CertificatePostResponse>(message).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Remove the game certificate.
        /// </summary>
        /// <param name="certificateId">The ID of the certificate to be removed.</param>
        /// <returns></returns>
        public async Task<bool> RemoveCertificate(Guid certificateId)
        {
            var url = _httpClient.BaseAddress + String.Format(CloudGameUriElements.CertificateResourcePath, certificateId);
            var message = await _httpClient.DeleteAsync(url).ConfigureAwait(false);
            if (message.IsSuccessStatusCode)
            {
                // Error result, so throw an exception
                throw new ServiceManagementClientException(
                    message.StatusCode,
                    new ServiceManagementError { Code = message.StatusCode.ToString() },
                    string.Empty);
            }

            return true;
        }

        /// <summary>
        /// Gets the asset packages.
        /// </summary>
        /// <param name="cloudGameId">An optional cloud game ID to filter by.</param>
        /// <returns></returns>
        public async Task<AssetCollectionResponse> GetAssets(Guid? cloudGameId)
        {
            var url = _httpClient.BaseAddress + (
                cloudGameId.HasValue ? String.Format(CloudGameUriElements.AssetsForGameResourcePath, cloudGameId.Value) :
                String.Format(CloudGameUriElements.AssetsResourcePath));
            var message = await _httpClient.GetAsync(url, Logger).ConfigureAwait(false);
            return await ClientHelper.ProcessJsonResponse<AssetCollectionResponse>(message).ConfigureAwait(false);
        }

        /// <summary>
        /// Creates a new asset package.
        /// </summary>
        /// <param name="assetName">The asset name.</param>
        /// <param name="assetFileName">The asset filename.</param>
        /// <param name="assetStream">The asset filestream.</param>
        /// <returns></returns>
        /// <exception cref="Microsoft.WindowsAzure.ServiceManagement.ServiceManagementClientException"></exception>
        /// <exception cref="ServiceManagementError"></exception>
        public async Task<string> NewAsset(string assetName, string assetFileName, Stream assetStream)
        {
            // Idempotent call to do a first time registration of the subscription-level wrapping container.
            await ClientHelper.RegisterAndCreateContainerResourceIfNeeded(_httpClient, _httpXmlClient).ConfigureAwait(false);

            // Call in to get an AssetID and preauthURL to use for upload of the asset
            var newGameAssetRequest = new AssetRequest()
            {
                Filename = assetFileName,
                Name = assetName
            };

            var multipartFormContent = new MultipartFormDataContent
            {
                {
                    new StringContent(ClientHelper.ToJson(newGameAssetRequest)), "metadata"
                }
            };

            var url = _httpClient.BaseAddress + String.Format(CloudGameUriElements.AssetsResourcePath);
            var responseMessage = await _httpClient.PostAsync(url, multipartFormContent).ConfigureAwait(false);
            var postAssetResult = await ClientHelper.ProcessJsonResponse<AssetPostResponse>(responseMessage).ConfigureAwait(false);

            try
            {
                var cloudblob = new CloudBlob(postAssetResult.AssetPreAuthUrl);
                await Task.Factory.FromAsync(
                    (callback, state) => cloudblob.BeginUploadFromStream(assetStream, callback, state),
                    cloudblob.EndUploadFromStream,
                    TaskCreationOptions.None).ConfigureAwait(false);
            }
            catch (StorageException)
            {
                var errorMessage = string.Format("Failed to upload asset file for CloudGame instance to azure storage. assetId {0}", postAssetResult.AssetId);
                throw ClientHelper.CreateExceptionFromJson(HttpStatusCode.Ambiguous, errorMessage);
            }

            var multpartFormContentMetadata = new MultipartFormDataContent
            {
                {
                    new StringContent(ClientHelper.ToJson(newGameAssetRequest)),"metadata"
                }
            };

            url = _httpClient.BaseAddress + String.Format(CloudGameUriElements.AssetResourcePath, postAssetResult.AssetId);
            responseMessage = await _httpClient.PutAsync(url, multpartFormContentMetadata).ConfigureAwait(false);

            if (!responseMessage.IsSuccessStatusCode)
            {
                // Error result, so throw an exception
                throw new ServiceManagementClientException(
                    responseMessage.StatusCode,
                    new ServiceManagementError { Code = responseMessage.StatusCode.ToString() },
                    string.Empty);
            }

            // Return the Asset info
            return postAssetResult.AssetId;
        }

        /// <summary>
        /// Creates a new game package.
        /// </summary>
        /// <param name="cloudGameName">The cloud game.</param>
        /// <param name="platform">The cloud game platform.</param>
        /// <param name="vmPackageId">The parent VM package Id.</param>
        /// <param name="name">The game package name.</param>
        /// <param name="fileName">The game package filename.</param>
        /// <param name="isActive">Whether the game package should be activated or not.</param>
        /// <param name="fileStream">The game package filestream.</param>
        /// <returns></returns>
        public async Task<string> NewGamePackage(
            string cloudGameName,
            CloudGamePlatform platform,
            Guid vmPackageId,
            string name,
            string fileName,
            bool isActive,
            Stream fileStream)
        {
            // Call in to get a game package ID and preauth URL to use for upload of the game package
            var newGamePackageRequest = new GamePackageRequest()
            {
                Filename = fileName,
                Name = name,
                Active = isActive
            };

            var multipartFormContent = new MultipartFormDataContent
            {
                {
                    new StringContent(ClientHelper.ToJson(newGamePackageRequest)),
                    "metadata"
                }
            };

            var platformResourceString = ClientHelper.GetPlatformString(platform);
            var url = _httpClient.BaseAddress + String.Format(CloudGameUriElements.GamePackagesResourcePath, platformResourceString, cloudGameName, vmPackageId);
            var responseMessage = await _httpClient.PostAsync(url, multipartFormContent).ConfigureAwait(false);
            var postGamePackageResult = await ClientHelper.ProcessJsonResponse<GamePackagePostResponse>(responseMessage).ConfigureAwait(false);

            try
            {
                var cloudblob = new CloudBlob(postGamePackageResult.GamePackagePreAuthUrl);
                await Task.Factory.FromAsync(
                    (callback, state) => cloudblob.BeginUploadFromStream(fileStream, callback, state),
                    cloudblob.EndUploadFromStream,
                    TaskCreationOptions.None).ConfigureAwait(false);
            }
            catch (StorageException)
            {
                var errorMessage = string.Format("Failed to upload game package file for cloud game instance to azure storage. gameId {0}, platform {1}, vmPackageId, {2} GamePackageId {3}", cloudGameName, platformResourceString, vmPackageId, postGamePackageResult.GamePackageId);
                throw ClientHelper.CreateExceptionFromJson(HttpStatusCode.Ambiguous, errorMessage);
            }

            var multpartFormContentMetadata = new MultipartFormDataContent
            {
                {
                    new StringContent(ClientHelper.ToJson(newGamePackageRequest)), "metadata"
                }
            };

            url = _httpClient.BaseAddress + String.Format(CloudGameUriElements.GamePackageResourcePath, platformResourceString, cloudGameName, vmPackageId, postGamePackageResult.GamePackageId);
            responseMessage = await _httpClient.PutAsync(url, multpartFormContentMetadata).ConfigureAwait(false);
            if (!responseMessage.IsSuccessStatusCode)
            {
                throw new ServiceManagementClientException(
                    responseMessage.StatusCode,
                    new ServiceManagementError { Code = responseMessage.StatusCode.ToString() },
                    string.Empty);
            }

            // Return the CodeFile info
            return postGamePackageResult.GamePackageId;
        }

        /// <summary>
        /// Sets values on the game package that need to change.
        /// </summary>
        /// <param name="cloudGameName">The cloud game.</param>
        /// <param name="platform">The cloud game platform.</param>
        /// <param name="vmPackageId">The parent VM package Id.</param>
        /// <param name="gamePackageId">The Id of the game package to change.</param>
        /// <param name="name">The game package name.</param>
        /// <param name="fileName">The game package filename.</param>
        /// <param name="isActive">Whether the game package should be activated or not.</param>
        /// <returns></returns>
        public async Task<bool> SetGamePackage(
            string cloudGameName,
            CloudGamePlatform platform,
            Guid vmPackageId,
            Guid gamePackageId,
            string name,
            string fileName,
            bool isActive)
        {
            // Create the new game package metadata
            var gamePackageRequest = new GamePackageRequest()
            {
                Name = name,
                Filename = fileName,
                Active = isActive
            };

            var multpartFormContentMetadata = new MultipartFormDataContent
            {
                {
                    new StringContent(ClientHelper.ToJson(gamePackageRequest)),
                    "metadata"
                }
            };

            var url = _httpClient.BaseAddress + String.Format(CloudGameUriElements.GamePackageResourcePath, ClientHelper.GetPlatformString(platform), cloudGameName, vmPackageId, gamePackageId);
            var message = await _httpClient.PutAsync(url, multpartFormContentMetadata).ConfigureAwait(false);
            if (!message.IsSuccessStatusCode)
            {
                // Error result, so throw an exception
                throw new ServiceManagementClientException(
                    message.StatusCode,
                    new ServiceManagementError { Code = message.StatusCode.ToString() },
                    string.Empty);

            }

            return true;
        }

        /// <summary>
        /// Gets the game packages.
        /// </summary>
        /// <param name="cloudGameName">The cloud game name.</param>
        /// <param name="platform">The cloud game platform.</param>
        /// <param name="vmPackageId">The parent VM package identifier.</param>
        /// <returns>
        /// Collection of game packages
        /// </returns>
        public async Task<GamePackageCollectionResponse> GetGamePackages(string cloudGameName, CloudGamePlatform platform, Guid vmPackageId)
        {
            var url = _httpClient.BaseAddress + String.Format(CloudGameUriElements.GamePackagesResourcePath, ClientHelper.GetPlatformString(platform), cloudGameName, vmPackageId);
            var message = await _httpClient.GetAsync(url, Logger).ConfigureAwait(false);
            return await ClientHelper.ProcessJsonResponse<GamePackageCollectionResponse>(message).ConfigureAwait(false);
        }

        /// <summary>
        /// Remove the game package.
        /// </summary>
        /// <param name="cloudGameName">The cloud game.</param>
        /// <param name="platform">The cloud game platform.</param>
        /// <param name="vmPackageId">The parent VM package Id.</param>
        /// <param name="gamePackageId">The ID of the game package to remove.</param>
        /// <returns>
        /// True if removed, false if not removed.
        /// </returns>
        public async Task<bool> RemoveGamePackage(string cloudGameName, CloudGamePlatform platform, Guid vmPackageId, Guid gamePackageId)
        {
            string url = _httpClient.BaseAddress + String.Format(CloudGameUriElements.GamePackageResourcePath, ClientHelper.GetPlatformString(platform), cloudGameName, vmPackageId, gamePackageId);
            var message = await _httpClient.DeleteAsync(url).ConfigureAwait(false);
            if (!message.IsSuccessStatusCode)
            {
                // Error result, so throw an exception
                throw new ServiceManagementClientException(
                    message.StatusCode,
                    new ServiceManagementError { Code = message.StatusCode.ToString() },
                    string.Empty);
            }

            return true;
        }

        /// <summary>
        /// Remove the asset package.
        /// </summary>
        /// <param name="assetId">The ID of the asset to be removed.</param>
        /// <returns></returns>
        /// <exception cref="Microsoft.WindowsAzure.ServiceManagement.ServiceManagementClientException"></exception>
        /// <exception cref="ServiceManagementError"></exception>
        public async Task<bool> RemoveAsset(Guid assetId)
        {
            string url = _httpClient.BaseAddress + String.Format(CloudGameUriElements.AssetResourcePath, assetId);
            var message = await _httpClient.DeleteAsync(url).ConfigureAwait(false);
            if (!message.IsSuccessStatusCode)
            {
                // Error result, so throw an exception
                throw new ServiceManagementClientException(
                    message.StatusCode,
                    new ServiceManagementError { Code = message.StatusCode.ToString() },
                    string.Empty);
            }

            return true;
        }

        /// <summary>
        /// Gets the compute summary report.
        /// </summary>
        /// <param name="cloudGameName">The cloud game name.</param>
        /// <param name="platform">The cloud game platform.</param>
        /// <returns></returns>
        public async Task<DashboardSummary> GetComputeSummaryReport(string cloudGameName, CloudGamePlatform platform)
        {
            var url = _httpClient.BaseAddress + String.Format(CloudGameUriElements.DashboardSummaryPath, ClientHelper.GetPlatformString(platform), cloudGameName);
            var message = await _httpClient.GetAsync(url, Logger).ConfigureAwait(false);
            return await ClientHelper.ProcessJsonResponse<DashboardSummary>(message).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets the deployments report.
        /// </summary>
        /// <param name="cloudGameName">The cloud game name.</param>
        /// <param name="platform">The cloud game platform.</param>
        /// <returns></returns>
        public async Task<DeploymentData> GetComputeDeploymentsReport(string cloudGameName, CloudGamePlatform platform)
        {
            var url = _httpClient.BaseAddress + String.Format(CloudGameUriElements.DeploymentsReportPath, ClientHelper.GetPlatformString(platform), cloudGameName);
            var message = await _httpClient.GetAsync(url, Logger).ConfigureAwait(false);
            return await ClientHelper.ProcessJsonResponse<DeploymentData>(message).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets the service pools report.
        /// </summary>
        /// <param name="cloudGameName">The cloud game name.</param>
        /// <param name="platform">The cloud game platform.</param>
        /// <returns></returns>
        public async Task<PoolData> GetComputePoolsReport(string cloudGameName, CloudGamePlatform platform)
        {
            var url = _httpClient.BaseAddress + String.Format(CloudGameUriElements.ServicepoolsReportPath, ClientHelper.GetPlatformString(platform), cloudGameName);
            var message = await _httpClient.GetAsync(url, Logger).ConfigureAwait(false);
            return await ClientHelper.ProcessJsonResponse<PoolData>(message).ConfigureAwait(false);
        }

        /// <summary>
        /// Creates a new cloud game resource.
        /// </summary>
        /// <param name="platform">The cloud game platform.</param>
        /// <param name="titleId">The title ID within the subscription to use (in Decimal form)</param>
        /// <param name="selectionOrder">The selection order to use</param>
        /// <param name="sandboxes">A comma seperated list of sandbox names</param>
        /// <param name="resourceSetIds">A comma seperated list of resource set IDs</param>
        /// <param name="name">The name of the Cloud Game</param>
        /// <param name="schemaId">The Id of an existing variant schema</param>
        /// <param name="schemaName">The name of the game mode schema to use if a schema Id is not specified.</param>
        /// <param name="schemaFileName">The local schema file name (only used for reference) if a schema Id is not specified.</param>
        /// <param name="schemaStream">The schema data as a file stream, used if a schema Id is not specified.</param>
        /// <returns>
        /// The cloud task for completion
        /// </returns>
        public async Task<bool> NewCloudGame(
            CloudGamePlatform platform,
            string titleId,
            int selectionOrder,
            string sandboxes,
            string resourceSetIds,
            string name,
            Guid? schemaId,
            string schemaName,
            string schemaFileName,
            Stream schemaStream)
        {
            var platformResourceString = ClientHelper.GetPlatformString(platform);

            // Idempotent call to do a first time registration of the cloud service wrapping container.
            await ClientHelper.RegisterCloudService(_httpClient, _httpXmlClient, platformResourceString).ConfigureAwait(false);

            GameModeSchemaRequest gameModeSchemaRequestData = null;
            if (!schemaId.HasValue)
            {
                // Schema ID not provided, so must have schemaName, etc.
                if (String.IsNullOrEmpty(schemaName) || String.IsNullOrEmpty(schemaFileName) || schemaStream == null)
                {
                    throw new ServiceManagementClientException(HttpStatusCode.BadRequest,
                        new ServiceManagementError { Code = HttpStatusCode.BadRequest.ToString() },
                        "Invalid Game Mode Schema values provided.");
                }

                string schemaContent;
                using (var streamReader = new StreamReader(schemaStream))
                {
                    schemaContent = streamReader.ReadToEnd();
                }

                gameModeSchemaRequestData = new GameModeSchemaRequest()
                {
                    Metadata = new GameModeSchema()
                    {
                        Name = schemaName,
                        Filename = schemaFileName,
                        TitleId = titleId
                    },
                    Content = schemaContent
                };
            }

            var cloudGame = new CloudGame()
            {
                Name = name,
                ResourceSets = resourceSetIds,
                Sandboxes = sandboxes,
                SchemaName = schemaName,
                TitleId = titleId,
                SelectionOrder = selectionOrder
            };

            var putGameRequest = new CloudGameRequest()
            {
                CloudGame = cloudGame
            };

            // If a schemaID is provided, use that in the request, otherwise, add the schema data contract to the put request
            if (schemaId.HasValue)
            {
                cloudGame.SchemaId = schemaId.Value.ToString();
            }
            else
            {
                putGameRequest.GameModeSchema = gameModeSchemaRequestData;
            }

            var doc = new XmlDocument();
            var resource = new Resource()
            {
                Name = name,
                ETag = Guid.NewGuid().ToString(),       // BUGBUG What should this ETag value be?
                Plan = string.Empty,
                ResourceProviderNamespace = CloudGameUriElements.NamespaceName,
                Type = platformResourceString,
                SchemaVersion = CloudGameUriElements.SchemaVersion,
                IntrinsicSettings = new XmlNode[]
                {
                    doc.CreateCDataSection(ClientHelper.ToJson(putGameRequest))
                }
            };

            var url = _httpClient.BaseAddress + String.Format(CloudGameUriElements.CloudGameResourcePath, platformResourceString, name);
            var message = await _httpClient.PutAsXmlAsync(url, resource).ConfigureAwait(false);
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

            // Poll RDFE to see if the CloudGame instance has been created
            var created = false;
            var numRetries = 0;
            do
            {
                var xblComputeInstances = await GetCloudGames().ConfigureAwait(false);
                if (xblComputeInstances.Any(cloudGameInstance => cloudGameInstance.Name == name))
                {
                    created = true;
                }

                Thread.Sleep(1000);
            }
            while (!created && (numRetries++ < 10));

            return created;
        }

        /// <summary>
        /// Removes a cloud game instance.
        /// </summary>
        /// <param name="cloudGameName">The cloud game name.</param>
        /// <param name="platform">The cloud game platform.</param>
        /// <returns></returns>
        public async Task<bool> RemoveCloudGame(string cloudGameName, CloudGamePlatform platform)
        {
            var url = _httpClient.BaseAddress + String.Format(CloudGameUriElements.CloudGameResourcePath, ClientHelper.GetPlatformString(platform), cloudGameName);
            var message = await _httpClient.DeleteAsync(url).ConfigureAwait(false);
            if (!message.IsSuccessStatusCode)
            {
                // Error result, so throw an exception
                throw new ServiceManagementClientException(
                    message.StatusCode,
                    new ServiceManagementError { Code = message.StatusCode.ToString() },
                    string.Empty);
            }

            return true;
        }

        /// <summary>
        /// Gets the cloud game instances for the Azure Game Services resource in the current subscription
        /// </summary>
        /// <returns></returns>
        public async Task<CloudGameColletion> GetCloudGames()
        {
            var url = _httpClient.BaseAddress + String.Format(CloudGameUriElements.GetCloudServicesResourcePath);
            var message = await _httpXmlClient.GetAsync(url, Logger).ConfigureAwait(false);
            return await ClientHelper.ProcessCloudServiceResponse(message).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets the AzureGameServicesProperties for the current subscription
        /// </summary>
        /// <returns>
        /// The task for completion.
        /// </returns>
        public async Task<AzureGameServicesPropertiesResponse> GetAzureGameServicesProperties()
        {
            var url = _httpClient.BaseAddress + String.Format(CloudGameUriElements.ResourcePropertiesPath);

            var message = await _httpXmlClient.GetAsync(url, Logger).ConfigureAwait(false);
            var propertyList = await ClientHelper.ProcessXmlResponse<ResourceProviderProperties>(message).ConfigureAwait(false);

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

            var response = ClientHelper.DeserializeJsonToObject<AzureGameServicesPropertiesResponse>(property.Value);
            response.Platform = response.Platform.ToLower();
            return response;
        }

        /// <summary>
        /// Publishes the cloud game.
        /// </summary>
        /// <param name="cloudGameName">The cloud game name.</param>
        /// <param name="platform">The cloud game platform.</param>
        /// <param name="sandboxes">Optional, string delimitted list of sandboxes to deploy to</param>
        /// <param name="geoRegions">Optional, string delimitted list of geo regions to deploy to</param>
        /// <returns>
        /// The task for completion.
        /// </returns>
        public async Task<bool> DeployCloudGame(string cloudGameName, CloudGamePlatform platform, string sandboxes, string geoRegions)
        {
            var url = _httpClient.BaseAddress + String.Format(CloudGameUriElements.DeployCloudGamePath, ClientHelper.GetPlatformString(platform), cloudGameName, sandboxes, geoRegions);
            var message = await _httpClient.PutAsync(url, null).ConfigureAwait(false);
            if (!message.IsSuccessStatusCode)
            {
                // Error result, so throw an exception
                throw new ServiceManagementClientException(
                    message.StatusCode,
                    new ServiceManagementError { Code = message.StatusCode.ToString() },
                    string.Empty);
            }

            return true;
        }

        /// <summary>
        /// Stops the cloud game.
        /// </summary>
        /// <param name="cloudGameName">The cloud game name.</param>
        /// <param name="platform">The cloud game platform.</param>
        /// <returns></returns>
        public async Task<bool> StopCloudGame(string cloudGameName, CloudGamePlatform platform)
        {
            var url = _httpClient.BaseAddress + String.Format(CloudGameUriElements.StopCloudGamePath, ClientHelper.GetPlatformString(platform), cloudGameName);
            var message = await _httpClient.PutAsync(url, null).ConfigureAwait(false);
            if (!message.IsSuccessStatusCode)
            {
                // Error result, so throw an exception
                throw new ServiceManagementClientException(
                    message.StatusCode,
                    new ServiceManagementError { Code = message.StatusCode.ToString() },
                    string.Empty);
            }

            return true;
        }

        /// <summary>
        /// Gets the list of available diagnostic log files for the specific instance.
        /// </summary>
        /// <param name="cloudGameName">The cloud game name.</param>
        /// <param name="platform">The cloud game platform.</param>
        /// <param name="instanceId">The Id of the instance to get log files for</param>
        /// <returns>
        /// A list of URIs to download individual log files
        /// </returns>
        public async Task<EnumerateDiagnosticFilesResponse> GetLogFiles(string cloudGameName, CloudGamePlatform platform, string instanceId)
        {
            var url = _httpClient.BaseAddress + String.Format(CloudGameUriElements.LogFilePath, ClientHelper.GetPlatformString(platform), cloudGameName, instanceId);
            var message = await _httpClient.GetAsync(url, Logger).ConfigureAwait(false);
            return await ClientHelper.ProcessJsonResponse<EnumerateDiagnosticFilesResponse>(message).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets the list of available diagnostic dump files for the specific instance.
        /// </summary>
        /// <param name="cloudGameName">The cloud game name.</param>
        /// <param name="platform">The cloud game platform.</param>
        /// <param name="instanceId">The Id of the instance to get dump files for</param>
        /// <returns>
        /// A list of URIs to download individual dump files
        /// </returns>
        public async Task<EnumerateDiagnosticFilesResponse> GetDumpFiles(string cloudGameName, CloudGamePlatform platform, string instanceId)
        {
            var url = _httpClient.BaseAddress + String.Format(CloudGameUriElements.DumpFilePath, ClientHelper.GetPlatformString(platform), cloudGameName, instanceId);
            var message = await _httpClient.GetAsync(url, Logger).ConfigureAwait(false);
            return await ClientHelper.ProcessJsonResponse<EnumerateDiagnosticFilesResponse>(message).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets the list of clusters.
        /// </summary>
        /// <param name="cloudGameName">The cloud game name.</param>
        /// <param name="platform">The cloud game platform.</param>
        /// <param name="geoRegion">The regiond to enumerate clusters from</param>
        /// <param name="status">The status to filter on</param>
        /// <returns>
        /// A list of clusters that match the region and status filter
        /// </returns>
        public async Task<EnumerateClustersResponse> GetClusters(string cloudGameName, CloudGamePlatform platform, string geoRegion, string status)
        {
            var url = _httpClient.BaseAddress + String.Format(CloudGameUriElements.EnumerateClustersPath, ClientHelper.GetPlatformString(platform), cloudGameName, geoRegion, status);
            var message = await _httpClient.GetAsync(url, Logger).ConfigureAwait(false);
            return await ClientHelper.ProcessJsonResponse<EnumerateClustersResponse>(message).ConfigureAwait(false);
        }
    }
}
