﻿// ----------------------------------------------------------------------------------
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

using System.Collections.Generic;

namespace Microsoft.WindowsAzure.Commands.GameServices.Model
{
    using System;
    using System.Collections;
    using Contract;
    using System.IO;
    using System.Threading.Tasks;
    using Microsoft.WindowsAzure.Commands.GameServices.Model.Common;

    /// <summary>
    ///     Defines interface to communicate with Azure Game Services REST API
    /// </summary>
    public interface ICloudGameClient
    {
        /// <summary>
        /// Gets general information about the cloud game client.
        /// </summary>
        /// <returns></returns>
        GameServicesCmdletsInfo Info { get; }

        /// <summary>
        /// Gets the VM packages.
        /// </summary>
        /// <param name="cloudGameName">The cloud game name.</param>
        /// <param name="platform">The cloud game platform.</param>
        /// <returns></returns>
        Task<VmPackageCollectionResponse> GetVmPackages(string cloudGameName, CloudGamePlatform platform);

        /// <summary>
        /// Upload VM package components to a cloud game.
        /// </summary>
        /// <param name="cloudGameName">The cloud game name.</param>
        /// <param name="platform">The cloud game platform.</param>
        /// <param name="packageName">The name of the package.</param>
        /// <param name="maxPlayers">The max number of players allowed.</param>
        /// <param name="assetId">The id of a previously uploaded asset file.</param>
        /// <param name="certificateIds">The ids of certificates to associate.</param>
        /// <param name="cspkgFileName">The name of the local cspkg file name.</param>
        /// <param name="cspkgStream">The cspkg file stream.</param>
        /// <param name="cscfgFileName">The name of the local cscfg file name.</param>
        /// <param name="cscfgStream">The game cscfg file stream.</param>
        /// <returns></returns>
        Task<VmPackagePostResponse> NewVmPackage(
            string cloudGameName,
            CloudGamePlatform platform,
            string packageName,
            int maxPlayers,
            Guid? assetId,
            Guid[] certificateIds,
            string cspkgFileName,
            Stream cspkgStream,
            string cscfgFileName,
            Stream cscfgStream);

        /// <summary>
        ///     Remove the VM package.
        /// </summary>
        /// <param name="cloudGameName">The cloud game name.</param>
        /// <param name="platform">The cloud game platform.</param>
        /// <param name="vmPackageId">The VM package Id.</param>
        /// <returns></returns>
        Task<bool> RemoveVmPackage(string cloudGameName, CloudGamePlatform platform, Guid vmPackageId);

        /// <summary>
        ///     Gets the game modes associated with a parent schema.
        /// </summary>
        /// <param name="gameModeSchemaId">The game mode schema Id.</param>
        /// <returns></returns>
        Task<GameModeCollectionResponse> GetGameModes(Guid gameModeSchemaId);

        /// <summary>
        ///     Creates a Game Mode Schema.
        /// </summary>
        /// <param name="schemaName">The game mode schema name.</param>
        /// <param name="fileName">The game mode schema original filename.</param>
        /// <param name="schemaStream">The game mode schema stream.</param>
        /// <returns></returns>
        Task<NewGameModeSchemaResponse> NewGameModeSchema(
            string schemaName,
            string fileName,
            Stream schemaStream);

        /// <summary>
        ///     Remove the game mode schema.
        /// </summary>
        /// <param name="gameModeSchemaId">The game mode schema ID.</param>
        /// <returns></returns>
        Task<bool> RemoveGameModeSchema(Guid gameModeSchemaId);

        /// <summary>
        ///     Gets the game mode schemas.
        /// </summary>
        /// <param name="getDetails">If the list of child game modes should be included or not.</param>
        /// <returns></returns>
        Task<GameModeSchemaCollectionResponse> GetGameModeSchemas(bool getDetails = false);

        /// <summary>
        ///     Creates a Game Mode.
        /// </summary>
        /// <param name="gameModeSchemaId">The parent game mode schema identifier.</param>
        /// <param name="gameModeName">The game mode name.</param>
        /// <param name="gameModeFileName">The game mode original filename.</param>
        /// <param name="gameModeStream">The game mode stream.</param>
        /// <returns></returns>
        Task<NewGameModeResponse> NewGameMode(
            Guid gameModeSchemaId,
            string gameModeName,
            string gameModeFileName,
            Stream gameModeStream);

        /// <summary>
        ///     Remove the game mode.
        /// </summary>
        /// <param name="gameModeSchemaId">The game mode schema ID.</param>
        /// <param name="gameModeId">The game mode ID.</param>
        /// <returns></returns>
        Task<bool> RemoveGameMode(Guid gameModeSchemaId, Guid gameModeId);

