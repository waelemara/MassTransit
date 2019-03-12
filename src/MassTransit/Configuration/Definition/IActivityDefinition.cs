namespace MassTransit.Definition
{
    using System;
    using Courier;


    public interface IActivityDefinition :
        IExecuteActivityDefinition
    {
        /// <summary>
        /// The log type
        /// </summary>
        Type LogType { get; }

        /// <summary>
        /// Return the endpoint name for the compensate activity
        /// </summary>
        /// <param name="formatter"></param>
        /// <returns></returns>
        string GetCompensateEndpointName(IEndpointNameFormatter formatter);
    }


    public interface IActivityDefinition<TActivity, TArguments, TLog> :
        IActivityDefinition,
        IExecuteActivityDefinition<TActivity, TArguments>
        where TActivity : class, Activity<TArguments, TLog>
        where TLog : class
        where TArguments : class
    {
        /// <summary>
        /// Configure the compensate activity
        /// </summary>
        /// <param name="endpointConfigurator"></param>
        /// <param name="compensateActivityConfigurator"></param>
        void Configure(IReceiveEndpointConfigurator endpointConfigurator, ICompensateActivityConfigurator<TActivity, TLog> compensateActivityConfigurator);
    }
}
