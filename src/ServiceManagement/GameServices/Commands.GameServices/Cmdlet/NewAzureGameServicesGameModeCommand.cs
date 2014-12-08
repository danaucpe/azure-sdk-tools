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
    using System.IO;

    /// <summary>
    /// Create the cloud game mode.
    /// </summary>
    [Cmdlet(VerbsCommon.New, "AzureGameServicesGameMode"), OutputType(typeof(ItemCreatedResponse))]
    public class NewAzureGameServicesGameModeCommand : AzureGameServicesHttpClientCommandBase
    {
        [Alias("ParentId")]
        [Parameter(Position = 0, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The parent game mode schema ID.")]
        [ValidateNotNullOrEmpty]
        public Guid GameModeSchemaId { get; set; }

        [Alias("GameModeName")]
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The name of the game mode.")]
        [ValidatePattern(ClientHelper.ItemNameRegex)]
        public string Name { get; set; }

        [Alias("GameModeFileName")]
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The original filename of the game mode file.")]
        [ValidateNotNullOrEmpty]
        public string Filename { get; set; }

        [Alias("GameModeStream")]
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The game mode content stream.")]
        [ValidateNotNullOrEmpty]
        public Stream FileStream { get; set; }

        public ICloudGameClient Client { get; set; }

        protected override void Execute()
        {
            Client = Client ?? new CloudGameClient(CurrentContext, WriteDebugLog);
            var result = Client.NewGameMode(GameModeSchemaId, Name, Filename, FileStream).Result;
            WriteObject(new ItemCreatedResponse(result.GameModeId));
        }
    }
}