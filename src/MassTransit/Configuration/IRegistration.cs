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
namespace MassTransit
{
    using System;
    using ConsumeConfigurators;
    using Saga;


    /// <summary>
    /// Registration contains the consumers and sagas that have been registered, allowing them to be configured on one or more
    /// receive endpoints.
    /// </summary>
    public interface IRegistration
    {
        /// <summary>
        /// Configure a consumer on the receive endpoint
        /// </summary>
        /// <param name="consumerType">The consumer type</param>
        /// <param name="configurator"></param>
        void ConfigureConsumer(Type consumerType, IReceiveEndpointConfigurator configurator);

        /// <summary>
        /// Configure a consumer on the receive endpoint, with an optional configuration action
        /// </summary>
        /// <param name="configurator"></param>
        /// <param name="configure"></param>
        /// <typeparam name="T">The consumer type</typeparam>
        void ConfigureConsumer<T>(IReceiveEndpointConfigurator configurator, Action<IConsumerConfigurator<T>> configure = null)
            where T : class, IConsumer;

        /// <summary>
        /// Configure all registered consumers on the receive endpoint
        /// </summary>
        /// <param name="configurator"></param>
        void ConfigureConsumers(IReceiveEndpointConfigurator configurator);

        /// <summary>
        /// Configure a saga on the receive endpoint
        /// </summary>
        /// <param name="sagaType">The saga type</param>
        /// <param name="configurator"></param>
        void ConfigureSaga(Type sagaType, IReceiveEndpointConfigurator configurator);

        /// <summary>
        /// Configure a saga on the receive endpoint, with an optional configuration action
        /// </summary>
        /// <param name="configurator"></param>
        /// <param name="configure"></param>
        /// <typeparam name="T">The saga type</typeparam>
        void ConfigureSaga<T>(IReceiveEndpointConfigurator configurator, Action<ISagaConfigurator<T>> configure = null)
            where T : class, ISaga;

        /// <summary>
        /// Configure all registered sagas on the receive endpoint
        /// </summary>
        /// <param name="configurator"></param>
        void ConfigureSagas(IReceiveEndpointConfigurator configurator);

        /// <summary>
        /// Configure the specified execute activity type
        /// </summary>
        /// <param name="activityType"></param>
        /// <param name="configurator"></param>
        void ConfigureExecuteActivity(Type activityType, IReceiveEndpointConfigurator configurator);

        /// <summary>
        /// Configure the specified  activity type
        /// </summary>
        /// <param name="activityType"></param>
        /// <param name="executeEndpointConfigurator">The configurator for the execute activity endpoint</param>
        /// <param name="compensateEndpointConfigurator">The configurator for the compensate activity endpoint</param>
        void ConfigureActivity(Type activityType, IReceiveEndpointConfigurator executeEndpointConfigurator,
            IReceiveEndpointConfigurator compensateEndpointConfigurator);

        /// <summary>
        /// Configure the endpoints for all defined types
        /// </summary>
        /// <param name="configurator"></param>
        /// <param name="endpointNameFormatter"></param>
        /// <typeparam name="T"></typeparam>
        void ConfigureEndpoints<T>(T configurator, IEndpointNameFormatter endpointNameFormatter)
            where T : IBusFactoryConfigurator;
    }
}
