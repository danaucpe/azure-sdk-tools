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

namespace Microsoft.WindowsAzure.Commands.GameServices.Cmdlet
{
    using System;
    using System.Management.Automation;
    using Microsoft.WindowsAzure.Commands.GameServices.Model;

    /// <summary>
    /// Remove the config item.
    /// </summary>
    [Cmdlet(VerbsCommon.Remove, "AzureGameServicesInsightsConfigItem")]
    public class RemoveAzureGameServicesInsightsConfigItemCommand : AzureGameServicesHttpClientCommandBase
    {
        [Alias("Name")]
        [Parameter(Mandatory = true, Position = 0, ValueFromPipelineByPropertyName = true, HelpMessage = "The unique target name.")]
        [ValidateNotNullOrEmpty]
        public string TargetName { get; set; }

        [Parameter(HelpMessage = "Do not confirm deletion of config item.")]
        public SwitchParameter Force { get; set; }

        public ICloudGameClient Client { get; set; }

        protected override void Execute()
        {
            ConfirmAction(Force.IsPresent,
                          string.Format("Insights configuration item '{0}' will be deleted by this action.", TargetName),
                          string.Empty,
                          string.Empty,
                          () =>
                          {
                              Client = Client ?? new CloudGameClient(CurrentContext, WriteDebugLog);
                              var result = Client.RemoveInsightsConfigItem(TargetName).Result;
                          });
        }
    }
}