        /// <summary>
        ///     Gets the certificates.
        /// </summary>
        /// <param name="cloudGameId">An optional cloud game ID to filter by.</param>
        /// <returns></returns>
        Task<CertificateCollectionResponse> GetCertificates(Guid? cloudGameId);

        /// <summary>
        ///     Creates a certificate.
        /// </summary>
        /// <param name="certificateName">The certificate name.</param>
        /// <param name="certificateFileName">The certificate filename.</param>
        /// <param name="certificatePassword">The certificate password.</param>
        /// <param name="certificateStream">The certificate stream.</param>
        /// <returns></returns>
        Task<CertificatePostResponse> NewCertificate(
            string certificateName,
            string certificateFileName,
            string certificatePassword,
            Stream certificateStream);

        /// <summary>
        ///     Remove the game certificate.
        /// </summary>
        /// <param name="certificateId">The ID of the certificate to be removed.</param>
        /// <returns></returns>
        Task<bool> RemoveCertificate(Guid certificateId);

        /// <summary>
        ///     Gets the asset packages.
        /// </summary>
        /// <param name="cloudGameId">An optional cloud game ID to filter by.</param>
        /// <returns></returns>
        Task<AssetCollectionResponse> GetAssets(Guid? cloudGameId);

        /// <summary>
        ///     Creates a new asset package.
        /// </summary>
        /// <param name="assetName">The asset name.</param>
        /// <param name="assetFileName">The asset filename.</param>
        /// <param name="assetStream">The asset filestream.</param>
        /// <returns></returns>
        Task<AssetPostResponse> NewAsset(
            string assetName,
            string assetFileName,
            Stream assetStream);

        /// <summary>
        ///     Creates a new game package.
        /// </summary>
        /// <param name="cloudGameName">The cloud game.</param>
        /// <param name="platform">The cloud game platform.</param>
        /// <param name="vmPackageId">The parent VM package Id.</param>
        /// <param name="name">The game package name.</param>
        /// <param name="fileName">The game package filename.</param>
        /// <param name="isActive">Whether the game package should be activated or not.</param>
        /// <param name="fileStream">The game package filestream.</param>
        /// <returns></returns>
        Task<GamePackagePostResponse> NewGamePackage(
            string cloudGameName,
            CloudGamePlatform platform,
            Guid vmPackageId,
            string name,
            string fileName,
            bool isActive,
            Stream fileStream);

        /// <summary>
        ///     Sets values on the game package that need to change.
        /// </summary>
        /// <param name="cloudGameName">The cloud game.</param>
        /// <param name="platform">The cloud game platform.</param>
        /// <param name="vmPackageId">The parent VM package Id.</param>
        /// <param name="gamePackageId">The Id of the game package to change.</param>
        /// <param name="name">The game package name.</param>
        /// <param name="fileName">The game package filename.</param>
        /// <param name="isActive">Whether the game package should be activated or not.</param>
        /// <returns></returns>
        Task<bool> SetGamePackage(
            string cloudGameName,
            CloudGamePlatform platform,
            Guid vmPackageId,
            Guid gamePackageId,
            string name,
            string fileName,
            bool isActive);

        /// <summary>
        ///     Gets the game packages.
        /// </summary>
        /// <param name="cloudGameName">The cloud game name.</param>
        /// <param name="platform">The cloud game platform.</param>
        /// <param name="vmPackageId">The parent VM package identifier.</param>
        /// <returns>
        /// Collection of game packages
        /// </returns>
        Task<GamePackageCollectionResponse> GetGamePackages(string cloudGameName, CloudGamePlatform platform, Guid vmPackageId);

        /// <summary>
        ///     Remove the game package.
        /// </summary>
        /// <param name="cloudGameName">The cloud game.</param>
        /// <param name="platform">The cloud game platform.</param>
        /// <param name="vmPackageId">The parent VM package Id.</param>
        /// <param name="gamePackageId">The ID of the game package to remove.</param>
        /// <returns>True if removed, false if not removed.</returns>
        Task<bool> RemoveGamePackage(
            string cloudGameName,
            CloudGamePlatform platform,
            Guid vmPackageId,
            Guid gamePackageId);

        /// <summary>
        ///     Remove the asset package.
        /// </summary>
        /// <param name="assetId">The ID of the asset to be removed.</param>
        /// <returns></returns>
        Task<bool> RemoveAsset(Guid assetId);

