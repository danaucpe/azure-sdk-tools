﻿// ----------------------------------------------------------------------------------
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

namespace Microsoft.WindowsAzure.Commands.Utilities.CloudGame
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Json;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Xml;
    using System.Xml.Linq;
    using System.Xml.Serialization;
    using Common;
    using Contract;
    using Websites.Services;
    using ServiceManagement;
    using Newtonsoft.Json;
    using Management.Compute.Models;
    using Utilities.Common;

    /// <summary>
    ///     Implements helper functions used by the ClientGameClient
    /// </summary>
    public class ClientHelper
    {
        /// <summary>
        /// The general information about the game services cmdlets.
        /// </summary>
        public static readonly GameServicesCmdletsInfo Info = new GameServicesCmdletsInfo("2014_07_v1");

        private static readonly Dictionary<CloudGamePlatform, string> PlatformMapping = new Dictionary<CloudGamePlatform, string>
        {
            { CloudGamePlatform.XboxOne, CloudGameUriElements.XboxOneComputeResourceType },
            { CloudGamePlatform.Xbox360, CloudGameUriElements.Xbox360ComputeResourceType },
            { CloudGamePlatform.PC,      CloudGameUriElements.PcComputeResourceType }
        };

        private static readonly Dictionary<string, CloudGamePlatform> ReversePlatformMapping = PlatformMapping.ToDictionary(x => x.Value, x => x.Key);

        /// <summary>
        /// The regex used for cloud game names.
        /// </summary>
        public const string CloudGameNameRegex = @"^[a-zA-Z0-9._\-]{1,99}$";

        /// <summary>
        /// The regex used for items like asset names, certificate names, etc.
        /// </summary>
        public const string ItemNameRegex = @"^[a-zA-Z0-9._\-]([a-zA-Z0-9._\- ]{0,97}[a-zA-Z0-9._\-])?$";

        /// <summary>
        /// The sandbox regex.
        /// </summary>
        public const string SandboxRegex = @"[a-zA-Z0-9 .\-]{1,50}";

        /// <summary>
        /// Gets the platform resource type string from an enum.
        /// </summary>
        /// <param name="platform">The cloud game platform enum.</param>
        /// <returns>The resource type string.</returns>
        public static string GetPlatformResourceTypeString(CloudGamePlatform platform)
        {
            return PlatformMapping[platform];
        }

        /// <summary>
        /// Gets the platform from a cloud game resource type string.
        /// </summary>
        /// <param name="cloudGameResourceType">Cloud game resource type string.</param>
        /// <returns>The cloud game platform enum.</returns>
        public static CloudGamePlatform GetPlatformEnum(string cloudGameResourceType)
        {
            var resourceType = cloudGameResourceType.ToLower();
            if (ReversePlatformMapping.ContainsKey(resourceType))
            {
                return ReversePlatformMapping[resourceType];
            }

            throw new UnknownCloudGamePlatformException(resourceType);
        }

        public static async Task RegisterAndCreateContainerResourceIfNeeded(HttpClient httpJsonClient, HttpClient httpXmlClient)
        {
            // Check registration.
            var url = httpJsonClient.BaseAddress + String.Format("/services?service=gameservices." + CloudGameUriElements.ContainerResourceType + "&action=register");
            var responseMessage = await httpJsonClient.PutAsync(url, null).ConfigureAwait(false);
            ProcessBooleanJsonResponseAllowConflict(responseMessage);

            // Check if the container already exists
            var isNotCreated = await CheckContainerResourceNameAvailability(httpJsonClient, httpXmlClient, CloudGameUriElements.DefaultContainerName);

            if (isNotCreated)
            {
                // If not, create it
                await CreateCloudService(httpJsonClient, httpXmlClient);

                var resource = new Resource()
                {
                    Name = CloudGameUriElements.DefaultContainerName,
                    ETag = Guid.NewGuid().ToString(),
                    Plan = string.Empty,
                    ResourceProviderNamespace = CloudGameUriElements.NamespaceName,
                    Type = CloudGameUriElements.ContainerResourceType,
                    SchemaVersion = CloudGameUriElements.SchemaVersion
                };

                url = httpJsonClient.BaseAddress + String.Format(CloudGameUriElements.ContainerResourcePath);

                responseMessage = await httpXmlClient.PutAsXmlAsync(url, resource).ConfigureAwait(false);
                if (!responseMessage.IsSuccessStatusCode &&
                    responseMessage.StatusCode != HttpStatusCode.BadRequest)
                {
                    // Error result, so throw an exception
                    throw new ServiceManagementClientException(responseMessage.StatusCode,
                        new ServiceManagementError
                        {
                            Code = responseMessage.StatusCode.ToString()
                        },
                        string.Empty);
                }
            }
        }

        private static async Task<bool> CheckContainerResourceNameAvailability(HttpClient httpJsonClient, HttpClient httpXmlClient, string containerName)
        {
            var url = httpJsonClient.BaseAddress + String.Format(CloudGameUriElements.CheckContainerNameAvailabilityPath, containerName);
            var responseMessage = await httpXmlClient.GetAsync(url).ConfigureAwait(false);
            if (!responseMessage.IsSuccessStatusCode)
            {
                // The cloud service has not been created yet.
                if (responseMessage.StatusCode == HttpStatusCode.NotFound)
                {
                    return true;
                }

                // Error result, so throw an exception
                throw new ServiceManagementClientException(responseMessage.StatusCode,
                    new ServiceManagementError
                    {
                        Code = responseMessage.StatusCode.ToString()
                    },
                    string.Empty);
            }

            var nameAvailability = await ProcessXmlResponse<ResourceNameAvailabilityResponse>(responseMessage).ConfigureAwait(false);
            return nameAvailability.IsAvailable;
        }

        public static async Task RegisterCloudService(HttpClient httpJsonClient, HttpClient httpXmlClient, string resourceType)
        {
            // Check registration.
            var url = httpJsonClient.BaseAddress + String.Format("/services?service=gameservices." + resourceType + "&action=register");
            var responseMessage = await httpJsonClient.PutAsync(url, null).ConfigureAwait(false);
            ProcessBooleanJsonResponseAllowConflict(responseMessage);

            await CreateCloudService(httpJsonClient, httpXmlClient);
        }

        private static async Task CreateCloudService(HttpClient httpJsonClient, HttpClient httpXmlClient)
        {
            // See if the cloud service exists, and create it if it does not.
            var url = httpXmlClient.BaseAddress + String.Format(CloudGameUriElements.CloudServiceResourcePath);
            var responseMessage = await httpXmlClient.GetAsync(url).ConfigureAwait(false);
            CloudService existingCloudService = null;
            if (responseMessage.IsSuccessStatusCode)
            {
                existingCloudService = await ProcessXmlResponse<CloudService>(responseMessage).ConfigureAwait(false);
            }
            else if (responseMessage.StatusCode != HttpStatusCode.NotFound)
            {
                // Error result, so throw an exception
                throw new ServiceManagementClientException(responseMessage.StatusCode,
                    new ServiceManagementError
                    {
                        Code = responseMessage.StatusCode.ToString()
                    },
                    string.Empty);
            }

            // If the cloud service exists, and it has the DefaultServiceName, then no more work is needed
            if ((existingCloudService != null) && existingCloudService.Name.Equals(CloudGameUriElements.DefaultServiceName))
            {
                return;
            }

            // The service does not exists, so create it
            var newCloudService = new CloudService()
            {
                Name = CloudGameUriElements.DefaultServiceName,
                Description = CloudGameUriElements.DefaultServiceName,
                GeoRegion = "West US",
                Label = CloudGameUriElements.DefaultServiceName
            };

            responseMessage = await httpXmlClient.PutAsXmlAsync(url, newCloudService).ConfigureAwait(false);
            if (!responseMessage.IsSuccessStatusCode)
            {
                // Error result, so throw an exception
                throw new ServiceManagementClientException(
                    responseMessage.StatusCode,
                    new ServiceManagementError { Code = responseMessage.StatusCode.ToString() },
                    string.Empty);
            }
        }

        /// <summary>
        ///     Processes the response and handle error cases.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="responseMessage">The response message.</param>
        /// <returns></returns>
        /// <exception cref="Microsoft.WindowsAzure.ServiceManagement.ServiceManagementClientException"></exception>
        /// <exception cref="ServiceManagementError"></exception>
        public static async Task<T> ProcessJsonResponse<T>(HttpResponseMessage responseMessage)
        {
            var content = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);

            if (responseMessage.IsSuccessStatusCode)
            {
                return (T)JsonConvert.DeserializeObject(content, typeof(T));
            }

            if (responseMessage.StatusCode == HttpStatusCode.NotFound)
            {
                return default(T); // returns null for objects, 0 for int, '\0' for char
            }

            throw CreateExceptionFromJson(responseMessage);
        }

        public static bool ProcessBooleanJsonResponseAllowConflict(HttpResponseMessage responseMessage)
        {
            if (responseMessage.IsSuccessStatusCode || (responseMessage.StatusCode == HttpStatusCode.Conflict))
            {
                return true;
            }

            // Error
            throw CreateExceptionFromJson(responseMessage);
        }

        public static async Task<CloudGameColletion> ProcessCloudServiceResponse(ICloudGameClient client, HttpResponseMessage responseMessage)
        {
            var content = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
            var encoding = GetEncodingFromResponseMessage(responseMessage);
            var response = new CloudGameColletion();
            var deleteTasks = new List<Task>();

            if (responseMessage.IsSuccessStatusCode)
            {
                var ser = new DataContractSerializer(typeof(CloudService));
                using (var stream = new MemoryStream(encoding.GetBytes(content)))
                {
                    stream.Position = 0;
                    var reader = XmlDictionaryReader.CreateTextReader(stream, new XmlDictionaryReaderQuotas());
                    var serviceResponse = (CloudService)ser.ReadObject(reader, true);

                    foreach (var resource in serviceResponse.Resources)
                    {
                        if (!CloudGameUriElements.CloudGameResourceTypes.Contains(resource.Type.ToLower()))
                        {
                            // Skip anything that isn't a cloud game
                            continue;
                        }

                        CloudGame cloudGame;

                        // If there are no intrinsic settings, or the intrinsic setting is null, then attempt to fetch info directly from GSRM.
                        if (resource.IntrinsicSettings == null || 
                            resource.IntrinsicSettings.Length == 0 ||
                            resource.IntrinsicSettings[0] == null)
                        {
                            // Fetch missing info for this game from the GSRM passthrough endpoint
                            cloudGame = await client.GetCloudGame(resource.Name, GetPlatformEnum(resource.Type));
                            if (cloudGame == null)
                            {
                                // The GSRM does not know about this resource, so attempt to delete it from RDFE silently
                                deleteTasks.Add(client.RemoveCloudGame(resource.Name, GetPlatformEnum(resource.Type), false));
                                continue;
                            }
                        }
                        else
                        {
                            var cbData = resource.IntrinsicSettings[0] as XmlCDataSection;
                            var jsonSer = new DataContractJsonSerializer(typeof(CloudGame));
                            using (var jsonStream = new MemoryStream(Encoding.UTF8.GetBytes((cbData.Data))))
                            {
                                // Deserialize the result from GSRM
                                cloudGame = (CloudGame)jsonSer.ReadObject(jsonStream);

                                // Set the error state if applicable
                                cloudGame.Error = resource.OperationStatus.Error;

                            }
                        }

                        // Add the cloud game instance to the collection
                        response.Add(cloudGame);
                    }
                }

                try
                {
                    await TaskEx.WhenAll(deleteTasks);
                }
                catch (ServiceManagementClientException)
                {
                }

                return response;
            }

            throw CreateExceptionFromXml(content, responseMessage);
        }

        public static async Task<T> ProcessXmlResponse<T>(HttpResponseMessage responseMessage)
        {
            var content = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
            var encoding = GetEncodingFromResponseMessage(responseMessage);

            if (responseMessage.IsSuccessStatusCode)
            {
                var ser = new DataContractSerializer(typeof(T));
                using (var stream = new MemoryStream(encoding.GetBytes(content)))
                {
                    stream.Position = 0;
                    var reader = XmlDictionaryReader.CreateTextReader(stream, new XmlDictionaryReaderQuotas());
                    return (T)ser.ReadObject(reader, true);
                }
            }

            throw CreateExceptionFromXml(content, responseMessage);
        }

        public static Encoding GetEncodingFromResponseMessage(HttpResponseMessage message)
        {
            var encodingString = message.Content.Headers.ContentType.CharSet;
            var encoding = Encoding.GetEncoding(encodingString);
            return encoding;
        }

        public static ServiceManagementClientException CreateExceptionFromXml(string content, HttpResponseMessage message)
        {
            var encoding = GetEncodingFromResponseMessage(message);

            using (var stream = new MemoryStream(encoding.GetBytes(content)))
            {
                stream.Position = 0;
                var serializer = new XmlSerializer(typeof(ServiceError));
                var serviceError = (ServiceError)serializer.Deserialize(stream);
                return new ServiceManagementClientException(
                    message.StatusCode,
                    new ServiceManagementError
                    {
                        Code = message.StatusCode.ToString(),
                        Message = serviceError.Message
                    },
                    string.Empty);
            }
        }

        /// <summary>
        ///     Unwraps error message and creates ServiceManagementClientException.
        /// </summary>
        public static ServiceManagementClientException CreateExceptionFromJson(HttpStatusCode statusCode, string message)
        {
            var exception = new ServiceManagementClientException(
                statusCode,
                new ServiceManagementError
                {
                    Code = statusCode.ToString(),
                    Message = message
                },
                string.Empty);

            return exception;
        }

        /// <summary>
        ///     Unwraps error message and creates ServiceManagementClientException.
        /// </summary>
        public static ServiceManagementClientException CreateExceptionFromJson(HttpResponseMessage httpResponse)
        {
            // See if there is any detailed information we can extract from the HTTP response object to aid the user in determining the cause
            string contentString;
            ErrorResponse messageDetails;

            try
            {
                contentString = httpResponse.Content != null ? (httpResponse.Content.ReadAsAsync<string>().Result) : null;
            }
            catch (Exception ex)
            {
                // Defensively parse in case this data is missing/malformed
                if ((ex is AggregateException && ex.InnerException is XmlException) || ex is XmlException)
                {
                    contentString = null;
                }
                else
                {
                    throw;
                }
            }

            try
            {
                messageDetails = contentString != null ? (JsonConvert.DeserializeObject(contentString, typeof (ErrorResponse)) as ErrorResponse) : null;
            }
            catch (JsonReaderException)
            {
                messageDetails = null;
            }

            var exception = new ServiceManagementClientException(
                httpResponse.StatusCode,
                new ServiceManagementError
                {
                    Code = httpResponse.ReasonPhrase,
                    Message = messageDetails != null ? messageDetails.ExtendedCode : string.Empty
                },
                string.Empty);

            return exception;
        }

        /// <summary>
        /// Polls an asynchronous operation status asynchronously.
        /// See http://msdn.microsoft.com/en-us/library/windowsazure/ee460783.aspx
        /// </summary>
        /// <param name="initialHttpResponse">The initial "accepted" response to perform the operation.</param>
        /// <param name="httpXmlClient">The XML HTTP client.</param>
        /// <param name="pollIntervalInSeconds">The poll interval in seconds.</param>
        /// <param name="timeoutInSeconds">The timeout in seconds.</param>
        /// <param name="logger">The logger.</param>
        /// <returns>
        /// The task for completion.
        /// </returns>
        /// <exception cref="ServiceManagementClientException">If the initial response code is not 202 Accepted.</exception>
        /// <exception cref="System.ArgumentException">If there is no request ID header found in initial response</exception>
        public static async Task<ComputeOperationStatusResponse> PollOperationStatus(
            HttpResponseMessage initialHttpResponse,
            HttpClient httpXmlClient,
            int pollIntervalInSeconds,
            int timeoutInSeconds,
            Action<string> logger = null)
        {
            if (initialHttpResponse.StatusCode != HttpStatusCode.Accepted)
            {
                // Unexpected result, so throw an exception
                throw new ServiceManagementClientException(
                    initialHttpResponse.StatusCode,
                    new ServiceManagementError { Code = initialHttpResponse.StatusCode.ToString(), Message = "Unexpected status code. Should be 202 Accepted." },
                    string.Empty);
            }

            if (!initialHttpResponse.Headers.Contains(CloudGameUriElements.RequestIdHeader))
            {
                throw new ArgumentException("PollOperationStatus: No request ID header found in initial response");
            }

            var requestId = initialHttpResponse.Headers.GetValues(CloudGameUriElements.RequestIdHeader).FirstOrDefault();

            // Construct URL
            var url = httpXmlClient.BaseAddress + string.Format(CloudGameUriElements.OperationStatusPath, requestId);

            // Poll for a result
            var beginPollTime = DateTime.UtcNow;
            var pollInterval = new TimeSpan(0, 0, pollIntervalInSeconds);
            var endPollTime = beginPollTime + new TimeSpan(0, 0, timeoutInSeconds);
            ComputeOperationStatusResponse result = null;

            var done = false;
            while (!done)
            {
                var httpResponse = await httpXmlClient.GetAsync(url).ConfigureAwait(false);
                var xmlString = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);

                result = ParseOperationStatusResponse(xmlString);
                result.StatusCode = httpResponse.StatusCode;
                if (httpResponse.Headers.Contains(CloudGameUriElements.RequestIdHeader))
                {
                    result.RequestId = httpResponse.Headers.GetValues(CloudGameUriElements.RequestIdHeader).FirstOrDefault();
                }

                switch (result.Status)
                {
                    case Management.Compute.Models.OperationStatus.InProgress:
                        Thread.Sleep((int)pollInterval.TotalMilliseconds);
                        break;

                    case Management.Compute.Models.OperationStatus.Failed:
                        throw new ServiceManagementClientException(
                        result.HttpStatusCode,
                        new ServiceManagementError
                        {
                            Code = result.Error.Code,
                            Message = result.Error.Message
                        },
                        string.Empty);

                    case Management.Compute.Models.OperationStatus.Succeeded:
                        done = true;
                        break;
                }

                if (!done && DateTime.UtcNow > endPollTime)
                {
                    if (logger != null)
                    {
                        logger("Operation status polling timed out after " + timeoutInSeconds + " seconds");
                    }

                    done = true;
                }
            }

            return result;
        }

        private static ComputeOperationStatusResponse ParseOperationStatusResponse(string xmlString)
        {
            // Note a helper method is used to parse the XML response because the endpoint is not a WCF service and does not use a standard data contract
            var result = new ComputeOperationStatusResponse();
            var responseDoc = XDocument.Parse(xmlString);

            var operationElement = responseDoc.Element(XName.Get("Operation", "http://schemas.microsoft.com/windowsazure"));
            if (operationElement != null)
            {
                var idElement = operationElement.Element(XName.Get("ID", "http://schemas.microsoft.com/windowsazure"));
                if (idElement != null)
                {
                    var idInstance = idElement.Value;
                    result.Id = idInstance;
                }

                var statusElement = operationElement.Element(XName.Get("Status", "http://schemas.microsoft.com/windowsazure"));
                if (statusElement != null)
                {
                    var statusInstance = (Management.Compute.Models.OperationStatus)Enum.Parse(typeof(Management.Compute.Models.OperationStatus), statusElement.Value, false);
                    result.Status = statusInstance;
                }

                var httpStatusCodeElement = operationElement.Element(XName.Get("HttpStatusCode", "http://schemas.microsoft.com/windowsazure"));
                if (httpStatusCodeElement != null)
                {
                    var httpStatusCodeInstance = (HttpStatusCode)Enum.Parse(typeof(HttpStatusCode), httpStatusCodeElement.Value, false);
                    result.HttpStatusCode = httpStatusCodeInstance;
                }

                var errorElement = operationElement.Element(XName.Get("Error", "http://schemas.microsoft.com/windowsazure"));
                if (errorElement != null)
                {
                    var errorInstance = new ComputeOperationStatusResponse.ErrorDetails();
                    result.Error = errorInstance;

                    var codeElement = errorElement.Element(XName.Get("Code", "http://schemas.microsoft.com/windowsazure"));
                    if (codeElement != null)
                    {
                        var codeInstance = codeElement.Value;
                        errorInstance.Code = codeInstance;
                    }

                    var messageElement = errorElement.Element(XName.Get("Message", "http://schemas.microsoft.com/windowsazure"));
                    if (messageElement != null)
                    {
                        var messageInstance = messageElement.Value;
                        errorInstance.Message = messageInstance;
                    }
                }
            }

            return result;
        }

        /// <summary>
        ///     Creates and initialize and instance of HttpClient for a specific media type
        /// </summary>
        /// <returns></returns>
        public static HttpClient CreateCloudGameHttpClient(WindowsAzureSubscription subscription, string mediaType, Action<string> logger)
        {
            if (subscription.Certificate == null)
            {
                throw new ArgumentException("The selected Azure Subscription must have an associated certificate in order to authenticate");
            }

            var requestHandler = new WebRequestHandler();
            requestHandler.ClientCertificates.Add(subscription.Certificate);
            var retryHandler = new RetryHttpHandler(requestHandler, logger);
            var endpoint = new StringBuilder(General.EnsureTrailingSlash(subscription.ServiceEndpoint.ToString()));
            endpoint.Append(subscription.SubscriptionId);

            var client = HttpClientHelper.CreateClient(endpoint.ToString(), handler: retryHandler);
            client.DefaultRequestHeaders.Add(CloudGameUriElements.XblCorrelationHeader, Guid.NewGuid().ToString());
            client.DefaultRequestHeaders.Add(CloudGameUriElements.GameServicesCmdletVersionHeader, Info.Version);
            client.DefaultRequestHeaders.Add(Constants.VersionHeaderName, "2013-11-01");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(mediaType));
            client.Timeout = TimeSpan.FromMilliseconds((client.Timeout.TotalMilliseconds + retryHandler.DeplayBetweenTriesMs) * retryHandler.MaxTries);
            return client;
        }

        /// <summary>
        /// Write out object to JSON string.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">The value.</param>
        /// <returns>A string of JSON</returns>
        public static string ToJson<T>(T value)
        {
            var serializer = new DataContractJsonSerializer(typeof(T));
            using (var stream = new MemoryStream())
            {
                serializer.WriteObject(stream, value);
                return Encoding.UTF8.GetString(stream.GetBuffer(), 0, (int)stream.Length);
            }
        }

        public static TResult DeserializeJsonToObject<TResult>(string json)
        {
            var output = default(TResult);
            using (var mstream = new MemoryStream(Encoding.UTF8.GetBytes(json)))
            {
                var dcs = new DataContractJsonSerializer(typeof(TResult));
                output = (TResult)dcs.ReadObject(mstream);
            }

            return output;
        }
    }
}