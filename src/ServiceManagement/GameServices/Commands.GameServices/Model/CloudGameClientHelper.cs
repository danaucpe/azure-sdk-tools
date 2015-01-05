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

namespace Microsoft.WindowsAzure.Commands.GameServices.Model
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Formatting;
    using System.Net.Http.Headers;
    using System.Reflection;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Runtime.Serialization.Json;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Xml;
    using System.Xml.Linq;
    using System.Xml.Serialization;
    using Contract;
    using Microsoft.WindowsAzure.Commands.Common;
    using Microsoft.WindowsAzure.Commands.Common.Factories;
    using Microsoft.WindowsAzure.Commands.Common.Models;
    using Microsoft.WindowsAzure.Commands.GameServices.Model.Common;
    using Microsoft.WindowsAzure.Commands.ServiceManagement.Model;
    using Microsoft.WindowsAzure.Commands.Utilities.Common;
    using Microsoft.WindowsAzure.Commands.Utilities.Common.Authentication;
    using Microsoft.WindowsAzure.Commands.Utilities.Websites.Services;
    using Microsoft.WindowsAzure.Common.Internals;
    using Newtonsoft.Json;

    /// <summary>
    ///     Implements helper functions used by the ClientGameClient
    /// </summary>
    public static class ClientHelper
    {
        /// <summary>
        /// The general information about the game services cmdlets.
        /// </summary>
        public static readonly GameServicesCmdletsInfo Info = new GameServicesCmdletsInfo("2015_01_v1");

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
                    throw new ServiceResponseException(responseMessage.StatusCode, string.Empty);
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
                throw new ServiceResponseException(responseMessage.StatusCode, string.Empty);
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
                throw new ServiceResponseException(responseMessage.StatusCode, string.Empty);
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
                throw new ServiceResponseException(
                    responseMessage.StatusCode,
                    string.Empty);
            }
        }

        /// <summary>
        ///     Processes the response and handle error cases.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="responseMessage">The response message.</param>
        /// <returns></returns>
        /// <exception cref="Microsoft.WindowsAzure.ServiceManagement.ServiceResponseException"></exception>
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

        public static bool ProcessBooleanJsonResponse(HttpResponseMessage responseMessage)
        {
            if (responseMessage.IsSuccessStatusCode)
            {
                return true;
            }

            // Error
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
                catch (ServiceResponseException)
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

        public static ServiceResponseException CreateExceptionFromXml(string content, HttpResponseMessage message)
        {
            var encoding = GetEncodingFromResponseMessage(message);

            using (var stream = new MemoryStream(encoding.GetBytes(content)))
            {
                stream.Position = 0;
                var serializer = new XmlSerializer(typeof(ServiceError));
                var serviceError = (ServiceError)serializer.Deserialize(stream);
                return new ServiceResponseException(message.StatusCode, serviceError.Message);
            }
        }

        /// <summary>
        ///     Unwraps error message and creates ServiceResponseException.
        /// </summary>
        public static ServiceResponseException CreateExceptionFromJson(HttpStatusCode statusCode, string message)
        {
            var exception = new ServiceResponseException(statusCode, message);
            return exception;
        }

        /// <summary>
        ///     Unwraps error message and creates ServiceResponseException.
        /// </summary>
        public static ServiceResponseException CreateExceptionFromJson(HttpResponseMessage httpResponse)
        {
            // See if there is any detailed information we can extract from the HTTP response object to aid the user in determining the cause
            string errorMessage = string.Empty;

            if (httpResponse.Content != null)
            {
                var contentString = httpResponse.Content.ReadAsStringAsync().Result;
                var doc = new XmlDocument();
                try
                {
                    doc.LoadXml(contentString);
                    var root = doc.FirstChild;
                    if (root.ChildNodes.Count == 1)
                    {
                        contentString = root.InnerText;
                        try
                        {
                            var messageDetails = JsonConvert.DeserializeObject<ErrorResponse>(contentString);
                            errorMessage = messageDetails.ExtendedCode;
                        }
                        catch (JsonSerializationException)
                        {
                            errorMessage = contentString;
                        }
                    }
                    else
                    {
                        var nsmgr = new XmlNamespaceManager(doc.NameTable);
                        nsmgr.AddNamespace("azure", "http://schemas.microsoft.com/windowsazure");
                        var message = root.SelectSingleNode("azure:Message", nsmgr);
                        errorMessage = message != null ? message.InnerText : root.InnerText;
                    }
                }
                catch (Exception ex)
                {
                    // Defensively parse in case this data is missing/malformed
                    ex = ex is AggregateException ? ex.InnerException : ex;
                    if (ex is SerializationException || ex is XmlException)
                    {
                        errorMessage = contentString;
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            var exception = new ServiceResponseException(httpResponse.StatusCode, errorMessage);
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
        /// <exception cref="ServiceResponseException">If the initial response code is not 202 Accepted.</exception>
        /// <exception cref="System.ArgumentException">If there is no request ID header found in initial response</exception>
        public static async Task<OperationStatusResponse> PollOperationStatus(
            HttpResponseMessage initialHttpResponse,
            HttpClient httpXmlClient,
            int pollIntervalInSeconds,
            int timeoutInSeconds,
            Action<string> logger = null)
        {
            try
            {
                if (initialHttpResponse.StatusCode != HttpStatusCode.Accepted)
                {
                    // Unexpected result, so throw an exception
                    throw new ServiceResponseException(initialHttpResponse.StatusCode, "Unexpected status code. Should be 202 Accepted.");
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
                OperationStatusResponse result = null;

                var done = false;
                while (!done)
                {
                    var httpResponse = await httpXmlClient.GetAsync(url).ConfigureAwait(false);
                    try
                    {
                        var xmlString = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);

                        result = ParseOperationStatusResponse(xmlString);
                        result.StatusCode = httpResponse.StatusCode;
                        if (httpResponse.Headers.Contains(CloudGameUriElements.RequestIdHeader))
                        {
                            result.RequestId =
                                httpResponse.Headers.GetValues(CloudGameUriElements.RequestIdHeader).FirstOrDefault();
                        }

                        switch (result.Status)
                        {
                            case WindowsAzure.OperationStatus.InProgress:
                                Thread.Sleep((int) pollInterval.TotalMilliseconds);
                                break;

                            case WindowsAzure.OperationStatus.Failed:
                                throw new ServiceResponseException(result.HttpStatusCode, result.Error.Message);

                            case WindowsAzure.OperationStatus.Succeeded:
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
                    finally
                    {
                        if (httpResponse != null)
                        {
                            httpResponse.Dispose();
                        }
                    }
                }

                return result;
            }
            finally
            {
                if (initialHttpResponse != null)
                {
                    initialHttpResponse.Dispose();
                }
            }
        }

        private static OperationStatusResponse ParseOperationStatusResponse(string xmlString)
        {
            // Note a helper method is used to parse the XML response because the endpoint is not a WCF service and does not use a standard data contract
            var result = new OperationStatusResponse();
            XDocument responseDoc = XDocument.Parse(xmlString);

            XElement operationElement = responseDoc.Element(XName.Get("Operation", "http://schemas.microsoft.com/windowsazure"));
            if (operationElement != null)
            {
                XElement idElement = operationElement.Element(XName.Get("ID", "http://schemas.microsoft.com/windowsazure"));
                if (idElement != null)
                {
                    string idInstance = idElement.Value;
                    result.Id = idInstance;
                }

                XElement statusElement = operationElement.Element(XName.Get("Status", "http://schemas.microsoft.com/windowsazure"));
                if (statusElement != null)
                {
                    var statusInstance = ((WindowsAzure.OperationStatus)Enum.Parse(typeof(WindowsAzure.OperationStatus), statusElement.Value, true));
                    result.Status = statusInstance;
                }

                XElement httpStatusCodeElement = operationElement.Element(XName.Get("HttpStatusCode", "http://schemas.microsoft.com/windowsazure"));
                if (httpStatusCodeElement != null)
                {
                    HttpStatusCode httpStatusCodeInstance = ((HttpStatusCode)Enum.Parse(typeof(HttpStatusCode), httpStatusCodeElement.Value, true));
                    result.HttpStatusCode = httpStatusCodeInstance;
                }

                XElement errorElement = operationElement.Element(XName.Get("Error", "http://schemas.microsoft.com/windowsazure"));
                if (errorElement != null)
                {
                    OperationStatusResponse.ErrorDetails errorInstance = new OperationStatusResponse.ErrorDetails();
                    result.Error = errorInstance;

                    XElement codeElement = errorElement.Element(XName.Get("Code", "http://schemas.microsoft.com/windowsazure"));
                    if (codeElement != null)
                    {
                        string codeInstance = codeElement.Value;
                        errorInstance.Code = codeInstance;
                    }

                    XElement messageElement = errorElement.Element(XName.Get("Message", "http://schemas.microsoft.com/windowsazure"));
                    if (messageElement != null)
                    {
                        string messageInstance = messageElement.Value;
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
        public static HttpClient CreateCloudGameHttpClient(AzureContext context, string mediaType, Action<string> logger)
        {
            var requestHandler = new WebRequestHandler();
            AuthenticationHeaderValue authHeader = null;

            var cloudCredentials = AzureSession.AuthenticationFactory.GetSubscriptionCloudCredentials(context);
            if (cloudCredentials != null)
            {
                if (cloudCredentials is CertificateCloudCredentials)
                {
                    // Attach the cert
                    CertificateCloudCredentials creds = (CertificateCloudCredentials) cloudCredentials;
                    requestHandler.ClientCertificates.Add(creds.ManagementCertificate);
                }
                else if (cloudCredentials is AccessTokenCredential)
                {
                    // Extract the token
                    var field = typeof (AccessTokenCredential).GetField("token",
                        BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.Instance);
                    var accessToken = field.GetValue(cloudCredentials) as IAccessToken;
                    if (accessToken != null)
                    {
                        var token = accessToken.AccessToken;
                        authHeader = new AuthenticationHeaderValue("Bearer", token);
                    }
                    else
                    {
                        throw new Exception("Missing access token");
                    }
                }
                else
                {
                    throw new Exception("Unexpected credentials type");
                }
            }
            
            var loggingHandler = new LoggingHttpHandler(requestHandler, logger);
            var retryHandler = new RetryHttpHandler(loggingHandler, logger);
            var endpoint = new StringBuilder(GeneralUtilities.EnsureTrailingSlash(context.Environment.Endpoints[AzureEnvironment.Endpoint.ServiceManagement]));
            endpoint.Append(context.Subscription.Id);

            var client = AzureSession.ClientFactory.CreateHttpClient(endpoint.ToString(), retryHandler);
            client.DefaultRequestHeaders.Add(CloudGameUriElements.XblCorrelationHeader, Guid.NewGuid().ToString());
            client.DefaultRequestHeaders.Add(CloudGameUriElements.GameServicesCmdletVersionHeader, Info.Version);
            client.DefaultRequestHeaders.Add(Constants.VersionHeaderName, "2013-11-01");
            
            if (authHeader != null)
            {
                client.DefaultRequestHeaders.Authorization = authHeader;
            }

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