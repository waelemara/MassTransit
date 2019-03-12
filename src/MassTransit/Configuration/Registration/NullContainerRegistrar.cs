// Copyright 2007-2019 Chris Patterson, Dru Sellers, Travis Smith, et. al.
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
namespace MassTransit.Registration
{
    using Courier;
    using Definition;
    using Saga;


    public class NullContainerRegistrar :
        IContainerRegistrar
    {
        public void RegisterConsumer<T>()
            where T : class, IConsumer
        {
        }

        public void RegisterConsumerDefinition<TDefinition, TConsumer>()
            where TDefinition : class, IConsumerDefinition<TConsumer>
            where TConsumer : class, IConsumer
        {
        }

        public void RegisterSaga<T>()
            where T : class, ISaga
        {
        }

        public void RegisterSagaDefinition<TDefinition, TSaga>()
            where TDefinition : class, ISagaDefinition<TSaga>
            where TSaga : class, ISaga
        {
        }

        public void RegisterExecuteActivity<TActivity, TArguments>()
            where TActivity : class, ExecuteActivity<TArguments>
            where TArguments : class
        {
        }

        public void RegisterCompensateActivity<TActivity, TLog>()
            where TActivity : class, CompensateActivity<TLog>
            where TLog : class
        {
        }
    }
}
