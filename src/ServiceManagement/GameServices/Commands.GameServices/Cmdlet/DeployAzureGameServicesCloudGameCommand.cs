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
    using System.Linq;
    using System.Management.Automation;
    using Microsoft.WindowsAzure.Commands.GameServices.Model;
    using Microsoft.WindowsAzure.Commands.GameServices.Model.Common;

    /// <summary>
    /// Gets cloud game service.
    /// </summary>
    [Cmdlet("Deploy", "AzureGameServicesCloudGame")]
    public class DeployAzureGameServicesCloudGameCommand : AzureGameServicesHttpClientCommandBase
    {
        private string[] sandboxes = new string[0];
        private string[] geoRegions = new string[0];

        [Parameter(Position = 0, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The cloud game name.")]
        [ValidatePattern(ClientHelper.CloudGameNameRegex)]
        public string CloudGameName { get; set; }

        [Parameter(Position = 1, Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The cloud game platform.")]
        [ValidateNotNullOrEmpty]
        public CloudGamePlatform Platform { get; set; }

        [Parameter(Position = 2, Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The sandboxes to deploy to.")]
        [ValidateNotNullOrEmpty]
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

        [Parameter(Position = 3, Mandatory = false, ValueFromPipelineByPropertyName = true,
            HelpMessage = "The geographic regions to deploy to.")]
        [ValidateNotNullOrEmpty]
        public string[] GeoRegions
        {
            get { return this.geoRegions; }
            set
            {
                if (value.Length == 1)
                {
                    // For back-compat
                    this.geoRegions = value[0].Split(',');
                }
            }
        }

        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, HelpMessage = "Only publish the game (do not attempt to deploy).")]
        public SwitchParameter PublishOnly { get; set; }

        public ICloudGameClient Client { get; set; }

        protected override void Execute()
        {
            Client = Client ?? new CloudGameClient(CurrentContext, WriteDebugLog);
            var result = Client.DeployCloudGame(CloudGameName, Platform, Sandboxes, GeoRegions, PublishOnly.ToBool()).Result;
        }
    }
}