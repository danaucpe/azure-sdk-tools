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
    using Microsoft.WindowsAzure.Commands.GameServices.Model.Common;

    /// <summary>
    /// Remove the config item.
    /// </summary>
    [Cmdlet(VerbsCommon.New, "AzureGameServicesInsightsConfigItem"), OutputType(typeof(ItemCreatedResponse))]
    public class NewAzureGameServicesInsightsConfigItemCommand : AzureGameServicesHttpClientCommandBase
    {
        [Alias("Name")]
        [Parameter(Mandatory = true, Position = 0, ValueFromPipelineByPropertyName = true, HelpMessage = "The unique target name.")]
        [ValidateNotNullOrEmpty]
        public string TargetName { get; set; }

        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The target type (defaults to EventHub).")]
        [ValidateNotNullOrEmpty]
        public string TargetType { get; set; }

        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The connection string.")]
        [ValidateNotNullOrEmpty]
        public string ConnectionString { get; set; }

        public ICloudGameClient Client { get; set; }

        protected override void Execute()
        {
            var targetType = string.IsNullOrEmpty(this.TargetType) ? "EventHub" : this.TargetType;
            Client = Client ?? new CloudGameClient(CurrentContext, WriteDebugLog);
            var result = Client.NewInsightsConfigItem(this.TargetName, targetType, this.ConnectionString).Result;
            WriteObject(new ItemCreatedResponse(this.TargetName));
        }
    }
}