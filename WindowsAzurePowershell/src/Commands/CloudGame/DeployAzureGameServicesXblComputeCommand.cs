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

namespace Microsoft.WindowsAzure.Commands.XblCompute
{
    using System.Management.Automation;
    using Microsoft.WindowsAzure.Commands.Utilities.XblCompute;

    /// <summary>
    /// Gets cloud game service.
    /// </summary>
    [Cmdlet("Deploy", "AzureGameServicesXblCompute"), OutputType(typeof(bool))]
    public class DeployAzureGameServicesXblComputeCommand : AzureGameServicesHttpClientCommandBase
    {
        [Parameter(Position = 0, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The Xbox Live compute instance name.")]
        [ValidateNotNullOrEmpty]
        public string XblComputeName { get; set; }

        [Parameter(Position = 1, Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The sandboxes to deploy to")]
        [ValidateNotNullOrEmpty]
        public string Sandboxes { get; set; }

        [Parameter(Position = 2, Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The geo Regions to deploy to")]
        [ValidateNotNullOrEmpty]
        public string GeoRegions { get; set; }

        public IXblComputeClient Client { get; set; }

        public override void ExecuteCmdlet()
        {
            Client = Client ?? new XblComputeClient(CurrentSubscription, WriteDebug);
            var result = false;

            CatchAggregatedExceptionFlattenAndRethrow(() => { result = Client.DeployXblCompute(XblComputeName, Sandboxes ?? string.Empty, GeoRegions ?? string.Empty).Result; });
            WriteObject(result);
        }
    }
}