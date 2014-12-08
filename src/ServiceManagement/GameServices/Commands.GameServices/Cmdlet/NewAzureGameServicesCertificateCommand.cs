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
    using Microsoft.WindowsAzure.Commands.GameServices.Model;
    using Microsoft.WindowsAzure.Commands.GameServices.Model.Common;
    using System.IO;
    using System.Management.Automation;

    /// <summary>
    /// Create the certificate.
    /// </summary>
    [Cmdlet(VerbsCommon.New, "AzureGameServicesCertificate"), OutputType(typeof(ItemCreatedResponse))]
    public class NewAzureGameServicesCertificateCommand : AzureGameServicesHttpClientCommandBase
    {
        [Alias("CertificateName")]
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The certificate name.")]
        [ValidatePattern(ClientHelper.ItemNameRegex)]
        public string Name { get; set; }

        [Alias("CertificateFilename")]
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The certificate filename.")]
        [ValidateNotNullOrEmpty]
        public string Filename { get; set; }

        [Alias("CertificatePassword")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The certificate password.")]
        public string Password { get; set; }

        [Alias("CertificateStream")]
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The certificate stream.")]
        [ValidateNotNullOrEmpty]
        public Stream FileStream { get; set; }

        public ICloudGameClient Client { get; set; }

        protected override void Execute()
        {
            Client = Client ?? new CloudGameClient(CurrentContext, WriteDebugLog);
            var result = Client.NewCertificate(
                Name,
                Filename,
                Password,
                FileStream).Result;
            WriteObject(new ItemCreatedResponse(result.CertificateId));
        }
    }
}