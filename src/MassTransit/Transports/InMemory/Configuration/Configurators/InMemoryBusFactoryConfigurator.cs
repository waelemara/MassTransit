// Copyright 2007-2018 Chris Patterson, Dru Sellers, Travis Smith, et. al.
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
namespace MassTransit.Transports.InMemory.Configurators
{
    using System;
    using BusConfigurators;
    using Configuration;
    using Definition;
    using MassTransit.Builders;
    using Topology.Configurators;


    public class InMemoryBusFactoryConfigurator :
        BusFactoryConfigurator,
        IInMemoryBusFactoryConfigurator,
        IBusFactory
    {
        readonly IInMemoryEndpointConfiguration _busEndpointConfiguration;
        readonly IInMemoryBusConfiguration _configuration;
        readonly IInMemoryHostConfiguration _inMemoryHostConfiguration;

        public InMemoryBusFactoryConfigurator(IInMemoryBusConfiguration configuration, IInMemoryEndpointConfiguration busEndpointConfiguration,
            Uri baseAddress = null)
            : base(configuration, busEndpointConfiguration)
        {
            _configuration = configuration;
            _busEndpointConfiguration = busEndpointConfiguration;

            _inMemoryHostConfiguration = _configuration.CreateHostConfiguration(baseAddress, Environment.ProcessorCount);
        }

        public IBusControl CreateBus()
        {
            var busQueueName = _configuration.Topology.Consume.CreateTemporaryQueueName("bus");

            var busReceiveEndpointConfiguration = _configuration.CreateReceiveEndpointConfiguration(busQueueName, _busEndpointConfiguration);

            var builder = new ConfigurationBusBuilder(_configuration, busReceiveEndpointConfiguration, BusObservable);

            ApplySpecifications(builder);

            return builder.Build();
        }

        public void Publish<T>(Action<IInMemoryMessagePublishTopologyConfigurator<T>> configureTopology)
            where T : class
        {
            IInMemoryMessagePublishTopologyConfigurator<T> configurator = _configuration.Topology.Publish.GetMessageTopology<T>();

            configureTopology?.Invoke(configurator);
        }

        public new IInMemoryPublishTopologyConfigurator PublishTopology => _configuration.Topology.Publish;

        public void ReceiveEndpoint(string queueName, Action<IInMemoryReceiveEndpointConfigurator> configureEndpoint)
        {
            var configuration = _inMemoryHostConfiguration.CreateReceiveEndpointConfiguration(queueName);

            ConfigureReceiveEndpoint(configuration, configuration.Configurator, configureEndpoint);
        }

        public void ReceiveEndpoint(IEndpointDefinition definition, IEndpointNameFormatter endpointNameFormatter,
            Action<IInMemoryReceiveEndpointConfigurator> configureEndpoint = null)
        {
            var queueName = definition.GetEndpointName(endpointNameFormatter ?? DefaultEndpointNameFormatter.Instance);

            ReceiveEndpoint(queueName, x => x.Apply(definition, configureEndpoint));
        }

        public override void ReceiveEndpoint(IEndpointDefinition definition, IEndpointNameFormatter endpointNameFormatter,
            Action<IReceiveEndpointConfigurator> configureEndpoint = null)
        {
            ReceiveEndpoint(definition, endpointNameFormatter, configureEndpoint);
        }

        public override void ReceiveEndpoint(string queueName, Action<IReceiveEndpointConfigurator> configureEndpoint)
        {
            ReceiveEndpoint(queueName, configureEndpoint);
        }

        public int TransportConcurrencyLimit
        {
            set => _inMemoryHostConfiguration.Host.TransportConcurrencyLimit = value;
        }

        public IInMemoryHost Host => _inMemoryHostConfiguration.Host;
    }
}
