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

namespace Microsoft.WindowsAzure.Commands.Utilities.XblCompute.Contract
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    /// <summary>
    /// The cloud game image
    /// </summary>
    [DataContract(Namespace = "")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Used by JavaScriptSerializer")]
    public class XblPackage
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [DataMember(Name = "name")]
        public string PackageName
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the min players.
        /// </summary>
        /// <value>
        /// The max players.
        /// </value>
        [DataMember(Name = "minRequiredPlayers")]
        public string MinRequiredPlayers
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the max players.
        /// </summary>
        /// <value>
        /// The max players.
        /// </value>
        [DataMember(Name = "maxRequiredPlayers")]
        public string MaxRequiredPlayers
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the id of the package
        /// </summary>
        /// <value>
        /// The id.
        /// </value>
        [DataMember(Name = "gsiId")]
        public string Id
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the GsiSetId.
        /// </summary>
        /// <value>
        /// The id.
        /// </value>
        [DataMember(Name = "gsiSetId")]
        public string GsiSetId
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the name of the package file.
        /// </summary>
        [DataMember(Name = "cspkgFileName")]
        public string CspkgFileName
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the name of the package config file.
        /// </summary>
        [DataMember(Name = "cscfgFileName")]
        public string CscfgFileName
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the Ids of the package asset file(s).
        /// </summary>
        [DataMember(Name = "gsiAssetIds")]
        public List<string> AssetIds
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the Ids of the package certificate(s).
        /// </summary>
        [DataMember(Name = "certificateIds")]
        public List<string> CertificateIds
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        [DataMember(Name = "type")]
        public string Type
        {
            get
            {
                return EntityTypeConstants.Package;
            }
            set{ }
        }
    }
}
