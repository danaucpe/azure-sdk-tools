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
    using System.Runtime.Serialization;

    /// <summary>
    /// The cloud game definition.
    /// </summary>
    [DataContract(Namespace = "")]
    public class XblCompute
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        [DataMember(Name = "name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the display name.
        /// </summary>
        [DataMember(Name = "displayName")]
        public string DisplayName 
        { 
            get 
            { 
                return this.Name; 
            } 
            set{}
        }

        /// <summary>
        /// Gets or sets the status
        /// </summary>
        [DataMember(Name = "status")]
        public string Status { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the game can be deployed.
        /// </summary>
        [DataMember(Name = "canDeploy")]
        public bool CanDeploy { get; set; }

        /// <summary>
        /// Gets or sets the subscription id.
        /// </summary>
        [DataMember(Name = "subscriptionId")]
        public string SubscriptionId { get; set; }

        /// <summary>
        /// Gets or sets the resource sets.
        /// </summary>
        /// <value>
        /// The resource sets.
        /// </value>
        [DataMember(Name = "resourceSets")]
        public string ResourceSets { get; set; }

        /// <summary>
        /// Gets or sets the sandboxes.
        /// </summary>
        /// <value>
        /// The sandboxes.
        /// </value>
        [DataMember(Name = "sandboxes")]
        public string Sandboxes { get; set; }

        /// <summary>
        /// Gets or sets the schema id.
        /// </summary>
        /// <value>
        /// The schema id.
        /// </value>
        [DataMember(Name = "schemaId")]
        public string SchemaId { get; set; }

        /// <summary>
        /// Gets or sets the name of the schema.
        /// </summary>
        /// <value>
        /// The name of the schema.
        /// </value>
        [DataMember(Name = "schemaName")]
        public string SchemaName { get; set; }

        /// <summary>
        /// Gets or sets the gsi set id.
        /// </summary>
        /// <value>
        /// The gsi set id.
        /// </value>
        [DataMember(Name = "gsiSetId")]
        public string GsiSetId { get; set; }

        /// <summary>
        /// Gets or sets the title id.
        /// </summary>
        /// <value>
        /// The title id.
        /// </value>
        [DataMember(Name = "titleId")]
        public string TitleId { get; set; }

        /// <summary>
        /// Gets or sets the gsi set id.
        /// </summary>
        [DataMember(Name = "publisherId")]
        public string PublisherId { get; set; }

        /// <summary>
        /// Gets or sets the selection order.
        /// </summary>
        /// <value>
        /// The selection order.
        /// </value>
        [DataMember(Name = "selectionOrder")]
        public int SelectionOrder { get; set; }

        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        [DataMember(Name = "type")]
        public string type 
        { 
            get 
            { 
                return EntityTypeConstants.XblCompute; 
            } 
            set{}
        }

        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        [DataMember(Name = "id")]
        public string id
        {
            get
            {
                return this.GsiSetId;
            }
            set{}
        }

        /// <summary>
        /// Gets or sets the InErrorState
        /// </summary>
        public bool InErrorState { get; set; }
    }
}
