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

namespace Microsoft.WindowsAzure.Commands.CloudGame
{
    using System;
    using System.Management.Automation;
    using Utilities.CloudGame;

    /// <summary>
    /// Remove the certificate.
    /// </summary>
    [Cmdlet(VerbsCommon.Remove, "AzureGameServicesCertificate"), OutputType(typeof(bool))]
    public class RemoveAzureGameServicesCertificateCommand : AzureGameServicesHttpClientCommandBase
    {
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The certificate Id.")]
        [ValidateNotNullOrEmpty]
        public Guid CertificateId { get; set; }

        [Parameter(HelpMessage = "Do not confirm deletion of certificate.")]
        public SwitchParameter Force { get; set; }

        public ICloudGameClient Client { get; set; }

        protected override void Execute()
        {
            ConfirmAction(Force.IsPresent,
                          string.Format("CertificateId:{0} will be deleted by this action.", CertificateId),
                          string.Empty,
                          string.Empty,
                          () =>
                          {
                              Client = Client ?? new CloudGameClient(CurrentSubscription, WriteDebugLog);
                              var result = Client.RemoveCertificate(CertificateId).Result;
                              WriteObject(result);
                          });
        }
    }
}