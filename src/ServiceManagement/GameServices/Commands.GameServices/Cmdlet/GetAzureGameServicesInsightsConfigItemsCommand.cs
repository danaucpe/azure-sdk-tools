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

namespace Microsoft.WindowsAzure.Commands.GameServices.Cmdlet
{
    using System;
    using System.Collections.Generic;
    using System.Management.Automation;
    using Microsoft.WindowsAzure.Commands.GameServices.Model;
    using Microsoft.WindowsAzure.Commands.GameServices.Model.Contract;

    /// <summary>
    /// Get the cloud game assets.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "AzureGameServicesInsightsConfigItems"), OutputType(typeof(List<InsightsConfigItem>))]
    public class GetAzureGameServicesInsightsConfigItemsCommand : AzureGameServicesHttpClientCommandBase
    {
        public ICloudGameClient Client { get; set; }

        protected override void Execute()
        {
            Client = Client ?? new CloudGameClient(CurrentContext, WriteDebugLog);
            var result = Client.GetInsightsConfigItems().Result;
            WriteObject(result == null ? new List<InsightsConfigItem>() : result.InsightsConfigItems);
        }
    }
}