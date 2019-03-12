namespace MassTransit.Definition
{
    using System;
    using Courier;


    public interface IExecuteActivityDefinition
    {
        /// <summary>
        /// The Activity type
        /// </summary>
        Type ActivityType { get; }

        /// <summary>
        /// The argument type
        /// </summary>
        Type ArgumentType { get; }

        /// <summary>
        /// Return the endpoint name for the execute activity
        /// </summary>
        /// <param name="formatter"></param>
        /// <returns></returns>
        string GetExecuteEndpointName(IEndpointNameFormatter formatter);
    }


    public interface IExecuteActivityDefinition<TActivity, TArguments> :
        IExecuteActivityDefinition
        where TActivity : class, ExecuteActivity<TArguments>
        where TArguments : class
    {
        /// <summary>
        /// Configure the execute activity
        /// </summary>
        /// <param name="endpointConfigurator"></param>
        /// <param name="executeActivityConfigurator"></param>
        void Configure(IReceiveEndpointConfigurator endpointConfigurator, IExecuteActivityConfigurator<TActivity, TArguments> executeActivityConfigurator);
    }
}
