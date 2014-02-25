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

namespace Microsoft.WindowsAzure.Commands.CloudGame
{
    using System;
    using System.Management.Automation;
    using Utilities.CloudGame;

    /// <summary>
    /// Remove the asset package.
    /// </summary>
    [Cmdlet(VerbsCommon.Remove, "AzureGameServicesAsset"), OutputType(typeof(bool))]
    public class RemoveAzureGameServicesAssetCommand : AzureGameServicesHttpClientCommandBase
    {
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The asset ID.")]
        [ValidateNotNullOrEmpty]
        public Guid AssetId { get; set; }

        [Parameter(HelpMessage = "Do not confirm deletion of asset.")]
        public SwitchParameter Force { get; set; }

        public ICloudGameClient Client { get; set; }

        protected override void Execute()
        {
            ConfirmAction(Force.IsPresent,
                          string.Format("AssetId:{0} will be deleted by this action.", AssetId),
                          string.Empty,
                          string.Empty,
                          () =>
                          {
                              Client = Client ?? new CloudGameClient(CurrentSubscription, WriteDebugLog);
                              var result = Client.RemoveAsset(AssetId).Result;
                              WriteObject(result);
                          });
        }
    }
}