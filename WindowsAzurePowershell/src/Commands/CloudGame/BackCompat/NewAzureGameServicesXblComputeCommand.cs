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

namespace Microsoft.WindowsAzure.Commands.CloudGame.BackCompat
{
    using Utilities.CloudGame.BackCompat;
    using System.IO;
    using System.Management.Automation;

    /// <summary>
    /// Create cloud game package.
    /// </summary>
    [Cmdlet(VerbsCommon.New, "AzureGameServicesXblCompute"), OutputType(typeof(bool))]
    public class NewAzureGameServicesXblComputeCommand : AzureGameServicesHttpClientCommandBase
    {
        [Parameter(Position = 0, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The Xbox Live compute instance name.")]
        [ValidateNotNullOrEmpty]
        public string XblComputeName { get; set; }

        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The title Id.")]
        [ValidateNotNullOrEmpty]
        public string TitleId { get; set; }

        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The selection order to use.")]
        [ValidateNotNullOrEmpty]
        public int SelectionOrder { get; set; }

        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The sandboxes to use (comma seperated list.)")]
        [ValidateNotNullOrEmpty]
        public string Sandboxes { get; set; }

        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The resourceSetIds to use (comma seperated list.)")]
        [ValidateNotNullOrEmpty]
        public string ResourceSetIds { get; set; }

        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The game mode schema Id")]
        [ValidateNotNullOrEmpty]
        public string SchemaId { get; set; }

        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The game mode schema name")]
        [ValidateNotNullOrEmpty]
        public string SchemaName { get; set; }

        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The game mode schema local filename")]
        [ValidateNotNullOrEmpty]
        public string SchemaFileName { get; set; }

        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The game mode schema file stream.")]
        [ValidateNotNullOrEmpty]
        public Stream SchemaStream { get; set; }

        public IXblComputeClient Client { get; set; }

        protected override void Execute()
        {
            Client = Client ?? new XblComputeClient(CurrentSubscription, WriteDebugLog);
            var result = Client.NewXblCompute(
                TitleId,
                SelectionOrder,
                Sandboxes,
                ResourceSetIds,
                XblComputeName,
                SchemaId,
                SchemaName,
                SchemaFileName,
                SchemaStream).Result;
            WriteObject(result);
        }
    }
}