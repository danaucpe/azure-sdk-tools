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

namespace Microsoft.WindowsAzure.Commands.Utilities.XblCompute.Contract
{
    using System.Runtime.Serialization;

    [DataContract]
    public class GeoRegionPoolInfo
    {
        /// <summary>
        /// Gets or sets the geo region.
        /// </summary>
        [DataMember(Name = "geoRegion")]
        public string GeoRegion { get; set; }

        /// <summary>
        /// Gets or sets the total.
        /// </summary>
        [DataMember(Name = "totalInstances")]
        public int TotalInstances { get; set; }

        /// <summary>
        /// Gets or sets the active instances.
        /// </summary>
        [DataMember(Name = "activeInstances")]
        public int ActiveInstances { get; set; }

        /// <summary>
        /// Gets or sets the standby instances.
        /// </summary>
        [DataMember(Name = "standbyInstances")]
        public int StandbyInstances { get; set; }

        /// <summary>
        /// Gets or sets the quarantined instances.
        /// </summary>
        [DataMember(Name = "quarantinedInstances")]
        public int QuarantinedInstances { get; set; }
    }
}
