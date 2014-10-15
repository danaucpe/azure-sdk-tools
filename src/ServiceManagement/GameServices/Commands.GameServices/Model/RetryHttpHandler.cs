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

namespace Microsoft.WindowsAzure.Commands.GameServices.Model
{
    using System;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.WindowsAzure.Commands.Utilities.Common;

    public class RetryHttpHandler : DelegatingHandler
    {
        private const int DefaultMaxTries = 3;
        private const int DefaultDeplayBetweenTriesMs = 600;
        private readonly Action<string> logger;

        public RetryHttpHandler(
            HttpMessageHandler innerHandler,
            int maxTries = DefaultMaxTries,
            int delayBetweenTriesMs = DefaultDeplayBetweenTriesMs,
            Action<string> logger = null)
            : base(innerHandler)
        {
            MaxTries = maxTries;
            DeplayBetweenTriesMs = delayBetweenTriesMs;
            this.logger = logger;
        }

        public RetryHttpHandler(
            HttpMessageHandler innerHandler,
            Action<string> logger = null)
            : this(innerHandler, DefaultMaxTries, DefaultDeplayBetweenTriesMs, logger)
        {
        }

        public int MaxTries
        {
            get;
            set;
        }

        public int DeplayBetweenTriesMs
        {
            get;
            set;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            HttpResponseMessage resp = null;
            for (var numTries = 0; numTries < MaxTries; numTries++)
            {
                resp = await base.SendAsync(request, cancellationToken);

                if ((int)resp.StatusCode < 500 || numTries == MaxTries - 1)
                {
                    // No need to keep retrying
                    return resp;
                }

                // First request and final response are already logged in the Commands.Utilities.Common.HttpClientExtensions methods,
                // so we just make note of any retries.
                WriteLog("Request attempt #" + (numTries + 1) + " will be retried due to a bad response:\n"
                    + GeneralUtilities.GetHttpResponseLog(resp.StatusCode.ToString(), resp.Headers, string.Empty));

                resp.Dispose();

                await Task.Delay(DeplayBetweenTriesMs, cancellationToken);
            }

            return resp;
        }

        private void WriteLog(string logEntry)
        {
            // Note: Log entries are only shown if $DebugPreference = "Continue"
            if (logger != null)
            {
                logger(logEntry);
            }
        }
    }
}
