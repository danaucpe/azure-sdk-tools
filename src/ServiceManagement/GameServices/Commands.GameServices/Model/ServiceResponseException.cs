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
    using System.Net;
    using System.Text;

    public class ServiceResponseException : Exception
    {
        private static string ErrorString(HttpStatusCode? statusCode, string message)
        {
            var sb = new StringBuilder();
            sb.Append("HTTP Error");
            if (statusCode.HasValue)
            {
                sb.Append(" ");
                sb.Append(statusCode.Value);
            }
            if (!string.IsNullOrEmpty(message))
            {
                sb.Append(": ");
                sb.Append(message);
            }
            return sb.ToString();
        }

        public HttpStatusCode? StatusCode { get; set; }

        public string ErrorMessage { get; set; }

        public ServiceResponseException(string error) : base(ErrorString(null, error)) { }

        public ServiceResponseException(HttpStatusCode code, string error)
            : base(ErrorString(code, error))
        {
            this.StatusCode = code;
            this.ErrorMessage = error;
        }
    }
}
