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

namespace Microsoft.WindowsAzure.Commands.Utilities.CloudGame.Contract
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    [DataContract]
    public class PoolData
    {
        /// <summary>
        /// Gets or sets the total.
        /// </summary>
        [DataMember(Name = "total")]
        public int Total { get; set; }

        /// <summary>
        /// Gets or sets the total active.
        /// </summary>
        [DataMember(Name = "totalActive")]
        public int TotalActive { get; set; }

        /// <summary>
        /// Gets or sets the total stand by.
        /// </summary>
        [DataMember(Name = "totalStandBy")]
        public int TotalStandBy { get; set; }

        /// <summary>
        /// Gets or sets the total quarantined.
        /// </summary>
        [DataMember(Name = "totalQuarantined")]
        public int TotalQuarantined { get; set; }

        /// <summary>
        /// Gets or sets the pools.
        /// </summary>
        [DataMember(Name = "pools")]
        public List<GeoRegionPoolInfo> Pools { get; set; }
    }
}
