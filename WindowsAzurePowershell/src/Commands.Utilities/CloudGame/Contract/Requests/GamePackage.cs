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
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    /// <summary>
    /// The game package
    /// </summary>
    [DataContract(Namespace = "")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Used by JavaScriptSerializer")]
    public class GamePackage
    {

        /// <summary>
        /// Gets or sets the Id of the game package
        /// </summary>
        /// <value>
        /// The Id.
        /// </value>
        [DataMember(Name = "codeFileId")]
        public string GamePackageId
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the friendly name.
        /// </summary>
        /// <value>
        /// The friendly name.
        /// </value>
        [DataMember(Name = "name")]
        public string Name
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the name of the file.
        /// </summary>
        /// <value>
        /// The name of the file.
        /// </value>
        [DataMember(Name = "fileName")]
        public string FileName { get; set; }

        /// <summary>
        /// Gets or Sets whether the code file is active
        /// </summary>
        [DataMember(Name = "active")]
        public bool Active
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
                return EntityTypeConstants.GamePackage;
            }
            set { }
        }
    }
}