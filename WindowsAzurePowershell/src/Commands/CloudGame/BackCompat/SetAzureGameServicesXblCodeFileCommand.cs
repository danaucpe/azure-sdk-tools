﻿// ----------------------------------------------------------------------------------
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
    using System;
    using System.Management.Automation;
    using Utilities.CloudGame.BackCompat;

    /// <summary>
    /// Sets cloud game code file.
    /// </summary>
    [Cmdlet(VerbsCommon.Set, "AzureGameServicesXblCodefile"), OutputType(typeof(bool))]
    [Obsolete("This cmdlet is obsolete. Please use Set-AzureGameServicesGamePackage instead.")]
    public class SetAzureGameServicesXblCodeFileCommand : AzureGameServicesHttpClientCommandBase
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
        
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The friendly name of the code file")]
        [ValidatePattern(Utilities.CloudGame.ClientHelper.ItemNameRegex)]
        public string CodeFileName { get; set; }

        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The file name of the code file")]
        [ValidateNotNullOrEmpty]
        public string CodeFileFileName { get; set; }

        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Whether this code file should be active.")]
        [ValidateNotNullOrEmpty]
        public bool IsActive { get; set; }

        public IXblComputeClient Client { get; set; }

        protected override void Execute()
        {
            Client = Client ?? new XblComputeClient(CurrentSubscription, WriteDebugLog);
            var result = Client.SetXblCodeFile(XblComputeName, GameServiceId, CodeFileId, CodeFileName, CodeFileFileName, IsActive).Result;
            WriteObject(result);
        }
    }
}