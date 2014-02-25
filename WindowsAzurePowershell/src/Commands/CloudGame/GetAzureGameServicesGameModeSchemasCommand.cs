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
    using Utilities.CloudGame;
    using Utilities.CloudGame.Contract;
    using System.Management.Automation;

    /// <summary>
    /// Get the game mode schemas.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "AzureGameServicesGameModeSchemas"), OutputType(typeof(GameModeSchemaCollectionResponse))]
    public class GetAzureGameServicesGameModeSchemasCommand : AzureGameServicesHttpClientCommandBase
    {
        [Parameter(Position = 0, Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "Fetch child game modes of each schema.")]
        [ValidateNotNullOrEmpty]
        public bool IncludeGameModes { get; set; }

        public ICloudGameClient Client { get; set; }

        protected override void Execute()
        {
            Client = Client ?? new CloudGameClient(CurrentSubscription, WriteDebugLog);
            var result = Client.GetGameModeSchemas(IncludeGameModes).Result;
            WriteObject(result);
        }
    }
}