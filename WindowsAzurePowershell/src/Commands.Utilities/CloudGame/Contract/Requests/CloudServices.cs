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
    using System;
    using System.Runtime.Serialization;
    using System.Xml;

    /// <summary>
    /// The cloud game definition.
    /// </summary>
    //[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Used by JavaScriptSerializer")]
    [DataContract(Namespace = "")]
    public class CloudServices
    {
        /// <summary>
        /// Gets or sets the default service name.
        /// </summary>
        [DataMember(Name = "Name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the service description.
        /// </summary>
        [DataMember(Name = "Description")]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the preferred region
        /// </summary>
        [DataMember(Name = "GeoRegion")]
        public string GeoRegion { get; set; }

        /// <summary>
        /// Gets or sets the service lable.
        /// </summary>
        [DataMember(Name = "Label")]
        public string Label { get; set; }
    }
}

