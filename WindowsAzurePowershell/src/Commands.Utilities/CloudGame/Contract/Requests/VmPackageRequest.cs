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

using System;

namespace Microsoft.WindowsAzure.Commands.Utilities.CloudGame.Contract
{
    using System.Runtime.Serialization;

    [DataContract(Namespace = "")]
    public class VmPackageRequest
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [DataMember(Name = "name")]
        public string Name
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the developer facing file name of the gsi cspkg.
        /// </summary>
        /// <value>
        /// The name of the CSPKG file.
        /// </value>
        [DataMember(Name = "cspkgFileName")]
        public string CspkgFilename
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the developer facing file name of the gsi cscfg.
        /// </summary>
        /// <value>
        /// The name of the CSCFG file.
        /// </value>
        [DataMember(Name = "cscfgFileName")]
        public string CscfgFilename
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the asset Id to be associated with the VM package.
        /// </summary>
        /// <value>
        /// The Id of the asset file.
        /// </value>
        [DataMember(Name = "assetId")]
        public string AssetId
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets MinRequiredPlayers, which defines the minimum number of players needed in a session hosted by the GSI.
        /// </summary>
        /// <value>
        /// The min required players.
        /// </value>
        [DataMember(Name = "minRequiredPlayers")]
        public int MinRequiredPlayers
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets MaxAllowedPlayers, which defines how many players fits in a session hosted by the GSI.
        /// </summary>
        /// <value>
        /// The max allowed players.
        /// </value>
        [DataMember(Name = "maxRequiredPlayers")]
        public int MaxAllowedPlayers
        {
            get;
            set;
        }
    }
}
