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
    using Microsoft.WindowsAzure.Commands.Utilities.XblCompute.Contract;
    using System.IO;
    using System.Management.Automation;

    /// <summary>
    /// Create the cloud game certificate.
    /// </summary>
    [Cmdlet(VerbsCommon.New, "AzureGameServicesXblCertificate"), OutputType(typeof(XblCertificatePostResponse))]
    public class NewAzureGameServicesXblCertificateCommand : AzureGameServicesHttpClientCommandBase
    {
        [Parameter(Position = 0, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The Xbox Live compute instance name.")]
        [ValidateNotNullOrEmpty]
        public string XblComputeName { get; set; }

        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The certificate name.")]
        [ValidateNotNullOrEmpty]
        public string CertificateName { get; set; }

        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The certificate Filename.")]
        [ValidateNotNullOrEmpty]
        public string CertificateFilename { get; set; }

        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The certificate password.")]
        public string CertificatePassword { get; set; }

        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The certificate stream.")]
        [ValidateNotNullOrEmpty]
        public Stream CertificateStream { get; set; }

        public IXblComputeClient Client { get; set; }

        public override void ExecuteCmdlet()
        {
            Client = Client ?? new XblComputeClient(CurrentSubscription, WriteDebug);
            XblCertificatePostResponse result = null;

            CatchAggregatedExceptionFlattenAndRethrow(() => { result = Client.NewXblCertificate(
                XblComputeName, 
                CertificateName,
                CertificateFilename,
                CertificatePassword,
                CertificateStream).Result; });
            WriteObject(result);
        }
    }
}