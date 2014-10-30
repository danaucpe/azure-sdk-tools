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
    using System.Linq;
    using System.Management.Automation;
    using Microsoft.WindowsAzure.Commands.GameServices.Model;
    using Utilities.CloudGame.Common;

    /// <summary>
    /// Remove a CloudGame instance.
    /// </summary>
    [Cmdlet(VerbsCommon.Remove, "AzureGameServicesCloudGame"), OutputType(typeof(bool))]
    public class RemoveAzureGameServicesCloudGameCommand : AzureGameServicesHttpClientCommandBase
    {
        [Parameter(Position = 0, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The cloud game name.")]
        [ValidatePattern(ClientHelper.CloudGameNameRegex)]
        public string CloudGameName { get; set; }

        [Parameter(Position = 1, Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The cloud game platform.")]
        [ValidateNotNullOrEmpty]
        public CloudGamePlatform Platform { get; set; }

        [Parameter(HelpMessage = "Do not confirm deletion of cloud game instance.")]
        public SwitchParameter Force { get; set; }

        public ICloudGameClient Client { get; set; }

        protected override void Execute()
        {
            // Show confirmation dialog unless -Force is specified
            ConfirmAction(Force.IsPresent,
                string.Format("Cloud game:{0} will be deleted by this action.", CloudGameName),
                string.Empty,
                string.Empty,
                () =>
                {
                    Client = Client ?? new CloudGameClient(CurrentContext, WriteDebugLog);
                    bool isDeployedToRetail = false;
                    bool knownDeploymentStatus = true;
                    string promptMsg = string.Format("Cloud game:{0} is currently deployed to RETAIL. Are you positive you want to delete it?", CloudGameName);
                    try
                    {
                        var deploymentReport = Client.GetComputeDeploymentsReport(CloudGameName, Platform).Result; // depends on GSMS
                        if (deploymentReport != null)
                        {
                            isDeployedToRetail = deploymentReport.IsDeployedToRetail();
                        }
                    }
                    catch (Exception)
                    {
                        // If we can't verify the deployment status then play it safe and prompt
                        knownDeploymentStatus = false;
                        promptMsg = string.Format("Cloud game:{0} may already be deployed. Are you positive you want to delete it?", CloudGameName);
                    }
                    
                    // Add extra, unskippable prompt if we aren't positive the game is non-RETAIL
                    ConfirmAction((!isDeployedToRetail && knownDeploymentStatus),
                        promptMsg,
                        string.Empty,
                        string.Empty,
                        () =>
                        {
                            var result = Client.RemoveCloudGame(CloudGameName, Platform).Result;
                            WriteObject(result);
                        });
                });
        }
    }
}