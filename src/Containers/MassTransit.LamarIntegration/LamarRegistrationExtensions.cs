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
    using Definition;
    using Lamar;
    using LamarIntegration;
    using LamarIntegration.Registration;
    using Saga;


    /// <summary>
    /// Standard registration extensions, which are used to configure consumers, sagas, and activities on receive endpoints from a
    /// dependency injection container.
    /// </summary>
    public static class LamarRegistrationExtensions
    {
        /// <summary>
        /// Adds the required services to the service collection, and allows consumers to be added and/or discovered
        /// </summary>
        /// <param name="registry"></param>
        /// <param name="configure"></param>
        public static void AddMassTransit(this ServiceRegistry registry, Action<IServiceRegistryConfigurator> configure = null)
        {
            var configurator = new ServiceRegistryRegistrationConfigurator(registry);

            configure?.Invoke(configurator);
        }

        /// <summary>
        /// Configure the endpoints for all defined consumer, saga, and activity types using an optional
        /// endpoint name formatter. If no endpoint name formatter is specified and an <see cref="IEndpointNameFormatter"/>
        /// is registered in the container, it is resolved from the container. Otherwise, the <see cref="DefaultEndpointNameFormatter"/>
        /// is used.
        /// </summary>
        /// <param name="configurator">The <see cref="IBusFactoryConfigurator"/> for the bus being configured</param>
        /// <param name="container">The container reference</param>
        /// <param name="endpointNameFormatter">Optional, the endpoint name formatter</param>
        /// <typeparam name="T">The bus factory type (depends upon the transport)</typeparam>
        public static void ConfigureEndpoints<T>(this T configurator, IContainer container, IEndpointNameFormatter endpointNameFormatter = null)
            where T : IBusFactoryConfigurator
        {
            var registration = container.GetInstance<IRegistration>();

            registration.ConfigureEndpoints(configurator, endpointNameFormatter);
        }

        /// <summary>
        /// Configure the endpoints for all defined consumer, saga, and activity types using an optional
        /// endpoint name formatter. If no endpoint name formatter is specified and an <see cref="IEndpointNameFormatter"/>
        /// is registered in the container, it is resolved from the container. Otherwise, the <see cref="DefaultEndpointNameFormatter"/>
        /// is used.
        /// </summary>
        /// <param name="configurator">The <see cref="IBusFactoryConfigurator"/> for the bus being configured</param>
        /// <param name="context">The container reference</param>
        /// <param name="endpointNameFormatter">Optional, the endpoint name formatter</param>
        /// <typeparam name="T">The bus factory type (depends upon the transport)</typeparam>
        public static void ConfigureEndpoints<T>(this T configurator, IServiceContext context, IEndpointNameFormatter endpointNameFormatter = null)
            where T : IBusFactoryConfigurator
        {
            var registration = context.GetInstance<IRegistration>();

            registration.ConfigureEndpoints(configurator, endpointNameFormatter);
        }

        /// <summary>
        /// Configure a consumer (or consumers) on the receive endpoint
        /// </summary>
        /// <param name="configurator"></param>
        /// <param name="container"></param>
        /// <param name="consumerTypes">The consumer type(s) to configure</param>
        public static void ConfigureConsumer(this IReceiveEndpointConfigurator configurator, IContainer container, params Type[] consumerTypes)
        {
            var registration = container.GetInstance<IRegistration>();

            foreach (var consumerType in consumerTypes)
            {
                registration.ConfigureConsumer(consumerType, configurator);
            }
        }

        /// <summary>
        /// Configure a consumer (or consumers) on the receive endpoint
        /// </summary>
        /// <param name="configurator"></param>
        /// <param name="context"></param>
        /// <param name="consumerTypes">The consumer type(s) to configure</param>
        public static void ConfigureConsumer(this IReceiveEndpointConfigurator configurator, IServiceContext context, params Type[] consumerTypes)
        {
            var registration = context.GetInstance<IRegistration>();

            foreach (var consumerType in consumerTypes)
            {
                registration.ConfigureConsumer(consumerType, configurator);
            }
        }

        /// <summary>
        /// Configure a consumer on the receive endpoint, with an optional configuration action
        /// </summary>
        /// <param name="configurator"></param>
        /// <param name="container"></param>
        /// <param name="configure"></param>
        public static void ConfigureConsumer<T>(this IReceiveEndpointConfigurator configurator, IContainer container,
            Action<IConsumerConfigurator<T>> configure = null)
            where T : class, IConsumer
        {
            var registration = container.GetInstance<IRegistration>();

            registration.ConfigureConsumer(configurator, configure);
        }

        /// <summary>
        /// Configure a consumer on the receive endpoint, with an optional configuration action
        /// </summary>
        /// <param name="configurator"></param>
        /// <param name="context"></param>
        /// <param name="configure"></param>
        public static void ConfigureConsumer<T>(this IReceiveEndpointConfigurator configurator, IServiceContext context,
            Action<IConsumerConfigurator<T>> configure = null)
            where T : class, IConsumer
        {
            var registration = context.GetInstance<IRegistration>();

            registration.ConfigureConsumer(configurator, configure);
        }

        /// <summary>
        /// Configure all registered consumers on the receive endpoint
        /// </summary>
        /// <param name="configurator"></param>
        /// <param name="container"></param>
        public static void ConfigureConsumers(this IReceiveEndpointConfigurator configurator, IContainer container)
        {
            var registration = container.GetInstance<IRegistration>();

            registration.ConfigureConsumers(configurator);
        }

        /// <summary>
        /// Configure all registered consumers on the receive endpoint
        /// </summary>
        /// <param name="configurator"></param>
        /// <param name="container"></param>
        public static void ConfigureConsumers(this IReceiveEndpointConfigurator configurator, IServiceContext container)
        {
            var registration = container.GetInstance<IRegistration>();

            registration.ConfigureConsumers(configurator);
        }

        /// <summary>
        /// Configure a saga (or sagas) on the receive endpoint
        /// </summary>
        /// <param name="configurator"></param>
        /// <param name="container"></param>
        /// <param name="sagaTypes">The saga type(s) to configure</param>
        public static void ConfigureSaga(this IReceiveEndpointConfigurator configurator, IContainer container, params Type[] sagaTypes)
        {
            var registration = container.GetInstance<IRegistration>();

            foreach (var sagaType in sagaTypes)
            {
                registration.ConfigureSaga(sagaType, configurator);
            }
        }

        /// <summary>
        /// Configure a saga (or sagas) on the receive endpoint
        /// </summary>
        /// <param name="configurator"></param>
        /// <param name="context"></param>
        /// <param name="sagaTypes">The saga type(s) to configure</param>
        public static void ConfigureSaga(this IReceiveEndpointConfigurator configurator, IServiceContext context, params Type[] sagaTypes)
        {
            var registration = context.GetInstance<IRegistration>();

            foreach (var sagaType in sagaTypes)
            {
                registration.ConfigureSaga(sagaType, configurator);
            }
        }

        /// <summary>
        /// Configure a saga on the receive endpoint, with an optional configuration action
        /// </summary>
        /// <param name="configurator"></param>
        /// <param name="container"></param>
        /// <param name="configure"></param>
        public static void ConfigureSaga<T>(this IReceiveEndpointConfigurator configurator, IContainer container,
            Action<ISagaConfigurator<T>> configure = null)
            where T : class, ISaga
        {
            var registration = container.GetInstance<IRegistration>();

            registration.ConfigureSaga(configurator, configure);
        }

        /// <summary>
        /// Configure a saga on the receive endpoint, with an optional configuration action
        /// </summary>
        /// <param name="configurator"></param>
        /// <param name="context"></param>
        /// <param name="configure"></param>
        public static void ConfigureSaga<T>(this IReceiveEndpointConfigurator configurator, IServiceContext context,
            Action<ISagaConfigurator<T>> configure = null)
            where T : class, ISaga
        {
            var registration = context.GetInstance<IRegistration>();

            registration.ConfigureSaga(configurator, configure);
        }

        /// <summary>
        /// Configure all registered sagas on the receive endpoint
        /// </summary>
        /// <param name="configurator"></param>
        /// <param name="container"></param>
        public static void ConfigureSagas(this IReceiveEndpointConfigurator configurator, IContainer container)
        {
            var registration = container.GetInstance<IRegistration>();

            registration.ConfigureSagas(configurator);
        }

        /// <summary>
        /// Configure all registered sagas on the receive endpoint
        /// </summary>
        /// <param name="configurator"></param>
        /// <param name="context"></param>
        public static void ConfigureSagas(this IReceiveEndpointConfigurator configurator, IServiceContext context)
        {
            var registration = context.GetInstance<IRegistration>();

            registration.ConfigureSagas(configurator);
        }

        /// <summary>
        /// Configure the execute activity on the receive endpoint
        /// </summary>
        /// <param name="configurator"></param>
        /// <param name="container"></param>
        /// <param name="activityType"></param>
        public static void ConfigureExecuteActivity(this IReceiveEndpointConfigurator configurator, IContainer container, Type activityType)
        {
            var registration = container.GetInstance<IRegistration>();

            registration.ConfigureExecuteActivity(activityType, configurator);
        }

        /// <summary>
        /// Configure the execute activity on the receive endpoint
        /// </summary>
        /// <param name="configurator"></param>
        /// <param name="context"></param>
        /// <param name="activityType"></param>
        public static void ConfigureExecuteActivity(this IReceiveEndpointConfigurator configurator, IServiceContext context, Type activityType)
        {
            var registration = context.GetInstance<IRegistration>();

            registration.ConfigureExecuteActivity(activityType, configurator);
        }

        /// <summary>
        /// Configure an activity on two endpoints, one for execute, and the other for compensate
        /// </summary>
        /// <param name="executeEndpointConfigurator"></param>
        /// <param name="compensateEndpointConfigurator"></param>
        /// <param name="container"></param>
        /// <param name="activityType"></param>
        public static void ConfigureActivity(this IReceiveEndpointConfigurator executeEndpointConfigurator,
            IReceiveEndpointConfigurator compensateEndpointConfigurator, IContainer container, Type activityType)
        {
            var registration = container.GetInstance<IRegistration>();

            registration.ConfigureActivity(activityType, executeEndpointConfigurator, compensateEndpointConfigurator);
        }

        /// <summary>
        /// Configure an activity on two endpoints, one for execute, and the other for compensate
        /// </summary>
        /// <param name="executeEndpointConfigurator"></param>
        /// <param name="compensateEndpointConfigurator"></param>
        /// <param name="context"></param>
        /// <param name="activityType"></param>
        public static void ConfigureActivity(this IReceiveEndpointConfigurator executeEndpointConfigurator,
            IReceiveEndpointConfigurator compensateEndpointConfigurator, IServiceContext context, Type activityType)
        {
            var registration = context.GetInstance<IRegistration>();

            registration.ConfigureActivity(activityType, executeEndpointConfigurator, compensateEndpointConfigurator);
        }
    }
}
