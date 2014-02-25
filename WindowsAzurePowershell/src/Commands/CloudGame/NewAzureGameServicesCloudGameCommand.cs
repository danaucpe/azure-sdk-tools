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
    using System.IO;
    using System.Management.Automation;
    using Utilities.CloudGame;
    using Utilities.CloudGame.Common;

    /// <summary>
    /// Create cloud game.
    /// </summary>
    [Cmdlet(VerbsCommon.New, "AzureGameServicesCloudGame"), OutputType(typeof(bool))]
    public class NewAzureGameServicesCloudGameCommand : AzureGameServicesHttpClientCommandBase
    {
        [Parameter(Position = 0, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The cloud game name.")]
        [ValidateNotNullOrEmpty]
        public string CloudGameName { get; set; }

        [Parameter(Position = 1, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The cloud game platform.")]
        [ValidateNotNullOrEmpty]
        public CloudGamePlatform Platform { get; set; }

        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The cloud title ID.")]
        [ValidateNotNullOrEmpty]
        public string TitleId { get; set; }

        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The selection order to use.")]
        [ValidateNotNullOrEmpty]
        public int SelectionOrder { get; set; }

        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The sandboxes to use (comma seperated list).")]
        [ValidateNotNullOrEmpty]
        public string Sandboxes { get; set; }

        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The resourceSetIds to use (comma seperated list).")]
        [ValidateNotNullOrEmpty]
        public string ResourceSetIds { get; set; }

        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "An existing game mode schema ID.")]
        [ValidateNotNullOrEmpty]
        public Guid SchemaId { get; set; }

        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The game mode schema name for creating a new schema.")]
        [ValidateNotNullOrEmpty]
        public string SchemaName { get; set; }

        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The game mode schema local filename for creating a new schema.")]
        [ValidateNotNullOrEmpty]
        public string SchemaFileName { get; set; }

        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The game mode schema file stream for creating a new schema.")]
        [ValidateNotNullOrEmpty]
        public Stream SchemaStream { get; set; }

        public ICloudGameClient Client { get; set; }

        protected override void Execute()
        {
            Client = Client ?? new CloudGameClient(CurrentSubscription, WriteDebugLog);
            var result = Client.NewCloudGame(
                Platform,
                TitleId,
                SelectionOrder,
                Sandboxes,
                ResourceSetIds,
                CloudGameName,
                SchemaId,
                SchemaName,
                SchemaFileName,
                SchemaStream).Result;
            WriteObject(result);
        }
    }
}