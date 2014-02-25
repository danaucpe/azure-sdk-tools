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
    using System.IO;
    using System.Management.Automation;
    using Utilities.CloudGame;

    /// <summary>
    /// Create the cloud game asset.
    /// </summary>
    [Cmdlet(VerbsCommon.New, "AzureGameServicesAsset"), OutputType(typeof(string))]
    public class NewAzureGameServicesAssetCommand : AzureGameServicesHttpClientCommandBase
    {
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The name of the asset.")]
        [ValidateNotNullOrEmpty]
        public string AssetName { get; set; }

        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The original filename of the asset file.")]
        [ValidateNotNullOrEmpty]
        public string AssetFileName { get; set; }

        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The asset filestream.")]
        [ValidateNotNullOrEmpty]
        public Stream AssetStream { get; set; }

        public ICloudGameClient Client { get; set; }

        protected override void Execute()
        {
            Client = Client ?? new CloudGameClient(CurrentSubscription, WriteDebugLog);
            var result = Client.NewAsset(AssetName, AssetFileName, AssetStream).Result;
            WriteObject(result);
        }
    }
}