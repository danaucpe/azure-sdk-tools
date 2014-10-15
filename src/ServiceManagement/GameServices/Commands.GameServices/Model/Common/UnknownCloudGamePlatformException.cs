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

namespace Microsoft.WindowsAzure.Commands.Utilities.CloudGame.Common
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// Exception thrown when a cloud game platform is unknown.
    /// </summary>
    [SerializableAttribute]
    public class UnknownCloudGamePlatformException : Exception
    {
        public UnknownCloudGamePlatformException(String unknownPlatformString)
        {
            UnknownPlatformString = unknownPlatformString;
        }

        protected UnknownCloudGamePlatformException(SerializationInfo info, StreamingContext context) :
            base(info, context)
        {
            // Used for deserialization
            this.UnknownPlatformString = info.GetString("UnknownPlatformString");
        }

        // GetObjectData performs a custom serialization.
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("UnknownPlatformString", UnknownPlatformString);

            base.GetObjectData(info, context);
        }

        /// <summary>
        /// Gets or sets the unknown (attempted) resource type string.
        /// </summary>
        /// <value>
        /// The unknown platform string.
        /// </value>
        public string UnknownPlatformString { get; set; }
    }
}
