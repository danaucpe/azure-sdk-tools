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

using System;
using Microsoft.WindowsAzure.Commands.Common.Models;
using Microsoft.WindowsAzure.Common;
using System.Net;
using System.Net.Http;

namespace Microsoft.WindowsAzure.Commands.Common
{
    public interface IClientFactory
    {
        TClient CreateClient<TClient>(AzureSubscription subscription, AzureEnvironment.Endpoint endpoint) where TClient : ServiceClient<TClient>;

        TClient CreateClient<TClient>(params object[] parameters) where TClient : ServiceClient<TClient>;

        HttpClient CreateHttpClient(string endpoint, ICredentials credentials);

        HttpClient CreateHttpClient(string endpoint, HttpMessageHandler effectiveHandler);
    }
}
