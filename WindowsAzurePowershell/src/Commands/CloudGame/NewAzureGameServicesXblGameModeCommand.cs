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
    using System.IO;
    using System.Management.Automation;

    /// <summary>
    /// Create the cloud game mode.
    /// </summary>
    [Cmdlet(VerbsCommon.New, "AzureGameServicesXblGameMode"), OutputType(typeof(NewXblGameModeResponse))]
    public class NewAzureGameServicesXblGameModeCommand : AzureGameServicesHttpClientCommandBase
    {
        [Parameter(Position = 0, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The Xbox Live compute instance name.")]
        [ValidateNotNullOrEmpty]
        public string XblComputeName { get; set; }

        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The name of the game mode.")]
        [ValidateNotNullOrEmpty]
        public string GameModeName { get; set; }

        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The original filename of the game mode file.")]
        [ValidateNotNullOrEmpty]
        public string GameModeFileName { get; set; }

        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The game mode content stream.")]
        [ValidateNotNullOrEmpty]
        public Stream GameModeStream { get; set; }

        public IXblComputeClient Client { get; set; }

        public override void ExecuteCmdlet()
        {
            Client = Client ?? new XblComputeClient(CurrentSubscription, WriteDebug);
            NewXblGameModeResponse result = null;

            CatchAggregatedExceptionFlattenAndRethrow(() => { result = Client.NewXblGameMode(XblComputeName, GameModeName, GameModeFileName, GameModeStream).Result; });
            WriteObject(result);
        }
    }
}