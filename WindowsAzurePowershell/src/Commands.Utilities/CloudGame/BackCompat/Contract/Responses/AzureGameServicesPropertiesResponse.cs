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

namespace Microsoft.WindowsAzure.Commands.Utilities.CloudGame.BackCompat.Contract
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    [DataContract]
    public class AzureGameServicesPropertiesResponse
    {
        /// <summary>
        /// Gets or sets the cloud games.
        /// </summary>
        /// <value>
        /// The cloud games.
        /// </value>
        [DataMember(Name = "cloudGames")]
        public List<AzureGameServicesProperty> AzureGameServicesProperties { get; set; }

        /// <summary>
        /// Gets or sets the sandboxes.
        /// </summary>
        /// <value>
        /// The sandboxes.
        /// </value>
        [DataMember(Name = "sandboxes")]
        public List<string> Sandboxes { get; set; }
    }
}
