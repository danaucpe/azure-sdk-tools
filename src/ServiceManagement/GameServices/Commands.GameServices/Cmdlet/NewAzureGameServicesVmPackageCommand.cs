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

namespace Microsoft.WindowsAzure.Commands.GameServices.Cmdlet
{
    using System;
    using System.IO;
    using System.Management.Automation;
    using Microsoft.WindowsAzure.Commands.GameServices.Model.Common;
    using Microsoft.WindowsAzure.Commands.GameServices.Model.Contract;
    using Microsoft.WindowsAzure.Commands.GameServices.Model;

    /// <summary>
    /// Create cloud game package.
    /// </summary>
    [Cmdlet(VerbsCommon.New, "AzureGameServicesVmPackage"), OutputType(typeof(ItemCreatedResponse))]
    public class NewAzureGameServicesVmPackageCommand : AzureGameServicesHttpClientCommandBase
    {
        [Parameter(Position = 0, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The cloud game name.")]
        [ValidatePattern(ClientHelper.CloudGameNameRegex)]
        public string CloudGameName { get; set; }

        [Parameter(Position = 1, Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The cloud game platform.")]
        [ValidateNotNullOrEmpty]
        public CloudGamePlatform Platform { get; set; }

        [Alias("PackageName")]
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The name of the VM package.")]
        [ValidatePattern(ClientHelper.ItemNameRegex)]
        public string Name { get; set; }

        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The maximum number of players allowed.")]
        [ValidateNotNull]
        public int MaxPlayers { get; set; }

        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The IDs of the certificates to use")]
        public Guid[] CertificateIds { get; set; }

        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The ID of the asset file to use")]
        public Guid? AssetId { get; set; }

        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The name of the local cspkg file")]
        [ValidateNotNullOrEmpty]
        public string CspkgFileName { get; set; }

        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The CSPKG file stream.")]
        [ValidateNotNullOrEmpty]
        public Stream CspkgStream { get; set; }

        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The name of the local cscfg file")]
        [ValidateNotNullOrEmpty]
        public string CscfgFileName { get; set; }

        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The CSCFG file stream.")]
        [ValidateNotNullOrEmpty]
        public Stream CscfgStream { get; set; }

        public ICloudGameClient Client { get; set; }

        protected override void Execute()
        {
            Client = Client ?? new CloudGameClient(CurrentContext, WriteDebugLog);
            var result = Client.NewVmPackage(
                CloudGameName,
                Platform,
                Name,
                MaxPlayers,
                AssetId,
                CertificateIds,
                CspkgFileName,
                CspkgStream,
                CscfgFileName,
                CscfgStream).Result;
            WriteObject(new ItemCreatedResponse(result.VmPackageId));
        }
    }
}