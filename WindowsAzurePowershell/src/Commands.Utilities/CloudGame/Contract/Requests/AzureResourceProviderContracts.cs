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
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using System.Xml;
    using System.IO;

    /// <summary>
    /// Type representing identifier of Entity under subscription.
    /// </summary>
    [DataContract(Namespace = "http://schemas.datacontract.org/2004/07/Microsoft.Cis.DevExp.Services.Rdfe.ServiceManagement")]
    public class EntityId
    {
        [DataMember(EmitDefaultValue = false, IsRequired = true, Order = 0)]
        public string Id { get; set; }

        [DataMember(EmitDefaultValue = false, IsRequired = false, Order = 1)]
        public DateTime Created { get; set; }
    }

    /// <summary>
    /// State of entity.
    /// </summary>
    [DataContract(Namespace = "http://schemas.datacontract.org/2004/07/Microsoft.Cis.DevExp.Services.Rdfe.ServiceManagement")]
    public enum EntityState
    {
        [EnumMember]
        Deleted,

        [EnumMember]
        Enabled,

        [EnumMember]
        Disabled,

        [EnumMember]
        Migrated,

        [EnumMember]
        Updated,

        [EnumMember]
        Registered,

        [EnumMember]
        Unregistered
    }

    [DataContract(Namespace = "http://schemas.microsoft.com/windowsazure")]
    public class ResourceNameAvailabilityResponse
    {
        [DataMember(IsRequired = true, Order = 0)]
        public bool IsAvailable { get; set; }
    }

    [DataContract(Namespace = "http://schemas.datacontract.org/2004/07/Microsoft.Cis.DevExp.Services.Rdfe.ServiceManagement")]
    public class EntityEvent
    {
        [DataMember(EmitDefaultValue = false, IsRequired = true, Order = 0)]
        public string EventId { get; set; }

        [DataMember(EmitDefaultValue = false, Order = 1)]
        public string ListenerId { get; set; }

        [DataMember(EmitDefaultValue = false, Order = 2)]
        public string EntityType { get; set; }

        [DataMember(EmitDefaultValue = false, Order = 3)]
        public EntityState EntityState { get; set; }

        [DataMember(EmitDefaultValue = false, Order = 4)]
        public EntityId EntityId { get; set; }

        [DataMember(EmitDefaultValue = false, Order = 5)]
        public string OperationId { get; set; }

        [DataMember(EmitDefaultValue = false, Order = 6)]
        public bool IsAsync { get; set; }
    }


    /// <summary>
    /// The possible result from an operation.
    /// </summary>
    [DataContract(Namespace = "http://schemas.microsoft.com/windowsazure")]
    public enum OperationResult
    {
        [EnumMember]
        InProgress,

        [EnumMember]
        Succeeded,

        [EnumMember]
        Failed
    }

    /// <summary>
    /// Settings of the cloud service that are sent in the resource-level operations.
    /// </summary>
    [DataContract(Namespace = "http://schemas.microsoft.com/windowsazure")]
    public class CloudServiceSettings : IExtensibleDataObject
    {
        /// <summary>
        /// Gets or sets the geo region of the cloud service.
        /// </summary>
        [DataMember(IsRequired = true)]
        public string GeoRegion { get; set; }

        /// <summary>
        ///Gets or sets the user's email.
        /// </summary>
        [DataMember(IsRequired = false)]
        public string Email { get; set; }

        public ExtensionDataObject ExtensionData { get; set; }
    }

    /// <summary>
    /// Usage of a certain meter.
    /// For example, capacity (used/total), emails sent (sent/monthly limit), etc.
    /// </summary>
    [DataContract(Namespace = "http://schemas.microsoft.com/windowsazure")]
    public class UsageMeter : IExtensibleDataObject
    {
        /// <summary>
        /// Gets or sets the meter name.
        /// </summary>
        [DataMember(IsRequired = true)]
        public string Name { get; set; }

        /// <summary>
        /// Gets or set the unit of this meter.
        /// </summary>
        [DataMember(IsRequired = true)]
        public string Unit { get; set; }

        /// <summary>
        /// Gets or sets the included quantity.
        /// </summary>
        [DataMember(IsRequired = true)]
        public string Included { get; set; }

        /// <summary>
        /// Gets or sets the used quantity.
        /// </summary>
        [DataMember(IsRequired = true)]
        public string Used { get; set; }

        public ExtensionDataObject ExtensionData { get; set; }
    }

    /// <summary>
    /// List of usage meters, as returned by the resource provider.
    /// </summary>
    [CollectionDataContract(Name = "UsageMeters", Namespace = "http://schemas.microsoft.com/windowsazure")]
    public class UsageMeterCollection : List<UsageMeter>
    {
        public UsageMeterCollection()
        {
        }

        public UsageMeterCollection(IEnumerable<UsageMeter> meters)
            : base(meters)
        {
        }
    }

    [DataContract(Namespace = "http://schemas.microsoft.com/windowsazure")]
    public class OutputItem : IExtensibleDataObject
    {
        /// <summary>
        /// Gets or sets the key of the output item.
        /// </summary>
        [DataMember(IsRequired = true)]
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets the value of the output item.
        /// </summary>
        [DataMember(IsRequired = true)]
        public string Value { get; set; }

        public ExtensionDataObject ExtensionData { get; set; }
    }

    /// <summary>
    /// Class representing a list of key-value-pairs with the resource output.
    /// </summary>
    [CollectionDataContract(Name = "OutputItems", ItemName = "OutputItem", Namespace = "http://schemas.microsoft.com/windowsazure")]
    public class OutputItemList : List<OutputItem>
    {
        public OutputItemList()
        {
        }

        public OutputItemList(IEnumerable<OutputItem> outputs)
            : base(outputs)
        {
        }
    }

    /// <summary>
    /// Error information about a failed operation.
    /// </summary>
    [DataContract(Name = "Error", Namespace = "http://schemas.microsoft.com/windowsazure")]
    public class ErrorData : IExtensibleDataObject
    {
        /// <summary>
        /// Gets or sets the HTTP error code.
        /// </summary>
        [DataMember(IsRequired = true)]
        public int HttpCode { get; set; }

        /// <summary>
        /// Gets or sets the error message.
        /// </summary>
        [DataMember(IsRequired = true)]
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the extended code.
        /// </summary>
        [DataMember(IsRequired = false)]
        public string ExtendedCode { get; set; }

        public ExtensionDataObject ExtensionData { get; set; }
    }

    /// <summary>
    /// Status about an operation.
    /// </summary>
    [DataContract(Namespace = "http://schemas.microsoft.com/windowsazure")]
    public class OperationStatus : IExtensibleDataObject
    {
        /// <summary>
        /// Gets or sets the error code. This is not necessarily an integer.
        /// </summary>
        [DataMember(IsRequired = true)]
        public OperationResult Result { get; set; }

        /// <summary>
        /// Gets or sets the error information for an unhealthy resource.
        /// CS manager only passes this field to callers.
        /// </summary>
        [DataMember]
        public ErrorData Error { get; set; }

        public ExtensionDataObject ExtensionData { get; set; }
    }

    /// <summary>
    /// Resource information, as sent to the resource provider for the resource-level operations.
    /// </summary>
    [DataContract(Name = "Resource", Namespace = "http://schemas.microsoft.com/windowsazure")]
    public class ResourceInput : IExtensibleDataObject
    {
        /// <summary>
        /// The cloud service settings sent along with this resource input.
        /// </summary>
        [DataMember]
        public CloudServiceSettings CloudServiceSettings { get; set; }

        /// <summary>
        /// The label of the resource.
        /// </summary>
        [DataMember(IsRequired = false)]
        public string Label { get; set; }

        /// <summary>
        /// The type of the resource.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods", Justification = "Azure contract")]
        [DataMember]
        public string Type { get; set; }

        /// <summary>
        /// The plan of the resource.
        /// </summary>
        [DataMember(IsRequired = true)]
        public string Plan { get; set; }

        /// <summary>
        /// The etag of the resource.
        /// </summary>
        [DataMember(IsRequired = true)]
        public string ETag { get; set; }

        /// <summary>
        /// The schema version of the intrinsic settings.
        /// </summary>
        [DataMember(IsRequired = false)]
        public string SchemaVersion { get; set; }

        /// <summary>
        /// The intrinsic settings of the resource.
        /// The values and schema of this field are defined by the resource provider.
        /// </summary>
        [DataMember(IsRequired = false)]
        public XmlNode[] IntrinsicSettings { get; set; }

        /// <summary>
        /// The resource promotion code.
        /// </summary>
        [DataMember(IsRequired = false)]
        public string PromotionCode { get; set; }

        public ExtensionDataObject ExtensionData { get; set; }
    }

    /// <summary>
    /// Resource information, as returned by the resource provider.
    /// </summary>
    [DataContract(Name = "Resource", Namespace = "http://schemas.microsoft.com/windowsazure")]
    public class ResourceOutput : IExtensibleDataObject
    {
        /// <summary>
        /// The cloud service settings.
        /// This field is only filled in by the resource-level APIs (PUT/DELETE resource), not by the cloud-service-level APIs
        /// This is here, despite of not being used in the code, since, we want RPs to return the output as the same as input, and 
        /// we will have option to use it in the future.
        /// </summary>
        [DataMember]
        public CloudServiceSettings CloudServiceSettings { get; set; }

        /// <summary>
        /// The name of the resource. It is unique within a cloud service.
        /// This field is only filled in by the cloud-service-level APIs (PUT/GET/DELETE cloud service).
        /// In the resource-level APIs, the resource name is already present in the URI of the request.
        /// </summary>
        [DataMember]
        public string Name { get; set; }

        /// <summary>
        /// The type of the resource.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods", Justification = "Azure contract")]
        [DataMember]
        public string Type { get; set; }

        /// <summary>
        /// The plan of the resource.
        /// </summary>
        [DataMember(IsRequired = true)]
        public string Plan { get; set; }

        /// <summary>
        /// The etag of the resource.
        /// </summary>
        [DataMember(IsRequired = true)]
        public string ETag { get; set; }

        /// <summary>
        /// The schema version of the intrinsic settings.
        /// </summary>
        [DataMember(IsRequired = false)]
        public string SchemaVersion { get; set; }

        /// <summary>
        /// The intrinsic settings of the resource.
        /// The values and schema of this field are defined by the resource provider.
        /// </summary>
        [DataMember]
        public XmlNode[] IntrinsicSettings { get; set; }

        /// <summary>
        /// The resource promotion code. The resource provider is not required to return this field.
        /// The field is not returned to Portal nor used by RDFE. It is only defined here in case we decide to use it in the future.
        /// </summary>
        [DataMember]
        public string PromotionCode { get; set; }

        /// <summary>
        /// The output of of a resource, can be null.
        /// The values and schema of this field are defined by the resource provider.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "Azure contract file")]
        [DataMember]
        public OutputItemList OutputItems { get; set; }

        /// <summary>
        /// The state of the resource.
        /// </summary>
        [DataMember]
        public string State { get; set; }

        /// <summary>
        /// The sub-state of the resource. The possible values to this field are defined by the resource provider.
        /// </summary>
        [DataMember]
        public string SubState { get; set; }

        /// <summary>
        /// The usage meters of the resource. The specific meters are defined by the resource provider.
        /// This field is optional.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "Azure contract file")]
        [DataMember]
        public UsageMeterCollection UsageMeters { get; set; }

        /// <summary>
        /// Status about an operation on this resource.
        /// </summary>
        [DataMember(IsRequired = true)]
        public OperationStatus OperationStatus { get; set; }

        public ExtensionDataObject ExtensionData { get; set; }
    }

    /// <summary>
    /// List of resources, as returned by the resource provider.
    /// </summary>
    [CollectionDataContract(Name = "Resources", Namespace = "http://schemas.microsoft.com/windowsazure")]
    public class ResourceOutputCollection : List<ResourceOutput>
    {
        public ResourceOutputCollection()
        {
        }

        public ResourceOutputCollection(IEnumerable<ResourceOutput> resources)
            : base(resources)
        {
        }

        public ResourceOutput Find(string resourceType, string resourceName)
        {
            return this.Find(res =>
                string.Equals(res.Type, resourceType, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(res.Name, resourceName, StringComparison.OrdinalIgnoreCase));
        }
    }

    /// <summary>
    /// Cloud service information, as returned by the resource provider.
    /// </summary>
    [DataContract(Name = "CloudService", Namespace = "http://schemas.microsoft.com/windowsazure")]
    public class CloudServiceOutput : IExtensibleDataObject
    {
        /// <summary>
        /// The geo region of the cloud service.
        /// 
        /// This is here, despite of not being used in the code, since, we want RPs to return the output as the same as input, and 
        /// we will have option to use it in the future.
        /// </summary>
        [DataMember(IsRequired = true)]
        public string GeoRegion { get; set; }

        /// <summary>
        /// The resources of the cloud service.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "Azure contract file")]
        [DataMember(IsRequired = true)]
        public ResourceOutputCollection Resources { get; set; }

        public ExtensionDataObject ExtensionData { get; set; }
    }

    /// <summary>
    /// Class with the details to generate a token for single-sign-on.
    /// </summary>
    [DataContract(Namespace = "http://schemas.microsoft.com/windowsazure")]
    public class SsoToken : IExtensibleDataObject
    {
        /// <summary>
        /// The token.
        /// </summary>
        [DataMember(IsRequired = true)]
        public string Token { get; set; }

        /// <summary>
        /// Timestamp to indicate when the token was generated.
        /// </summary>
        [DataMember(IsRequired = true)]
        public string TimeStamp { get; set; }

        public ExtensionDataObject ExtensionData { get; set; }
    }

    /// <summary>
    /// The states that are reported by the resource provider.
    /// Resource providers should only return values in this enumeration, although that is not enforced.
    /// </summary>
    public enum ResourceState
    {
        /// <summary>
        /// The resource state is unknown because an error occurred when calling the resource provider.
        /// </summary>
        Unknown,

        /// <summary>
        /// The resource provider has no record of this resource.
        /// </summary>
        NotFound,

        /// <summary>
        /// The resource is started.
        /// </summary>
        Started,

        /// <summary>
        /// The resource is stopped.
        /// </summary>
        Stopped,

        /// <summary>
        /// The resource is paused.
        /// </summary>
        Paused,
    }

    [DataContract(Namespace = "http://schemas.microsoft.com/windowsazure")]
    public class ResourceProviderProperty : IExtensibleDataObject
    {
        /// <summary>
        /// Gets or sets the key of the property.
        /// </summary>
        [DataMember(IsRequired = true)]
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets the value of the property.
        /// </summary>
        [DataMember(IsRequired = true)]
        public string Value { get; set; }

        public ExtensionDataObject ExtensionData { get; set; }
    }

    /// <summary>
    /// Class representing a list of key-value-pairs with the resource provider.
    /// </summary>
    [CollectionDataContract(Name = "ResourceProviderProperties", ItemName = "ResourceProviderProperty", Namespace = "http://schemas.microsoft.com/windowsazure")]
    public class ResourceProviderProperties : List<ResourceProviderProperty>
    {
        public ResourceProviderProperties()
        {
        }

        public ResourceProviderProperties(IEnumerable<ResourceProviderProperty> properties)
            : base(properties)
        {
        }
    }

    [DataContract(Name = "Error", Namespace = "http://schemas.microsoft.com/windowsazure")]
    public class ResourceErrorInfo : IExtensibleDataObject
    {
        //public ResourceErrorInfo();

        [DataMember(Order = 3, EmitDefaultValue = false)]
        public string ExtendedCode { get; set; }
        public ExtensionDataObject ExtensionData { get; set; }
        [DataMember(Order = 1, EmitDefaultValue = false)]
        public int HttpCode { get; set; }
        [DataMember(Order = 2, EmitDefaultValue = false)]
        public string Message { get; set; }
    }

    [DataContract(Name = "OperationStatus", Namespace = "http://schemas.microsoft.com/windowsazure")]
    public class ResourceOperationStatus : IExtensibleDataObject
    {
        //public ResourceOperationStatus();

        [DataMember(Order = 3, EmitDefaultValue = false)]
        public ResourceErrorInfo Error { get; set; }
        public ExtensionDataObject ExtensionData { get; set; }
        [DataMember(Order = 2, EmitDefaultValue = false)]
        public string Result { get; set; }
        [DataMember(Order = 1, EmitDefaultValue = false)]
        public string Type { get; set; }
    }

    [CollectionDataContract(Name = "UsageMeters", Namespace = "http://schemas.microsoft.com/windowsazure")]
    public class UsageMeterList : List<UsageMeter>
    {
        //public UsageMeterList();
        //public UsageMeterList(IEnumerable<UsageMeter> meters);
    }

    [DataContract(Namespace = "http://schemas.microsoft.com/windowsazure")]
    public class Resource : IExtensibleDataObject
    {
        //public Resource();

        [DataMember(Order = 7, EmitDefaultValue = false)]
        public string ETag { get; set; }
        public ExtensionDataObject ExtensionData { get; set; }
        [DataMember(Order = 11, EmitDefaultValue = false)]
        public XmlNode[] IntrinsicSettings { get; set; }
        [DataMember(Order = 14, EmitDefaultValue = false)]
        public string Label { get; set; }
        [DataMember(Order = 3, EmitDefaultValue = false)]
        public string Name { get; set; }
        [DataMember(Order = 13, EmitDefaultValue = false)]
        public ResourceOperationStatus OperationStatus { get; set; }
        [DataMember(Order = 12, EmitDefaultValue = false)]
        public OutputItemList OutputItems { get; set; }
        [DataMember(Order = 4, EmitDefaultValue = false)]
        public string Plan { get; set; }
        [DataMember(Order = 5, EmitDefaultValue = false)]
        public string PromotionCode { get; set; }
        [DataMember(Order = 1, EmitDefaultValue = false)]
        public string ResourceProviderNamespace { get; set; }
        [DataMember(Order = 6, EmitDefaultValue = false)]
        public string SchemaVersion { get; set; }
        [DataMember(Order = 8, EmitDefaultValue = false)]
        public string State { get; set; }
        [DataMember(Order = 9, EmitDefaultValue = false)]
        public string SubState { get; set; }
        [DataMember(Order = 2, EmitDefaultValue = false)]
        public string Type { get; set; }
        [DataMember(Order = 10, EmitDefaultValue = false)]
        public UsageMeterList UsageMeters { get; set; }
    }

    [CollectionDataContract(Name = "Resources", ItemName = "Resource", Namespace = "http://schemas.microsoft.com/windowsazure")]
    public class ResourceList : List<Resource>
    {
    }

    [CollectionDataContract(Name = "CloudGames", ItemName = "CloudGame", Namespace = "http://schemas.microsoft.com/windowsazure")]
    public class CloudGameColletion : List<CloudGame>
    {
    }


    [DataContract(Namespace = "http://schemas.microsoft.com/windowsazure")]
    public class CloudService : IExtensibleDataObject
    {
        [DataMember(Order = 3, EmitDefaultValue = false)]
        public string Description { get; set; }
        public ExtensionDataObject ExtensionData { get; set; }
        [DataMember(Order = 4, EmitDefaultValue = false)]
        public string GeoRegion { get; set; }
        [DataMember(Order = 2, EmitDefaultValue = false)]
        public string Label { get; set; }
        [DataMember(Order = 1, EmitDefaultValue = false)]
        public string Name { get; set; }
        [DataMember(Order = 5, EmitDefaultValue = false)]
        public ResourceList Resources { get; set; }
    }

    /// <summary>
    /// The certificate contract.
    /// </summary>
    [DataContract(Namespace = "")]
    public sealed class GameCertificate
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        [DataMember(Name = "name")]
        public string Name
        {
            get;
            set;
        }

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
        }

        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        [DataMember(Name = "status")]
        public string Status
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
                return EntityTypeConstants.Certificate;
            }
        }

        /// <summary>
        /// Gets or sets the name of the file.
        /// </summary>
        [DataMember(Name = "fileName")]
        public string FileName
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        [DataMember(Name = "password")]
        public string Password
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the thumbprint.
        /// </summary>
        [DataMember(Name = "thumbprint")]
        public string Thumbprint
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the expires on.
        /// </summary>
        [DataMember(Name = "expiresOn")]
        public string ExpiresOn
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        [DataMember(Name = "id")]
        public string Id
        {
            get;
            set;
        }
    }

    [DataContract(Namespace = "")]
    public class HttpFile
    {
        [DataMember(Name = "FileName")]
        public string FileName
        {
            get;
            set;
        }

        [DataMember(Name = "InputStream")]
        public Stream InputStream
        {
            get;
            set;
        }
    }
}