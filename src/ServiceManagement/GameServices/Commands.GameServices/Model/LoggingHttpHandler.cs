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

    public class LoggingHttpHandler : DelegatingHandler
    {
        private readonly Action<string> logger;

        public LoggingHttpHandler(Action<string> logger = null)
            : base()
        {
            this.logger = logger;
        }
        
        public LoggingHttpHandler(HttpMessageHandler innerHandler, Action<string> logger = null)
            : base(innerHandler)
        {
            this.logger = logger;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            WriteLog("Request:");
            WriteLog(request.ToString());
            if (request.Content != null)
            {
                WriteLog(await request.Content.ReadAsStringAsync());
            }
            WriteLog("");

            HttpResponseMessage response = await base.SendAsync(request, cancellationToken);

            WriteLog("Response:");
            WriteLog(response.ToString());
            if (response.Content != null)
            {
                WriteLog(await response.Content.ReadAsStringAsync());
            }
            WriteLog("");

            return response;
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