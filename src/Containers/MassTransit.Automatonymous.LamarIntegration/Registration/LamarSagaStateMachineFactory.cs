﻿namespace MassTransit.AutomatonymousLamarIntegration.Registration
{
    using System;
    using Automatonymous;
    using Automatonymous.Registration;
    using Microsoft.Extensions.DependencyInjection;


    public class LamarSagaStateMachineFactory :
        ISagaStateMachineFactory
    {
        readonly IServiceProvider _container;

        public LamarSagaStateMachineFactory(IServiceProvider container)
        {
            _container = container;
        }

        SagaStateMachine<T> ISagaStateMachineFactory.CreateStateMachine<T>()
        {
            return _container.GetService<SagaStateMachine<T>>();
        }
    }
}
