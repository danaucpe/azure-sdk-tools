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
    using System.Management.Automation;

    /// <summary>
    /// Remove the cloud game asset.
    /// </summary>
    [Cmdlet(VerbsCommon.Remove, "AzureGameServicesXblCodeFile"), OutputType(typeof(bool))]
    public class RemoveAzureGameServicesXblCodeFileCommand : AzureGameServicesHttpClientCommandBase
    {
        [Parameter(Position = 0, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The Xbox Live compute instance name.")]
        [ValidateNotNullOrEmpty]
        public string XblComputeName { get; set; }

        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The game service Id.")]
        [ValidateNotNullOrEmpty]
        public string GameServiceId { get; set; }

        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The game code file file stream.")]
        [ValidateNotNullOrEmpty]
        public string CodeFileId { get; set; }

        [Parameter(HelpMessage = "Do not confirm deletion of XblCodeFile.")]
        public SwitchParameter Force { get; set; }

        public IXblComputeClient Client { get; set; }

        public override void ExecuteCmdlet()
        {
            ConfirmAction(Force.IsPresent,
                          string.Format("CodeFileId:{0} will be deleted by this action.", CodeFileId),
                          string.Empty,
                          string.Empty,
                          () =>
                          {
                              Client = Client ?? new XblComputeClient(CurrentSubscription, WriteDebug);
                              var result = false;

                              CatchAggregatedExceptionFlattenAndRethrow(() => { result = Client.RemoveXblCodeFile(XblComputeName, GameServiceId, CodeFileId).Result; });
                              WriteObject(result);
                          });
        }
    }
}