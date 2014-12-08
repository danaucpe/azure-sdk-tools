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

namespace Microsoft.WindowsAzure.Commands.GameServices.Cmdlet
{
    using System.Collections.Generic;
    using Microsoft.WindowsAzure.Commands.GameServices.Model;
    using Microsoft.WindowsAzure.Commands.GameServices.Model.Common;
    using Microsoft.WindowsAzure.Commands.GameServices.Model.Contract;
    using System.Management.Automation;

    /// <summary>
    /// Get a list of clusters in the regions and matching the status specified in the request.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "AzureGameServicesClusters"), OutputType(typeof(List<ClusterInformationEx>))]
    public class GetAzureGameServicesClustersCommand : AzureGameServicesHttpClientCommandBase
    {
        [Parameter(Position = 0, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The cloud game name.")]
        [ValidatePattern(ClientHelper.CloudGameNameRegex)]
        public string CloudGameName { get; set; }

        [Parameter(Position = 1, Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The cloud game platform.")]
        [ValidateNotNullOrEmpty]
        public CloudGamePlatform Platform { get; set; }

        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The geo region(s) to enumerate. Comma-separated if more than one. Empty for all.")]
        public string GeoRegion { get; set; }

        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The status of the clusters to query for. Empty for all.")]
        public string Status { get; set; }

        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The cluster ID for the game service.")]
        public string ClusterId { get; set; }

        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The agent ID for the game service.")]
        public string AgentId { get; set; }

        public ICloudGameClient Client { get; set; }

        protected override void Execute()
        {
            Client = Client ?? new CloudGameClient(CurrentContext, WriteDebugLog);
            var result = Client.GetClusters(CloudGameName, Platform, GeoRegion, Status, ClusterId, AgentId).Result;
            WriteObject(result == null ? new List<ClusterInformationEx>() : result.ClusterInformationCollection);
        }
    }
}