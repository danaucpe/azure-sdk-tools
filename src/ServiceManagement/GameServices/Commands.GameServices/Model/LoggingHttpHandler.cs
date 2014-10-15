using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.Commands.GameServices.Model
{
    using System.Net.Http;
    using System.Threading;

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