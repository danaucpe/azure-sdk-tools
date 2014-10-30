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
    using Microsoft.WindowsAzure.Commands.GameServices.Model;
    using Utilities.CloudGame.Common;

    /// <summary>
    /// Sets cloud game properties.
    /// </summary>
    [Cmdlet(VerbsCommon.Set, "AzureGameServicesCloudGameProperties"), OutputType(typeof(bool))]
    public class SetAzureGameServicesCloudGamePropertiesCommand : AzureGameServicesHttpClientCommandBase
    {
        private string[] sandboxes = new string[0];
        private string[] resourceSets = new string[0];

        [Parameter(Position = 0, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The cloud game name (does not get set; used purely for looking up).")]
        [ValidatePattern(ClientHelper.CloudGameNameRegex)]
        public string CloudGameName { get; set; }

        [Parameter(Position = 1, Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The cloud game platform (does not get set; used purely for looking up).")]
        [ValidateNotNullOrEmpty]
        public CloudGamePlatform Platform { get; set; }

        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "A comma-delimited list of resource set IDs.")]
        public string[] ResourceSets
        {
            get { return this.resourceSets; }
            set
            {
                if (value.Length == 1)
                {
                    // For back-compat
                    this.resourceSets = value[0].Split(',');
                }
            }
        }

        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "A comma-delimited list of sandboxes.")]
        public string[] Sandboxes
        {
            get { return this.sandboxes; }
            set
            {
                if (value.Length == 1)
                {
                    // For back-compat
                    this.sandboxes = value[0].Split(',');
                }
            }
        }

        public ICloudGameClient Client { get; set; }

        protected override void Execute()
        {
            Client = Client ?? new CloudGameClient(CurrentContext, WriteDebugLog);
            var result = Client.ConfigureCloudGame(CloudGameName, Platform, ResourceSets, Sandboxes).Result;
            WriteObject(result);
        }
    }
}