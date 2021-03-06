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
using System.Collections.Generic;
using Xunit;
using Microsoft.WindowsAzure.Commands.Common.Test.Mocks;
using Microsoft.WindowsAzure.Commands.ServiceBus;
using Microsoft.WindowsAzure.Commands.Test.Utilities.Common;
using Microsoft.WindowsAzure.Commands.Utilities.Properties;
using Microsoft.WindowsAzure.Commands.Utilities.ServiceBus;
using Microsoft.WindowsAzure.Management.ServiceBus.Models;
using Moq;

namespace Microsoft.WindowsAzure.Commands.Test.ServiceBus
{
    
    public class NewAzureSBNamespaceTests : TestBase
    {
        public NewAzureSBNamespaceTests()
        {
            new FileSystemHelper(this).CreateAzureSdkDirectoryAndImportPublishSettings();
        }

        [Fact]
        public void NewAzureSBNamespaceSuccessfull()
        {
            // Setup
            MockCommandRuntime mockCommandRuntime = new MockCommandRuntime();
            Mock<ServiceBusClientExtensions> client = new Mock<ServiceBusClientExtensions>();
            string name = "test";
            string location = "West US";
            NewAzureSBNamespaceCommand cmdlet = new NewAzureSBNamespaceCommand()
            {
                Name = name,
                Location = location,
                CommandRuntime = mockCommandRuntime,
                Client = client.Object
            };
            ExtendedServiceBusNamespace expected = new ExtendedServiceBusNamespace { Name = name, Region = location };
            client.Setup(f => f.CreateNamespace(name, location)).Returns(expected);
            client.Setup(f => f.GetServiceBusRegions()).Returns(new List<ServiceBusLocation>()
            {
                new ServiceBusLocation () { Code = location }
            });

            // Test
            cmdlet.ExecuteCmdlet();

            // Assert
            ExtendedServiceBusNamespace actual = mockCommandRuntime.OutputPipeline[0] as ExtendedServiceBusNamespace;
            Assert.Equal<ExtendedServiceBusNamespace>(expected, actual);
        }

        [Fact]
        public void NewAzureSBNamespaceGetsDefaultLocation()
        {
            // Setup
            Mock<ServiceBusClientExtensions> client = new Mock<ServiceBusClientExtensions>();
            MockCommandRuntime mockCommandRuntime = new MockCommandRuntime();
            string name = "test";
            string location = "West US";
            NewAzureSBNamespaceCommand cmdlet = new NewAzureSBNamespaceCommand()
            {
                Name = name,
                CommandRuntime = mockCommandRuntime,
                Client = client.Object,
                Location = location
            };
            ExtendedServiceBusNamespace expected = new ExtendedServiceBusNamespace { Name = name, Region = location };
            client.Setup(f => f.CreateNamespace(name, location)).Returns(expected);

            // Test
            cmdlet.ExecuteCmdlet();

            // Assert
            ExtendedServiceBusNamespace actual = mockCommandRuntime.OutputPipeline[0] as ExtendedServiceBusNamespace;
            Assert.Equal<ExtendedServiceBusNamespace>(expected, actual);
        }

        [Fact]
        public void NewAzureSBNamespaceWithInvalidNamesFail()
        {
            // Setup
            string[] invalidNames = { "1test", "test#", "test invaid", "-test", "_test" };
            Mock<ServiceBusClientExtensions> client = new Mock<ServiceBusClientExtensions>();

            foreach (string invalidName in invalidNames)
            {
                MockCommandRuntime mockCommandRuntime = new MockCommandRuntime();
                NewAzureSBNamespaceCommand cmdlet = new NewAzureSBNamespaceCommand()
                {
                    Name = invalidName,
                    Location = "West US",
                    CommandRuntime = mockCommandRuntime,
                    Client = client.Object
                };

                string expected = string.Format("{0}\r\nParameter name: Name", string.Format(Resources.InvalidNamespaceName, invalidName));
                client.Setup(f => f.CreateNamespace(invalidName, "West US")).Throws(new InvalidOperationException(expected));

                Testing.AssertThrows<InvalidOperationException>(() => cmdlet.ExecuteCmdlet(), expected);
            }
        }

        [Fact]
        public void CreatesNewSBCaseInsensitiveRegion()
        {
            // Setup
            MockCommandRuntime mockCommandRuntime = new MockCommandRuntime();
            Mock<ServiceBusClientExtensions> client = new Mock<ServiceBusClientExtensions>();
            string name = "test";
            string location = "West US";
            NewAzureSBNamespaceCommand cmdlet = new NewAzureSBNamespaceCommand()
            {
                Name = name,
                Location = location.ToLower(),
                CommandRuntime = mockCommandRuntime,
                Client = client.Object
            };
            ExtendedServiceBusNamespace expected = new ExtendedServiceBusNamespace { Name = name, Region = location };
            client.Setup(f => f.CreateNamespace(name, location.ToLower())).Returns(expected);
            client.Setup(f => f.GetServiceBusRegions()).Returns(new List<ServiceBusLocation>()
            {
                new ServiceBusLocation () { Code = location }
            });

            // Test
            cmdlet.ExecuteCmdlet();

            // Assert
            ExtendedServiceBusNamespace actual = mockCommandRuntime.OutputPipeline[0] as ExtendedServiceBusNamespace;
            Assert.Equal<ExtendedServiceBusNamespace>(expected, actual);
        }
    }
}