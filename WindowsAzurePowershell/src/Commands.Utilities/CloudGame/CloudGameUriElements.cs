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

namespace Microsoft.WindowsAzure.Commands.Utilities.CloudGame
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
        /// The create VM package resource path.
        /// </summary>
        public const string VmPackagesResourcePath = "/cloudservices/gameservices/resources/gameservices/~/{0}/{1}/images";

        /// <summary>
        /// The assets resource path.
        /// </summary>
        public const string AssetsResourcePath = "/cloudservices/gameservices/resources/gameservices/~/" + ContainerResourceType + "/" + DefaultContainerName + "/assets";

        /// <summary>
        /// The assets resource path.
        /// </summary>
        public const string AssetsForGameResourcePath = "/cloudservices/gameservices/resources/gameservices/~/" + ContainerResourceType + "/" + DefaultContainerName + "/assets?gsiSetId={0}";

        /// <summary>
        /// The assets resource path.
        /// </summary>
        public const string AssetResourcePath = "/cloudservices/gameservices/resources/gameservices/~/" + ContainerResourceType + "/" + DefaultContainerName + "/assets/{0}";

        /// <summary>
        /// The create game packages resource path.
        /// </summary>
        public const string GamePackagesResourcePath = "/cloudservices/gameservices/resources/gameservices/~/{0}/{1}/images/{2}/CodeFiles";

        /// <summary>
        /// The game package resource path.
        /// </summary>
        public const string GamePackageResourcePath = "/cloudservices/gameservices/resources/gameservices/~/{0}/{1}/images/{2}/CodeFiles/{3}";

        /// <summary>
        /// The VM package resource path.
        /// </summary>
        public const string VmPackageResourcePath = "/cloudservices/gameservices/resources/gameservices/~/{0}/{1}/images/{2}";

        /// <summary>
        /// The game mode schemas get resource path.
        /// </summary>
        public const string GameModeSchemasGetResourcePath = "/cloudservices/gameservices/resources/gameservices/~/" + ContainerResourceType + "/" + DefaultContainerName + "/variantschemas?details={0}";

        /// <summary>
        /// The game mode schemas resource path.
        /// </summary>
        public const string GameModeSchemasResourcePath = "/cloudservices/gameservices/resources/gameservices/~/" + ContainerResourceType + "/" + DefaultContainerName + "/variantschemas";

        /// <summary>
        /// The game mode schema resource path.
        /// </summary>
        public const string GameModeSchemaResourcePath = "/cloudservices/gameservices/resources/gameservices/~/" + ContainerResourceType + "/" + DefaultContainerName + "/variantschemas/{0}";

        /// <summary>
        /// The game modes resource path.
        /// </summary>
        public const string GameModesResourcePath = "/cloudservices/gameservices/resources/gameservices/~/" + ContainerResourceType + "/" + DefaultContainerName + "/variantschemas/{0}/variants";

        /// <summary>
        /// The game mode resource path.
        /// </summary>
        public const string GameModeResourcePath = "/cloudservices/gameservices/resources/gameservices/~/" + ContainerResourceType + "/" + DefaultContainerName + "/variantschemas/{0}/variants/{1}";

        /// <summary>
        /// The certificates resource path.
        /// </summary>
        public const string CertificatesResourcePath = "/cloudservices/gameservices/resources/gameservices/~/" + ContainerResourceType + "/" + DefaultContainerName + "/certificates";

        /// <summary>
        /// The certificates resource path.
        /// </summary>
        public const string CertificateResourcePath = "/cloudservices/gameservices/resources/gameservices/~/" + ContainerResourceType + "/" + DefaultContainerName + "/certificates/{0}";

        /// <summary>
        /// The per-Cloud Game certificates resource path.
        /// </summary>
        public const string CertificatesForGameResourcePath = "/cloudservices/gameservices/resources/gameservices/~/" + ContainerResourceType + "/" + DefaultContainerName + "/certificates?gsiSetId={0}";

        /// <summary>
        /// The deploy cloud game path.
        /// </summary>
        public const string DeployCloudGamePath = "/cloudservices/gameservices/resources/gameservices/~/{0}/{1}?operation=publish&sandboxes={2}&geoRegion={3}";

        /// <summary>
        /// The stop cloud game path.
        /// </summary>
        public const string StopCloudGamePath = "/cloudservices/gameservices/resources/gameservices/~/{0}/{1}?operation=unpublish";

        /// <summary>
        /// The configure cloud game path.
        /// </summary>
        public const string ConfigureGamePath = "/cloudservices/gameservices/resources/gameservices/~/{0}/{1}?operation=configure";

        /// <summary>
        /// The resource properties path.
        /// </summary>
        public const string ResourcePropertiesPath = "/resourceproviders/gameservices/Properties";

        /// <summary>
        /// The check container name availability.
        /// </summary>
        public const string CheckContainerNameAvailabilityPath = "/cloudservices/gameservices/resources/gameservices/" + ContainerResourceType + "/?op=checknameavailability&resourceName={0}";

        /// <summary>
        /// The container resource path.
        /// </summary>
        public const string ContainerResourcePath = "/cloudservices/gameservices/resources/gameservices/" + ContainerResourceType + "/" + DefaultContainerName;

        /// <summary>
        /// The dashboard summary for cloud game path.
        /// </summary>
        public const string DashboardSummaryPath = "/cloudservices/gameservices/resources/gameservices/~/{0}/{1}/monitoring?Details=DashboardSummary";

        /// <summary>
        /// The deployments report path.
        /// </summary>
        public const string DeploymentsReportPath = "/cloudservices/gameservices/resources/gameservices/~/{0}/{1}/poolunits/reports/deployments";

        /// <summary>
        /// The servicepools report path.
        /// </summary>
        public const string ServicepoolsReportPath = "/cloudservices/gameservices/resources/gameservices/~/{0}/{1}/poolunits/reports/servicepools";

        /// <summary>
        /// The Log File path.
        /// </summary>
        public const string LogFilePath = "/cloudservices/gameservices/resources/gameservices/~/{0}/{1}/diagnostics/logs/{2}";

        /// <summary>
        /// The Dump File path.
        /// </summary>
        public const string DumpFilePath = "/cloudservices/gameservices/resources/gameservices/~/{0}/{1}/diagnostics/dumps/{2}";

        /// <summary>
        /// The Enumerate Clusters path.
        /// </summary>
        public const string EnumerateClustersPath = "/cloudservices/gameservices/resources/gameservices/~/{0}/{1}/clusters/?geoRegion={2}&status={3}";

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
        /// The default container name
        /// </summary>
        public const string DefaultContainerName = "container";

        /// <summary>
        /// The container resource type
        /// </summary>
        public const string ContainerResourceType = "gameservicescontainer";

        /// <summary>
        /// The Xbox One compute resource type
        /// </summary>
        public const string XboxOneComputeResourceType = "xboxlivecompute";

        /// <summary>
        /// The Xbox 360 compute resource type
        /// </summary>
        public const string Xbox360ComputeResourceType = "xboxlivecomputethreesixty";

        /// <summary>
        /// The PC compute resource type
        /// </summary>
        public const string PcComputeResourceType = "gameservicescomputepc";

        /// <summary>
        /// All of the resource types which qualify as a "cloud game"
        /// </summary>
        public static readonly string[] CloudGameResourceTypes =
        {
            XboxOneComputeResourceType,
            Xbox360ComputeResourceType,
            PcComputeResourceType
        };

        /// <summary>
        /// The schema version
        /// </summary>
        public const string SchemaVersion = "1.0";

        /// <summary>
        /// The cloud games resource path
        /// </summary>
        public const string CloudGameResourcePath = "/cloudservices/gameservices/resources/gameservices/{0}/{1}";

        /// <summary>
        /// The get cloud services resource path.
        /// </summary>
        public const string GetCloudServicesResourcePath = "/cloudservices/gameservices?detailLevel=full";
    }
}