        /// <summary>
        ///     Gets the compute summary report.
        /// </summary>
        /// <param name="cloudGameName">The cloud game name.</param>
        /// <param name="platform">The cloud game platform.</param>
        /// <returns></returns>
        Task<DashboardSummary> GetComputeSummaryReport(string cloudGameName, CloudGamePlatform platform);

        /// <summary>
        ///     Gets the deployments report.
        /// </summary>
        /// <param name="cloudGameName">The cloud game name.</param>
        /// <param name="platform">The cloud game platform.</param>
        /// <returns></returns>
        Task<DeploymentData> GetComputeDeploymentsReport(string cloudGameName, CloudGamePlatform platform);

        /// <summary>
        ///     Gets the service pools report.
        /// </summary>
        /// <param name="cloudGameName">The cloud game name.</param>
        /// <param name="platform">The cloud game platform.</param>
        /// <returns></returns>
        Task<PoolData> GetComputePoolsReport(string cloudGameName, CloudGamePlatform platform);

        /// <summary>
        ///     Creates a new cloud game resource.
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
        /// <param name="tags">The tags for the cloud game.</param>
        /// <returns>
        /// The cloud task for completion
        /// </returns>
        Task<bool> NewCloudGame(
            CloudGamePlatform platform,
            string titleId,
            int selectionOrder,
            string[] sandboxes,
            string[] resourceSetIds,
            string name,
            Guid? schemaId,
            string schemaName,
            string schemaFileName,
            Stream schemaStream,
            Hashtable tags);

        /// <summary>
        /// Removes a cloud game instance.
        /// </summary>
        /// <param name="cloudGameName">The cloud game name.</param>
        /// <param name="platform">The cloud game platform.</param>
        /// <param name="checkStateFirst">if set to <c>true</c> check state first.</param>
        /// <returns></returns>
        Task<bool> RemoveCloudGame(string cloudGameName, CloudGamePlatform platform, bool checkStateFirst = true);

        /// <summary>
        ///     Gets the cloud game instances for the Azure Game Services resource in the current subscription
        /// </summary>
        /// <returns></returns>
        Task<CloudGameColletion> GetCloudGames();

        /// <summary>
        ///     Gets a cloud game.
        /// </summary>
        /// <param name="cloudGameName">Name of the cloud game.</param>
        /// <param name="platform">The cloud game platform.</param>
        /// <returns></returns>
        Task<CloudGame> GetCloudGame(string cloudGameName, CloudGamePlatform platform);

        /// <summary>
        ///     Gets the AzureGameServicesProperties for the current subscription
        /// </summary>
        /// <returns>The task for completion.</returns>
        Task<AzureGameServicesPropertiesResponse> GetAzureGameServicesProperties();

        /// <summary>
        /// Publishes the cloud game.
        /// </summary>
        /// <param name="cloudGameName">The cloud game name.</param>
        /// <param name="platform">The cloud game platform.</param>
        /// <param name="sandboxes">Optional, comma-delimitted list of sandboxes to deploy to.</param>
        /// <param name="geoRegions">Optional, comma-delimitted list of geo regions to deploy to.</param>
        /// <param name="publishOnly">if set to <c>true</c> publish only, and do not deploy.</param>
        /// <returns>
        /// The task for completion.
        /// </returns>
        Task<bool> DeployCloudGame(string cloudGameName, CloudGamePlatform platform, string[] sandboxes, string[] geoRegions, bool publishOnly);

        /// <summary>
        /// Stops the cloud game.
        /// </summary>
        /// <param name="cloudGameName">The cloud game name.</param>
        /// <param name="platform">The cloud game platform.</param>
        /// <param name="unpublishOnly">if set to <c>true</c> unpublish only, and do not attempt to destroy pools.</param>
        /// <returns>
        /// The task for completion.
        /// </returns>
        Task<bool> StopCloudGame(string cloudGameName, CloudGamePlatform platform, bool unpublishOnly);

        /// <summary>
        ///     Gets the list of available diagnostic log files for the specific instance.
        /// </summary>
        /// <param name="cloudGameName">The cloud game name.</param>
        /// <param name="platform">The cloud game platform.</param>
        /// <param name="instanceId">The Id of the instance to get log files for.</param>
        /// <param name="geoRegion">The geo region of the instance (if known).</param>
        /// <returns>A list of URIs to download individual log files.</returns>
        Task<EnumerateDiagnosticFilesResponse> GetLogFiles(string cloudGameName, CloudGamePlatform platform, string instanceId, string geoRegion);

