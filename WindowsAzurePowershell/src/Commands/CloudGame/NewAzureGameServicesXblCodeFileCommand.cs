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

namespace Microsoft.WindowsAzure.Commands.XblCompute
{
    using Microsoft.WindowsAzure.Commands.Utilities.XblCompute;
    using System.IO;
    using System.Management.Automation;

    /// <summary>
    /// Create cloud game package.
    /// </summary>
    [Cmdlet(VerbsCommon.New, "AzureGameServicesXblCodeFile"), OutputType(typeof(bool))]
    public class NewAzureGameServicesXblCodeFileCommand : AzureGameServicesHttpClientCommandBase
    {
        [Parameter(Position = 0, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The Xbox Live compute instance name.")]
        [ValidateNotNullOrEmpty]
        public string XblComputeName { get; set; }

        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The game service Id.")]
        [ValidateNotNullOrEmpty]
        public string GameServiceId { get; set; }

        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The name of the code file")]
        [ValidateNotNullOrEmpty]
        public string CodeFileName { get; set; }
        
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The original name of the code file file")]
        [ValidateNotNullOrEmpty]
        public string CodeFileFileName { get; set; }

        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The game code file file stream.")]
        [ValidateNotNullOrEmpty]
        public Stream CodeFileStream { get; set; }

        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Whether this code file should be active.")]
        [ValidateNotNullOrEmpty]
        public bool IsActive { get; set; }

        public IXblComputeClient Client { get; set; }

        public override void ExecuteCmdlet()
        {
            Client = Client ?? new XblComputeClient(CurrentSubscription, WriteDebug);
            string result = null;
            CatchAggregatedExceptionFlattenAndRethrow(
                () =>
                {
                    result = Client.NewXblCodeFile(
                        XblComputeName,
                        GameServiceId,
                        CodeFileName,
                        CodeFileFileName,
                        IsActive,
                        CodeFileStream).Result; 
                });
            WriteObject(result);
        }
    }
}