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

namespace Microsoft.WindowsAzure.Management.XblCompute
{
    using Microsoft.WindowsAzure.Commands.Utilities.XblCompute;
    using Microsoft.WindowsAzure.Commands.Utilities.XblCompute.Contract;
    using Microsoft.WindowsAzure.Commands.XblCompute;
    using System;
    using System.Management.Automation;

    /// <summary>
    /// Get a list of clusters in the regions and matching the status specified in the request.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "AzureGameServicesXblClusters"), OutputType(typeof(XblEnumerateClustersResponse))]
    public class GetAzureGameServicesXblClustersCommand : AzureGameServicesHttpClientCommandBase
    {
        [Parameter(Position = 0, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The Xbox Live compute instance name.")]
        [ValidateNotNullOrEmpty]
        public string XblComputeName { get; set; }

        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The geo regiond to enumerate")]
        [ValidateNotNullOrEmpty]
        public string GeoRegion { get; set; }

        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The status of the cluster")]
        [ValidateNotNullOrEmpty]
        public string Status { get; set; }

        public IXblComputeClient Client { get; set; }

        public override void ExecuteCmdlet()
        {
            Client = Client ?? new XblComputeClient(CurrentSubscription, WriteDebug);
            XblEnumerateClustersResponse result = null;

            CatchAggregatedExceptionFlattenAndRethrow(() => { result = Client.GetClusters(XblComputeName, GeoRegion, Status).Result; });
            WriteObject(result);
        }
    }
}