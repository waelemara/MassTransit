namespace MassTransit.Definition
{
    using System;
    using Saga;


    public interface ISagaDefinition
    {
        /// <summary>
        /// The saga type
        /// </summary>
        Type SagaType { get; }

        /// <summary>
        /// Return the endpoint name for the consumer, using the specified formatter if necessary.
        /// </summary>
        /// <param name="formatter"></param>
        /// <returns></returns>
        string GetEndpointName(IEndpointNameFormatter formatter);
    }


    public interface ISagaDefinition<TSaga> :
        ISagaDefinition
        where TSaga : class, ISaga
    {
        /// <summary>
        /// Configure the consumer on the receive endpoint
        /// </summary>
        /// <param name="endpointConfigurator">The receive endpoint configurator for the consumer</param>
        /// <param name="sagaConfigurator">The consumer configurator</param>
        void Configure(IReceiveEndpointConfigurator endpointConfigurator, ISagaConfigurator<TSaga> sagaConfigurator);
    }
}
