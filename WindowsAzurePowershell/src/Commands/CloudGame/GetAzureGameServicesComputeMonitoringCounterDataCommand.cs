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

namespace Microsoft.WindowsAzure.Commands.CloudGame
{
    using System;
    using System.Management.Automation;
    using Utilities.CloudGame;
    using Utilities.CloudGame.Contract;
    using Utilities.CloudGame.Common;

    /// <summary>
    /// Get the monitoring counter data.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "AzureGameServicesComputeMonitoringCounterData"), OutputType(typeof(CounterChartDataResponse))]
    public class GetAzureGameServicesComputeMonitoringCounterDataCommand : AzureGameServicesHttpClientCommandBase
    {
        [Parameter(Position = 0, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The cloud game name.")]
        [ValidateNotNullOrEmpty]
        public string CloudGameName { get; set; }

        [Parameter(Position = 1, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The cloud game platform.")]
        [ValidateNotNullOrEmpty]
        public CloudGamePlatform Platform { get; set; }

        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The geographic region.")]
        [ValidateNotNullOrEmpty]
        public string GeoRegion { get; set; }

        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The selected counters.")]
        [ValidateNotNullOrEmpty]
        public string[] CounterNames { get; set; }

        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The start time (expressed in UTC).")]
        public DateTime? StartTime { get; set; }

        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The end time (expressed in UTC).")]
        public DateTime? EndTime { get; set; }

        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The time zoom (time between data points).")]
        public TimeSpan? TimeZoom { get; set; }

        public ICloudGameClient Client { get; set; }

        protected override void Execute()
        {
            Client = Client ?? new CloudGameClient(CurrentSubscription, WriteDebugLog);
            DateTime startTime;
            DateTime endTime;
            if (!StartTime.HasValue && !EndTime.HasValue)
            {
                startTime = DateTime.UtcNow.AddHours(-12);
                endTime = DateTime.UtcNow;
            }
            else if (StartTime.HasValue && EndTime.HasValue)
            {
                startTime = StartTime.Value;
                endTime = EndTime.Value;
            }
            else
            {
                throw new ArgumentException("Either both start time and end time must be specified, or neither one.");
            }
            
            var timeZoom = TimeZoom.HasValue ? TimeZoom.Value : TimeSpan.FromHours(1);
            var result = Client.GetComputeMonitoringCounterData(CloudGameName, Platform, GeoRegion, startTime, endTime, timeZoom, CounterNames).Result;
            WriteObject(result);
        }
    }
}
