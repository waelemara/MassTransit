﻿namespace MassTransit.AutomatonymousWindsorIntegration.Registration
{
    using Automatonymous;
    using Automatonymous.Registration;
    using Castle.MicroKernel;


    public class WindsorSagaStateMachineFactory :
        ISagaStateMachineFactory
    {
        readonly IKernel _container;

        public WindsorSagaStateMachineFactory(IKernel container)
        {
            _container = container;
        }
        public SagaStateMachine<T> CreateStateMachine<T>() where T : class, SagaStateMachineInstance
        {
            return _container.Resolve<SagaStateMachine<T>>();
        }
    }
}