        /// <summary>
        ///     Gets the list of available diagnostic dump files for the specific instance.
        /// </summary>
        /// <param name="cloudGameName">The cloud game name.</param>
        /// <param name="platform">The cloud game platform.</param>
        /// <param name="instanceId">The Id of the instance to get dump files for.</param>
        /// <param name="geoRegion">The geo region of the instance (if known).</param>
        /// <returns>A list of URIs to download individual dump files.</returns>
        Task<EnumerateDiagnosticFilesResponse> GetDumpFiles(string cloudGameName, CloudGamePlatform platform, string instanceId, string geoRegion);

        /// <summary>
        ///     Gets the list of clusters.
        /// </summary>
        /// <param name="cloudGameName">The cloud game name.</param>
        /// <param name="platform">The cloud game platform.</param>
        /// <param name="geoRegion">The regiond to enumerate clusters from</param>
        /// <param name="status">The status to filter on</param>
        /// <param name="clusterId">The (optional) cluster ID to query for.</param>
        /// <param name="agentId">The (optional) agent ID to query for.</param>
        /// <returns>A list of clusters that match the region and status filter</returns>
        Task<EnumerateClustersResponse> GetClusters(string cloudGameName, CloudGamePlatform platform, string geoRegion, string status, string clusterId, string agentId);

        /// <summary>
        ///     Gets the monitoring counter names.
        /// </summary>
        /// <param name="cloudGameName">The cloud game name.</param>
        /// <param name="platform">The cloud game platform.</param>
        /// <returns>A list of monitoring counter names.</returns>
        Task<List<string>> GetComputeMonitoringCounters(string cloudGameName, CloudGamePlatform platform);


        /// <summary>
        ///     Gets the compute monitoring counter data.
        /// </summary>
        /// <param name="cloudGameName">The cloud game name.</param>
        /// <param name="platform">The cloud game platform.</param>
        /// <param name="geoRegionName">Name of the geo region.</param>
        /// <param name="startTime">The start time.</param>
        /// <param name="endTime">The end time.</param>
        /// <param name="timeZoom">The time zoom.</param>
        /// <param name="counterNames">The selected counters.</param>
        /// <returns>The counter data for the selected counters.</returns>
        Task<CounterChartDataResponse> GetComputeMonitoringCounterData(
            string cloudGameName,
            CloudGamePlatform platform,
            string geoRegionName,
            DateTime startTime,
            DateTime endTime,
            TimeSpan timeZoom,
            string[] counterNames);


        /// <summary>
        /// Configures the cloud game properties.
        /// </summary>
        /// <param name="cloudGameName">The cloud game name.</param>
        /// <param name="platform">The cloud game platform.</param>
        /// <param name="resourceSets">The resource set IDs.</param>
        /// <param name="sandboxes">The sandboxes.</param>
        /// <returns>The task for completion.</returns>
        Task<bool> ConfigureCloudGame(string cloudGameName, CloudGamePlatform platform, string[] resourceSets, string[] sandboxes);

        /// <summary>
        /// Repairs the session host.
        /// </summary>
        /// <param name="cloudGameName">Name of the cloud game.</param>
        /// <param name="platform">The platform.</param>
        /// <param name="sessionHostId">The session host identifier.</param>
        /// <returns></returns>
        Task<bool> RepairSessionHost(string cloudGameName, CloudGamePlatform platform, string sessionHostId);

        /// <summary>
        /// Creates a new Insights configuration item.
        /// </summary>
        /// <param name="targetName">Name of the target.</param>
        /// <param name="targetType">Type of the target.</param>
        /// <param name="connectionString">The connection string.</param>
        /// <returns></returns>
        Task<bool> NewInsightsConfigItem(string targetName, string targetType, string connectionString);

        /// <summary>
        /// Gets the insights configuration items.
        /// </summary>
        /// <returns></returns>
        Task<InsightsConfigItemsResponse> GetInsightsConfigItems();

        /// <summary>
        /// Sets the Insights configuration item.
        /// </summary>
        /// <param name="targetName">Name of the target.</param>
        /// <param name="targetType">Type of the target.</param>
        /// <param name="connectionString">The connection string.</param>
        /// <returns></returns>
        Task<bool> SetInsightsConfigItem(string targetName, string targetType, string connectionString);

        /// <summary>
        /// Removes the Insights configuration item.
        /// </summary>
        /// <param name="targetName">Name of the target.</param>
        /// <returns></returns>
        Task<bool> RemoveInsightsConfigItem(string targetName);
    }
}