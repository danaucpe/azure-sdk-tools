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

namespace Microsoft.WindowsAzure.Commands.Utilities.CloudGame.BackCompat
{
    using System;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Json;
    using System.Text;
    using System.Threading.Tasks;
    using System.Xml;
    using System.Xml.Serialization;
    using Contract;
    using Websites.Services;
    using ServiceManagement;
    using Newtonsoft.Json;
    using Utilities.Common;

    /// <summary>
    ///     Implements helper functions used by the ClientGameClient
    /// </summary>
    public class ClientHelper
    {
        public static async Task RegisterCloudService(HttpClient httpJsonClient, HttpClient httpXmlClient)
        {
            // Check registration.
            var url = httpJsonClient.BaseAddress + String.Format("/services?service=gameservices.xboxlivecompute&action=register");
            var responseMessage = await httpJsonClient.PutAsync(url, null).ConfigureAwait(false);
            ProcessBooleanJsonResponseAllowConflict(responseMessage);

            // See if the cloud service exists, and create it if it does not.
            url = httpXmlClient.BaseAddress + String.Format(CloudGameUriElements.CloudServiceResourcePath);
            responseMessage = await httpXmlClient.GetAsync(url).ConfigureAwait(false);
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

            throw CreateExceptionFromJson(responseMessage.StatusCode, content);
        }

        public static bool ProcessBooleanJsonResponseAllowConflict(HttpResponseMessage responseMessage)
        {
            if (responseMessage.IsSuccessStatusCode || (responseMessage.StatusCode == HttpStatusCode.Conflict))
            {
                return true;
            }

            // Error
            throw CreateExceptionFromJson(responseMessage.StatusCode, string.Empty);
        }

        public static async Task<XblComputeColletion> ProcessCloudServiceResponse(HttpResponseMessage responseMessage)
        {
            var content = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
            var encoding = GetEncodingFromResponseMessage(responseMessage);
            var response = new XblComputeColletion();

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
                        // If there are no intrinsic settings, or the intrinsic setting is null, then create an empty
                        // XblCompute in the error state.
                        if (resource.IntrinsicSettings == null || 
                            resource.IntrinsicSettings.Length == 0 ||
                            resource.IntrinsicSettings[0] == null)
                        {
                            response.Add(new XblCompute() { Name = resource.Name, InErrorState = true});
                            continue;
                        }

                        var cbData = resource.IntrinsicSettings[0] as XmlCDataSection;
                        var jsonSer = new DataContractJsonSerializer(typeof(XblCompute));
                        using (var jsonStream = new MemoryStream(UTF8Encoding.UTF8.GetBytes((cbData.Data))))
                        {
                            // Deserialize the result from GSRM
                            var xblCompute = (XblCompute)jsonSer.ReadObject(jsonStream);

                            // Check the error state
                            xblCompute.InErrorState = resource.OperationStatus.Error != null;

                            // Add the xlbCompute instance to the collection
                            response.Add(xblCompute);
                        }
                    }
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
        public static ServiceManagementClientException CreateExceptionFromJson(HttpStatusCode statusCode, string content)
        {
            var exception = new ServiceManagementClientException(
                statusCode,
                new ServiceManagementError
                {
                    Code = statusCode.ToString(),
                    Message = content
                },
                string.Empty);
            return exception;
        }

        /// <summary>
        ///     Creates and initialize and instance of HttpClient for a specific media type
        /// </summary>
        /// <returns></returns>
        public static HttpClient CreateCloudGameHttpClient(WindowsAzureSubscription subscription, string mediaType)
        {
            var requestHandler = new WebRequestHandler();
            requestHandler.ClientCertificates.Add(subscription.Certificate);
            var endpoint = new StringBuilder(General.EnsureTrailingSlash(subscription.ServiceEndpoint.ToString()));
            endpoint.Append(subscription.SubscriptionId);

            var client = HttpClientHelper.CreateClient(endpoint.ToString(), handler: requestHandler);
            client.DefaultRequestHeaders.Add(CloudGameUriElements.XblCorrelationHeader, Guid.NewGuid().ToString());
            client.DefaultRequestHeaders.Add(Constants.VersionHeaderName, "2012-08-01");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(mediaType));
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