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

using Microsoft.WindowsAzure.Commands.Utilities.CloudGame.Common;

namespace Microsoft.WindowsAzure.Commands.Test.Utilities.CloudGame
{
    using System;
    using System.Net.Http;
    using VisualStudio.TestTools.UnitTesting;
    using Common;
    using Commands.Utilities.CloudGame;

    [TestClass]
    public class XblComputeClientTests
    {
        /// <summary>
        /// CloudGameClient command
        /// </summary>
        public CloudGameClient command = null;

        ////public HttpMessageHandler testHandler = new HttpRestMessageHandler();

        [TestInitialize]
        public void InitCommand()
        {
            command = new CloudGameClient(null, null, new HttpClient(), null);
        }

        [TestCleanup]
        public void CleanCommand()
        {
            command = null;
        }

        [TestMethod]
        public void ValidatePipelineICloudBlobWithNullTest()
        {
            Testing.AssertThrows<ArgumentException>(() => command.NewGamePackage(null, CloudGamePlatform.XboxOne, Guid.NewGuid(), null, null, false, null), "");
        }
    }
}
