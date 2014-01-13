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
    /// <summary>
    ///     Contains URI fragments and namespaces used by Azure Media Services cmdlets
    /// </summary>
    public class CloudGameUriElements
    {
        /// <summary>
        /// The application json media type
        /// </summary>
        public const string ApplicationJsonMediaType = "application/json";

        /// <summary>
        /// The application json media type
        /// </summary>
        public const string ApplicationXmlMediaType = "application/xml";

        /// <summary>
        /// The XBL correlation header
        /// </summary>
        public const string XblCorrelationHeader = "X-XblCorrelationId";

        /// <summary>
        /// The create image resource path.
        /// </summary>
        public const string GameImagesResourcePath = "/cloudservices/gameservices/resources/gameservices/~/xboxlivecompute/{0}/images";

        /// <summary>
        /// The create assets resource path.
        /// </summary>
        public const string GameAssetsResourcePath = "/cloudservices/gameservices/resources/gameservices/~/xboxlivecompute/{0}/assets";

        /// <summary>
        /// The game asset resource path.
        /// </summary>
        public const string GameAssetResourcePath = "/cloudservices/gameservices/resources/gameservices/~/xboxlivecompute/{0}/assets/{1}";

        /// <summary>
        /// The game image resource path.
        /// </summary>
        public const string GameImageResourcePath = "/cloudservices/gameservices/resources/gameservices/~/xboxlivecompute/{0}/images/{1}";

        /// <summary>
        /// The create game mode resource path.
        /// </summary>
        public const string GameModesResourcePath = "/cloudservices/gameservices/resources/gameservices/~/xboxlivecompute/{0}/variants";

        /// <summary>
        /// The game mode resource path.
        /// </summary>
        public const string GameModeResourcePath = "/cloudservices/gameservices/resources/gameservices/~/xboxlivecompute/{0}/variants/{1}";

        /// <summary>
        /// The create game certificates resource path.
        /// </summary>
        public const string CertificatesResourcePath = "/cloudservices/gameservices/resources/gameservices/~/xboxlivecompute/{0}/certificates";

        /// <summary>
        /// The create game certificate resource path.
        /// </summary>
        public const string CertificateResourcePath = "/cloudservices/gameservices/resources/gameservices/~/xboxlivecompute/{0}/certificates/{1}";

        /// <summary>
        /// The delete resource path.
        /// </summary>
        private const string DeleteResourcePath = "/cloudservices/gameservices/resources/gameservices/~/xboxlivecompute/{0}/{1}/{2}";

        /// <summary>
        /// The deploy cloud game path.
        /// </summary>
        public const string DeployCloudGamePath = "/cloudservices/gameservices/resources/gameservices/~/xboxlivecompute/{0}?operation=publish&sandboxes={1}&geoRegion={2}";

        /// <summary>
        /// The stop cloud game path.
        /// </summary>
        public const string StopCloudGamePath = "/cloudservices/gameservices/resources/gameservices/~/xboxlivecompute/{0}?operation=unpublish";

        /// <summary>
        /// The configure cloud game path.
        /// </summary>
        private const string ConfigureGamePath = "/cloudservices/gameservices/resources/gameservices/~/xboxlivecompute/{0}?operation=configure";

        /// <summary>
        /// The resource properties path.
        /// </summary>
        public const string ResourcePropertiesPath = "/resourceproviders/gameservices/Properties?resourceType=xboxlivecompute";

        /// <summary>
        /// The dashboard summary for cloud game path.
        /// </summary>
        public const string DashboardSummaryPath = "/cloudservices/gameservices/resources/gameservices/~/xboxlivecompute/{0}/monitoring?Details=DashboardSummary";

        /// <summary>
        /// The deployments report path.
        /// </summary>
        public const string DeploymentsReportPath = "/cloudservices/gameservices/resources/gameservices/~/xboxlivecompute/{0}/poolunits/reports/deployments";

        /// <summary>
        /// The servicepools report path.
        /// </summary>
        public const string ServicepoolsReportPath = "/cloudservices/gameservices/resources/gameservices/~/xboxlivecompute/{0}/poolunits/reports/servicepools";

        /// <summary>
        /// The Log File path.
        /// </summary>
        public const string LogFilePath = "/cloudservices/gameservices/resources/gameservices/~/xboxlivecompute/{0}/diagnostics/logs/{1}";

        /// <summary>
        /// The Dump File path.
        /// </summary>
        public const string DumpFilePath = "/cloudservices/gameservices/resources/gameservices/~/xboxlivecompute/{0}/diagnostics/dumps/{1}";

        /// <summary>
        /// The Enumerate Clusters path.
        /// </summary>
        public const string EnumerateClustersPath = "/cloudservices/gameservices/resources/gameservices/~/xboxlivecompute/{0}/clusters/?geoRegion={1}&status={2}";

        /// <summary>
        /// The put cloud service resource path.
        /// </summary>
        public const string CloudServiceResourcePath = "/cloudservices/gameservices";

        /// <summary>
        /// The default service name
        /// </summary>
        public const string DefaultServiceName = "gameservices";

        /// <summary>
        /// The namespace name
        /// </summary>
        public const string NamespaceName = "gameservices";

        /// <summary>
        /// The xbox live compute resource type
        /// </summary>
        public const string XboxLiveComputeResourceType = "xboxlivecompute";

        /// <summary>
        /// The schema version
        /// </summary>
        public const string SchemaVersion = "1.0";

        /// <summary>
        /// The cloud games resource path
        /// </summary>
        public const string CloudGameResourcePath = "/cloudservices/gameservices/resources/gameservices/xboxlivecompute/{0}";

        /// <summary>
        /// The get cloud services resource path.
        /// </summary>
        public const string GetCloudServicesResourcePath = "/cloudservices/gameservices?detailLevel=full";
    }
}