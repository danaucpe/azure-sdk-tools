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
    public class PackageDeploymentInfo
    {
        /// <summary>
        /// Gets or sets the game server image Id.
        /// </summary>
        [DataMember(Name = "gameServerImageId")]
        public string GameServerImageId { get; set; }

        /// <summary>
        /// Gets or sets the sandbox.
        /// </summary>
        [DataMember(Name = "sandbox")]
        public string Sandbox { get; set; }

        /// <summary>
        /// Gets or sets the total progress.
        /// </summary>
        [DataMember(Name = "totalProgress")]
        public int TotalProgress { get; set; }

        /// <summary>
        /// Gets or sets the total usage.
        /// </summary>
        [DataMember(Name = "totalUsage")]
        public int TotalUsage { get; set; }

        /// <summary>
        /// Gets or sets the total standingby.
        /// </summary>
        [DataMember(Name = "totalStandingBy")]
        public int TotalStandingBy { get; set; }

        /// <summary>
        /// Gets or sets the total active.
        /// </summary>
        [DataMember(Name = "totalActive")]
        public int TotalActive { get; set; }

        /// <summary>
        /// Gets or sets the total quarantined.
        /// </summary>
        [DataMember(Name = "totalQuarantined")]
        public int TotalQuarantined { get; set; }

        /// <summary>
        /// Gets or sets the pools.
        /// </summary>
        [DataMember(Name = "geoRegionInfos")]
        public List<GeoRegionDeploymentInfo> GeoRegionInfos { get; set; }
    }
}