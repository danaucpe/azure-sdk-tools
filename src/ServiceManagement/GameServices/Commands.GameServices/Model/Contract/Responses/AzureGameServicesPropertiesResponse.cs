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


namespace Microsoft.WindowsAzure.Commands.GameServices.Model.Contract
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Microsoft.WindowsAzure.Commands.GameServices.Model.Common;

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

        /// <summary>
        /// Gets or sets the cloud game platform resource type.
        /// </summary>
        /// <value>
        /// The platform.
        /// </value>
        [DataMember(Name = "platform")]
        public string PlatformResourceType {get; set; }

        /// <summary>
        /// Gets the cloud game platform.
        /// </summary>
        /// <value>
        /// The platform.
        /// </value>
        public CloudGamePlatform Platform
        {
            get
            {
                switch (PlatformResourceType.ToLower())
                {
                    case CloudGameUriElements.Xbox360ComputeResourceType:
                        return CloudGamePlatform.Xbox360;
                    case CloudGameUriElements.PcComputeResourceType:
                        return CloudGamePlatform.PC;
                    default:
                        return CloudGamePlatform.XboxOne;
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the sandboxes and cloudGames were successfully retrieved or not.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the results are only partially complete; otherwise, <c>false</c>.
        /// </value>
        [DataMember(Name = "partialResults")]
        public bool PartialResults { get; set; }

        /// <summary>
        /// Gets or sets the GSMS version.
        /// </summary>
        /// <value>
        /// The package version.
        /// </value>
        [DataMember(Name = "managementService")]
        public string ManagementService { get; set; }
    }
}
