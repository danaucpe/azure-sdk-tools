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

namespace Microsoft.WindowsAzure.Commands.CloudGame
{
    using System;
    using System.Linq;
    using System.Management.Automation;
    using Microsoft.WindowsAzure.Commands.Utilities.Common;
    using Microsoft.WindowsAzure.ServiceManagement;
    using Utilities.CloudGame;
    using Utilities.CloudGame.Common;

    /// <summary>
    /// Get info about the GameServices cmdlets.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "AzureGameServicesCmdletsInfo"), OutputType(typeof(GameServicesCmdletsInfo))]
    public class GetAzureGameServicesCmdletsInfoCommand : CmdletWithSubscriptionBase
    {
        public ICloudGameClient Client { get; set; }

        public override void ExecuteCmdlet()
        {
            Client = Client ?? new CloudGameClient(CurrentSubscription, null);
            WriteObject(Client.Info);
        }
    }
}
