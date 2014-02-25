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
    /// The cloud game definition.
    /// </summary>
    [DataContract(Namespace = "")]
    public class CloudGame
    {
        private string platformLower;

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
        /// Gets or sets the variant IDs for this game.
        /// </summary>
        [DataMember(Name = "variants")]
        public string[] gameModeIds { get; set; }

        /// <summary>
        /// Gets or sets the VM package IDs for this game.
        /// </summary>
        [DataMember(Name = "gsiIds")]
        public string[] vmPackageIds { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the game can be deployed.
        /// </summary>
        [DataMember(Name = "canDeploy")]
        public bool CanDeploy { get; set; }

        /// <summary>
        /// Gets or sets the subscription Id.
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
        /// Gets or sets the schema Id.
        /// </summary>
        /// <value>
        /// The schema Id.
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
        /// Gets or sets the cloud game Id.
        /// </summary>
        /// <value>
        /// The gsi set Id.
        /// </value>
        [DataMember(Name = "gsiSetId")]
        public string CloudGameId { get; set; }

        /// <summary>
        /// Gets or sets the title Id.
        /// </summary>
        /// <value>
        /// The title Id.
        /// </value>
        [DataMember(Name = "titleId")]
        public string TitleId { get; set; }

        /// <summary>
        /// Gets or sets the gsi set Id.
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
        /// Gets or sets the platform of the cloud game.
        /// </summary>
        [DataMember(Name = "platform")]
        public string Platform
        {
            get
            {
                return this.platformLower;
            }

            set
            {
                this.platformLower = value == null ? null : value.ToLower();
            }
        }

        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        [DataMember(Name = "type")]
        public string Type
        { 
            get 
            { 
                return EntityTypeConstants.CloudGame; 
            } 
            set{}
        }

        /// <summary>
        /// Gets or sets the Id.
        /// </summary>
        [DataMember(Name = "id")]
        public string Id
        {
            get
            {
                return this.CloudGameId;
            }
            set{}
        }

        /// <summary>
        /// Gets or sets the InErrorState
        /// </summary>
        public bool InErrorState { get; set; }
    }
}
