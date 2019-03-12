namespace MassTransit.Definition
{
    using System;
    using Courier;


    public class EndpointExecuteActivityDefinition<TActivity, TArguments> :
        IExecuteActivityDefinition<TActivity, TArguments>
        where TActivity : class, ExecuteActivity<TArguments>
        where TArguments : class
    {
        readonly IEndpointDefinition<ExecuteActivity<TArguments>> _endpointDefinition;

        public EndpointExecuteActivityDefinition(IEndpointDefinition<ExecuteActivity<TArguments>> endpointDefinition)
        {
            _endpointDefinition = endpointDefinition;
        }

        void IExecuteActivityDefinition<TActivity, TArguments>.Configure(IReceiveEndpointConfigurator endpointConfigurator,
            IExecuteActivityConfigurator<TActivity, TArguments> consumerConfigurator)
        {
        }

        Type IExecuteActivityDefinition.ActivityType => typeof(TActivity);
        Type IExecuteActivityDefinition.ArgumentType => typeof(TArguments);

        string IExecuteActivityDefinition.GetExecuteEndpointName(IEndpointNameFormatter formatter)
        {
            return _endpointDefinition.GetEndpointName(formatter);
        }
    }
}
