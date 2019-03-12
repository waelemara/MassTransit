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
    using System.Reflection;
    using Automatonymous;
    using AutomatonymousWindsorIntegration.Registration;
    using WindsorIntegration;


    public static class WindsorAutomatonymousRegistrationExtensions
    {
        /// <summary>
        /// Adds a SagaStateMachine to the registry, using the factory method, and updates the registrar prior to registering so that the default
        /// saga registrar isn't notified.
        /// </summary>
        /// <param name="configurator"></param>
        /// <typeparam name="TStateMachine"></typeparam>
        /// <typeparam name="TInstance"></typeparam>
        public static void AddSagaStateMachine<TStateMachine, TInstance>(this IWindsorContainerConfigurator configurator)
            where TStateMachine : class, SagaStateMachine<TInstance>
            where TInstance : class, SagaStateMachineInstance
        {
            var registrar = new WindsorSagaStateMachineRegistrar(configurator.Container);

            configurator.AddSagaStateMachine<TStateMachine, TInstance>(registrar);
        }

        /// <summary>
        /// Adds SagaStateMachines to the registry, using the factory method, and updates the registrar prior to registering so that the default
        /// saga registrar isn't notified.
        /// </summary>
        /// <param name="configurator"></param>
        /// <param name="assemblies">The assemblies to scan for state machines</param>
        public static void AddSagaStateMachines(this IWindsorContainerConfigurator configurator, params Assembly[] assemblies)
        {
            var registrar = new WindsorSagaStateMachineRegistrar(configurator.Container);

            configurator.AddSagaStateMachines(registrar, assemblies);
        }

        /// <summary>
        /// Adds all sagas in the specified assemblies matching the namespace. If you are using both state machine and regular sagas, be
        /// sure to call AddSagaStateMachinesFromNamespaceContaining prior to calling this one.
        /// </summary>
        /// <param name="configurator"></param>
        /// <typeparam name="T">The anchor type</typeparam>
        public static void AddSagaStateMachinesFromNamespaceContaining<T>(this IWindsorContainerConfigurator configurator)
        {
            var registrar = new WindsorSagaStateMachineRegistrar(configurator.Container);

            configurator.AddSagaStateMachinesFromNamespaceContaining(registrar, typeof(T));
        }

        /// <summary>
        /// Adds all sagas in the specified assemblies matching the namespace. If you are using both state machine and regular sagas, be
        /// sure to call AddSagaStateMachinesFromNamespaceContaining prior to calling this one.
        /// </summary>
        /// <param name="configurator"></param>
        /// <param name="type">The type to use to identify the assembly and namespace to scan</param>
        public static void AddSagaStateMachinesFromNamespaceContaining(this IWindsorContainerConfigurator configurator, Type type)
        {
            var registrar = new WindsorSagaStateMachineRegistrar(configurator.Container);

            configurator.AddSagaStateMachinesFromNamespaceContaining(registrar, type);
        }

        /// <summary>
        /// Adds SagaStateMachines to the registry, using the factory method, and updates the registrar prior to registering so that the default
        /// saga registrar isn't notified.
        /// </summary>
        /// <param name="configurator"></param>
        /// <param name="types">The state machine types to add</param>
        public static void AddSagaStateMachines(this IWindsorContainerConfigurator configurator, params Type[] types)
        {
            var registrar = new WindsorSagaStateMachineRegistrar(configurator.Container);

            configurator.AddSagaStateMachines(registrar, types);
        }
    }
}
