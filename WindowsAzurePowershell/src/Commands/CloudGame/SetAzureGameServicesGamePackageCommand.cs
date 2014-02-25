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
    /// Sets game package.
    /// </summary>
    [Cmdlet(VerbsCommon.Set, "AzureGameServicesGamePackage"), OutputType(typeof(bool))]
    public class SetAzureGameServicesGamePackageCommand : AzureGameServicesHttpClientCommandBase
    {
        [Parameter(Position = 0, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The cloud game name.")]
        [ValidateNotNullOrEmpty]
        public string CloudGameName { get; set; }

        [Parameter(Position = 1, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The cloud game platform.")]
        [ValidateNotNullOrEmpty]
        public CloudGamePlatform Platform { get; set; }

        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The parent VM package ID.")]
        [ValidateNotNullOrEmpty]
        public Guid VmPackageId { get; set; }

        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The game package ID.")]
        [ValidateNotNullOrEmpty]
        public Guid GamePackageId { get; set; }

        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The name of the game package")]
        [ValidateNotNullOrEmpty]
        public string GamePackageName { get; set; }

        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The original name of the game package file")]
        [ValidateNotNullOrEmpty]
        public string GamePackageFileName { get; set; }

        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Whether this game package should be active.")]
        [ValidateNotNullOrEmpty]
        public bool IsActive { get; set; }

        public ICloudGameClient Client { get; set; }

        protected override void Execute()
        {
            Client = Client ?? new CloudGameClient(CurrentSubscription, WriteDebugLog);
            var result = Client.SetGamePackage(CloudGameName, Platform, VmPackageId, GamePackageId, GamePackageName, GamePackageFileName, IsActive).Result;
            WriteObject(result);
        }
    }
}