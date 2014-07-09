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
    using System.Net;
    using Microsoft.WindowsAzure.ServiceManagement;
    using Utilities.CloudGame;
    using Utilities.CloudGame.Common;

    /// <summary>
    /// Activates a game package (shortcut for Set-AzureGameServicesGamePackage).
    /// </summary>
    [Cmdlet("Activate", "AzureGameServicesGamePackage"), OutputType(typeof(bool))]
    public class ActivateAzureGameServicesGamePackageCommand : AzureGameServicesHttpClientCommandBase
    {
        [Parameter(Position = 0, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The cloud game name.")]
        [ValidatePattern(ClientHelper.CloudGameNameRegex)]
        public string CloudGameName { get; set; }

        [Parameter(Position = 1, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The cloud game platform.")]
        [ValidateNotNullOrEmpty]
        public CloudGamePlatform Platform { get; set; }

        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The parent VM package ID.")]
        [ValidateNotNullOrEmpty]
        public Guid VmPackageId { get; set; }

        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The game package ID to activate.")]
        [ValidateNotNullOrEmpty]
        public Guid GamePackageId { get; set; }

        public ICloudGameClient Client { get; set; }

        protected override void Execute()
        {
            Client = Client ?? new CloudGameClient(CurrentSubscription, WriteDebugLog);
            var result = Client.GetGamePackages(CloudGameName, Platform, VmPackageId).Result;
            if (result != null && result.GamePackages != null)
            {
                var gamePackageIdStr = GamePackageId.ToString();
                var gamePackage = result.GamePackages.Find((gamePack) => gamePack.GamePackageId == gamePackageIdStr);
                if (gamePackage != null)
                {
                    var result2 = Client.SetGamePackage(CloudGameName, Platform, VmPackageId, GamePackageId, gamePackage.Name, gamePackage.FileName, true).Result;
                    WriteObject(result2);
                    return;
                }
            }
            var notFound = new ServiceManagementClientException(HttpStatusCode.NotFound,
                new ServiceManagementError { Code = HttpStatusCode.NotFound.ToString() },
                string.Empty);
            var error = new ErrorRecord(notFound, "ActivateGamePackageFailedNotFound", ErrorCategory.InvalidOperation, Client);
            error.ErrorDetails = new ErrorDetails("Unable to find game package to activate.");
            ThrowTerminatingError(error);
        }
    }
}