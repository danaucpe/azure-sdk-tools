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
    /// Remove the game mode.
    /// </summary>
    [Cmdlet(VerbsCommon.Remove, "AzureGameServicesGameMode"), OutputType(typeof(bool))]
    public class RemoveAzureGameServicesGameModeCommand : AzureGameServicesHttpClientCommandBase
    {
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The parent game mode schema ID.")]
        [ValidateNotNullOrEmpty]
        public Guid GameModeSchemaId { get; set; }

        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The game mode ID.")]
        [ValidateNotNullOrEmpty]
        public Guid GameModeId { get; set; }

        [Parameter(HelpMessage = "Do not confirm deletion of game mode.")]
        public SwitchParameter Force { get; set; }

        public ICloudGameClient Client { get; set; }

        protected override void Execute()
        {
            ConfirmAction(Force.IsPresent,
                          string.Format("GameModeId:{0} will be deleted by this action.", GameModeId),
                          string.Empty,
                          string.Empty,
                          () =>
                          {
                              Client = Client ?? new CloudGameClient(CurrentSubscription, WriteDebugLog);
                              var result = Client.RemoveGameMode(GameModeSchemaId, GameModeId).Result;
                              WriteObject(result);
                          });
        }
    }
}