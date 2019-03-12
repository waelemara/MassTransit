namespace MassTransit.Definition
{
    using System;
    using ConsumeConfigurators;


    /// <summary>
    /// A consumer definition defines the configuration for a consumer, which can be used by the automatic registration code to
    /// configure the consumer on a receive endpoint.
    /// </summary>
    /// <typeparam name="TConsumer"></typeparam>
    public class ConsumerDefinition<TConsumer> :
        IConsumerDefinition<TConsumer>
        where TConsumer : class, IConsumer
    {
        int? _concurrentMessageLimit;
        string _endpointName;

        protected ConsumerDefinition()
        {
            // TODO if the partitionKey is specified, use a partition filter instead of a semaphore
        }

        /// <summary>
        /// Specify the endpoint name (which may be a queue, or a subscription, depending upon the transport) on which the consumer
        /// should be configured.
        /// </summary>
        protected string EndpointName
        {
            set => _endpointName = value;
        }

        /// Set the concurrent message limit for the consumer, which limits how many consumers are able to concurrently
        /// consume messages. 
        protected int ConcurrentMessageLimit
        {
            set => _concurrentMessageLimit = value;
        }

        void IConsumerDefinition<TConsumer>.Configure(IReceiveEndpointConfigurator endpointConfigurator, IConsumerConfigurator<TConsumer> consumerConfigurator)
        {
            if (_concurrentMessageLimit.HasValue)
                consumerConfigurator.UseConcurrentMessageLimit(_concurrentMessageLimit.Value);

            ConfigureConsumer(endpointConfigurator, consumerConfigurator);
        }

        Type IConsumerDefinition.ConsumerType => typeof(TConsumer);

        string IConsumerDefinition.GetEndpointName(IEndpointNameFormatter formatter)
        {
            return string.IsNullOrWhiteSpace(_endpointName)
                ? _endpointName = formatter.Consumer<TConsumer>()
                : _endpointName;
        }

        /// <summary>
        /// Define a message handled by the consumer
        /// </summary>
        /// <param name="configure"></param>
        /// <typeparam name="T">The message type</typeparam>
        protected void Message<T>(Action<IConsumerMessageDefinitionConfigurator<TConsumer, T>> configure = null)
            where T : class
        {
        }

        /// <summary>
        /// Define the request message handled by the consumer
        /// </summary>
        /// <param name="configure"></param>
        /// <typeparam name="T">The message type</typeparam>
        protected void Request<T>(Action<IConsumerRequestDefinitionConfigurator<TConsumer, T>> configure = null)
            where T : class
        {
        }

        /// <summary>
        /// Called when the consumer is being configured on the endpoint. Configuration only applies to this consumer, and does not apply to
        /// the endpoint.
        /// </summary>
        /// <param name="endpointConfigurator">The receive endpoint configurator for the consumer</param>
        /// <param name="consumerConfigurator">The consumer configurator</param>
        protected virtual void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator, IConsumerConfigurator<TConsumer> consumerConfigurator)
        {
        }
    }
}
