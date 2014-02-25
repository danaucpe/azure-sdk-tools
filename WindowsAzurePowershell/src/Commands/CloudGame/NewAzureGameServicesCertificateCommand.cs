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
    using System.IO;
    using System.Management.Automation;

    /// <summary>
    /// Create the certificate.
    /// </summary>
    [Cmdlet(VerbsCommon.New, "AzureGameServicesCertificate"), OutputType(typeof(CertificatePostResponse))]
    public class NewAzureGameServicesCertificateCommand : AzureGameServicesHttpClientCommandBase
    {
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The certificate name.")]
        [ValidateNotNullOrEmpty]
        public string CertificateName { get; set; }

        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The certificate filename.")]
        [ValidateNotNullOrEmpty]
        public string CertificateFilename { get; set; }

        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The certificate password.")]
        public string CertificatePassword { get; set; }

        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The certificate stream.")]
        [ValidateNotNullOrEmpty]
        public Stream CertificateStream { get; set; }

        public ICloudGameClient Client { get; set; }

        protected override void Execute()
        {
            Client = Client ?? new CloudGameClient(CurrentSubscription, WriteDebugLog);
            var result = Client.NewCertificate(
                CertificateName,
                CertificateFilename,
                CertificatePassword,
                CertificateStream).Result;
            WriteObject(result);
        }
    }
}