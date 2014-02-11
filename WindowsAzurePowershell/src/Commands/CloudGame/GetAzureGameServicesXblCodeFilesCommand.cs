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
    using Microsoft.WindowsAzure.Commands.Utilities.XblCompute;
    using Microsoft.WindowsAzure.Commands.Utilities.XblCompute.Contract;
    using System.Management.Automation;

    /// <summary>
    /// Get the cloud game asset.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "AzureGameServicesXblCodeFiles"), OutputType(typeof(XblAssetCollectionResponse))]
    public class GetAzureGameServicesXblCodeFilesCommand : AzureGameServicesHttpClientCommandBase
    {
        [Parameter(Position = 0, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The Xbox Live compute instance name.")]
        [ValidateNotNullOrEmpty]
        public string XblComputeName { get; set; }

        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The game service Id.")]
        [ValidateNotNullOrEmpty]
        public string XblGameServiceId { get; set; }

        public IXblComputeClient Client { get; set; }

        protected override void Execute()
        {
            Client = Client ?? new XblComputeClient(CurrentSubscription, WriteDebugLog);
            var result = Client.GetXblCodeFiles(XblComputeName, XblGameServiceId).Result;
            WriteObject(result);
        }
    }
}