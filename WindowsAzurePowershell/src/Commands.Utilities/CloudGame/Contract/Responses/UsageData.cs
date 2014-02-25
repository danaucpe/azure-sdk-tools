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
    using System.Runtime.Serialization;

    /// <summary>
    /// Cloud game usage data.
    /// </summary>
    [DataContract(Namespace = "")]
    public class UsageData
    {
        /// <summary>
        /// Gets or sets the active instance count.
        /// </summary>
        [DataMember(Name = "activeInstanceCount")]
        public int ActiveInstanceCount
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the standby instance count.
        /// </summary>
        [DataMember(Name = "standbyInstanceCount")]
        public int StandbyInstanceCount
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the quarantined instance count.
        /// </summary>
        [DataMember(Name = "quarantinedInstanceCount")]
        public int QuarantinedInstanceCount
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the log capacity.
        /// </summary>
        [DataMember(Name = "logCapacity")]
        public int LogCapacity
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the log usage.
        /// </summary>
        [DataMember(Name = "logUsage")]
        public int LogUsage
        {
            get;
            set;
        }
    }
}