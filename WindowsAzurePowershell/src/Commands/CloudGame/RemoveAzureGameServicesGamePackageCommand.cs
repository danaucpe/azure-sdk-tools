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
    using Utilities.CloudGame.Common;

    /// <summary>
    /// Remove the game package.
    /// </summary>
    [Cmdlet(VerbsCommon.Remove, "AzureGameServicesGamePackage"), OutputType(typeof(bool))]
    public class RemoveAzureGameServicesGamePackageCommand : AzureGameServicesHttpClientCommandBase
    {
        [Parameter(Position = 0, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The cloud game name.")]
        [ValidateNotNullOrEmpty]
        public string CloudGameName { get; set; }

        [Parameter(Position = 1, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The cloud game platform.")]
        [ValidateNotNullOrEmpty]
        public CloudGamePlatform Platform { get; set; }

        [Parameter(Position = 2, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The parent VM package Id.")]
        [ValidateNotNullOrEmpty]
        public Guid VmPackageId { get; set; }

        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The game package Id.")]
        [ValidateNotNullOrEmpty]
        public Guid GamePackageId { get; set; }

        [Parameter(HelpMessage = "Do not confirm deletion of game package.")]
        public SwitchParameter Force { get; set; }

        public ICloudGameClient Client { get; set; }

        protected override void Execute()
        {
            ConfirmAction(Force.IsPresent,
                          string.Format("GamePackageId:{0} will be deleted by this action.", GamePackageId),
                          string.Empty,
                          string.Empty,
                          () =>
                          {
                              Client = Client ?? new CloudGameClient(CurrentSubscription, WriteDebugLog);
                              var result = Client.RemoveGamePackage(CloudGameName, Platform, VmPackageId, GamePackageId).Result;
                              WriteObject(result);
                          });
        }
    }
}