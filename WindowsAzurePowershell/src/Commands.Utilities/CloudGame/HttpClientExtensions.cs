// ----------------------------------------------------------------------------------
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
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Microsoft.WindowsAzure.Commands.Utilities.Common;

    public static class HttpClientExtensions
    {
        private const int DefaultMaxTries = 3;
        private const int DeplayBetweenTriesMs = 600;

        public static async Task<HttpResponseMessage> GetWithRetriesAsync(this HttpClient client, string requestUri, Action<string> logger, int maxTries = DefaultMaxTries)
        {
            HttpResponseMessage resp = null;

            for (var numTries = 0; numTries < maxTries; numTries++)
            {
                resp = await client.GetAsync(requestUri, logger);

                if (resp.IsSuccessStatusCode
                    || resp.StatusCode == HttpStatusCode.BadRequest
                    || numTries == maxTries - 1)
                {
                    // No need to keep retrying
                    break;
                }

                await TaskEx.Delay(DeplayBetweenTriesMs);
            }

            return resp;
        }

        public static async Task<HttpResponseMessage> PostWithRetriesAsync(this HttpClient client, string requestUri, HttpContent content, int maxTries = DefaultMaxTries)
        {
            HttpResponseMessage resp = null;

            for (var numTries = 0; numTries < maxTries; numTries++)
            {
                resp = await client.PostAsync(requestUri, content);

                if (resp.IsSuccessStatusCode
                    || resp.StatusCode == HttpStatusCode.BadRequest
                    || numTries == maxTries - 1)
                {
                    // No need to keep retrying
                    break;
                }

                await TaskEx.Delay(DeplayBetweenTriesMs);
            }

            return resp;
        }

        public static async Task<HttpResponseMessage> PutWithRetriesAsync(this HttpClient client, string requestUri, HttpContent content, int maxTries = DefaultMaxTries)
        {
            HttpResponseMessage resp = null;

            for (var numTries = 0; numTries < maxTries; numTries++)
            {
                resp = await client.PutAsync(requestUri, content);

                if (resp.IsSuccessStatusCode
                    || resp.StatusCode == HttpStatusCode.BadRequest
                    || numTries == maxTries - 1)
                {
                    // No need to keep retrying
                    break;
                }

                await TaskEx.Delay(DeplayBetweenTriesMs);
            }

            return resp;
        }

        public static async Task<HttpResponseMessage> PutAsJsonWithRetriesAsync<T>(this HttpClient client, string requestUri, T value, int maxTries = DefaultMaxTries)
        {
            HttpResponseMessage resp = null;

            for (var numTries = 0; numTries < maxTries; numTries++)
            {
                resp = await client.PutAsJsonAsync(requestUri, value);

                if (resp.IsSuccessStatusCode
                    || resp.StatusCode == HttpStatusCode.BadRequest
                    || numTries == maxTries - 1)
                {
                    // No need to keep retrying
                    break;
                }

                await TaskEx.Delay(DeplayBetweenTriesMs);
            }

            return resp;
        }

        public static async Task<HttpResponseMessage> DeleteWithRetriesAsync(this HttpClient client, string requestUri, int maxTries = DefaultMaxTries)
        {
            HttpResponseMessage resp = null;

            for (var numTries = 0; numTries < maxTries; numTries++)
            {
                resp = await client.DeleteAsync(requestUri);

                if (resp.IsSuccessStatusCode
                    || resp.StatusCode == HttpStatusCode.NotFound
                    || resp.StatusCode == HttpStatusCode.BadRequest
                    || numTries == maxTries - 1)
                {
                    // No need to keep retrying
                    break;
                }

                await TaskEx.Delay(DeplayBetweenTriesMs);
            }

            return resp;
        }
    }
}
