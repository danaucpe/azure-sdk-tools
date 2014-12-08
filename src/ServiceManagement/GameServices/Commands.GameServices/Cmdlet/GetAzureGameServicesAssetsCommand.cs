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
    using System.Collections.Generic;
    using System.Management.Automation;
    using Microsoft.WindowsAzure.Commands.GameServices.Model;
    using Microsoft.WindowsAzure.Commands.GameServices.Model.Contract;

    /// <summary>
    /// Get the cloud game assets.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "AzureGameServicesAssets"), OutputType(typeof(List<AssetResponse>))]
    public class GetAzureGameServicesAssetsCommand : AzureGameServicesHttpClientCommandBase
    {
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "An optional cloud game ID to filter by.")]
        public Guid? CloudGameId { get; set; }

        public ICloudGameClient Client { get; set; }

        protected override void Execute()
        {
            Client = Client ?? new CloudGameClient(CurrentContext, WriteDebugLog);
            var result = Client.GetAssets(CloudGameId).Result;
            WriteObject(result == null ? new List<AssetResponse>() : result.Assets);
        }
    }
}