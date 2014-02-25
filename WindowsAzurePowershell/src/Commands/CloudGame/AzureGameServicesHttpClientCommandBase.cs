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
    using Utilities.Common;
    using System;
    using System.Text;

    public class AzureGameServicesHttpClientCommandBase : CmdletWithSubscriptionBase
    {
        private readonly StringBuilder debugLog = new StringBuilder();

        public override void ExecuteCmdlet()
        {
            try
            {
                this.debugLog.Clear();
                this.WriteDebugLog(string.Format(
                    "Start Commandlet execution. Subscription {0}, StartTime: {1}",
                    this.HasCurrentSubscription ? this.CurrentSubscription.SubscriptionName + " : " + this.CurrentSubscription.SubscriptionId : "None",
                    DateTime.UtcNow));
                CatchAggregatedExceptionFlattenAndRethrow(() =>
                    {
                        Validate.ValidateInternetConnection();
                        this.Execute();
                    });
            }
            catch (Exception ex)
            {
                WriteDebugLog(ex.ToString());
                WriteExceptionError(ex);
            }
            finally
            {
                this.WriteDebugLog(string.Format("End Commandlet execution. EndTime: {0}", DateTime.UtcNow));
                WriteDebug(this.debugLog.ToString());
            }
        }

        protected virtual void Execute()
        {
        }

        protected void WriteDebugLog(string log)
        {
            this.debugLog.AppendLine(log);
        }

        private static void CatchAggregatedExceptionFlattenAndRethrow(Action c)
        {
            try
            {
                c();
            }
            catch (AggregateException ex)
            {
                var flat = ex.Flatten();
                if (flat.InnerExceptions.Count == 1)
                {
                    throw flat.InnerException;
                }
                else
                {
                    throw flat;
                }
            }
        }
    }
}