// Copyright 2007-2018 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use
// this file except in compliance with the License. You may obtain a copy of the
// License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR
// CONDITIONS OF ANY KIND, either express or implied. See the License for the
// specific language governing permissions and limitations under the License.
#if NETSTANDARD
namespace MassTransit
{
    using System.Diagnostics;
    using ConsumePipeSpecifications;
    using PublishPipeSpecifications;
    using SendPipeSpecifications;


    public static class DiagnosticsActivityPipeConfiguratorExtensions
    {
        public static void UseDiagnosticsActivity(this IBusFactoryConfigurator configurator, DiagnosticSource diagnosticSource,
            string activityIdKey = DiagnosticHeaders.ActivityIdHeader,
            string activityCorrelationContextKey = DiagnosticHeaders.ActivityCorrelationContext)
        {
            configurator.ConfigureSend(x =>
                x.ConnectSendPipeSpecificationObserver(new ActivitySendPipeSpecificationObserver(diagnosticSource, activityIdKey, activityCorrelationContextKey)));

            configurator.ConfigurePublish(x =>
                x.ConnectPublishPipeSpecificationObserver(new ActivityPublishPipeSpecificationObserver(diagnosticSource, activityIdKey, activityCorrelationContextKey)));

            var observer = new ActivityConsumePipeSpecificationObserver(configurator, diagnosticSource, activityIdKey, activityCorrelationContextKey);
        }
    }
}
#endif
