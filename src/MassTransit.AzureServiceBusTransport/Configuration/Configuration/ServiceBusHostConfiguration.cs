﻿// Copyright 2007-2018 Chris Patterson, Dru Sellers, Travis Smith, et. al.
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
namespace MassTransit.AzureServiceBusTransport.Configuration
{
    using System;
    using System.Threading.Tasks;
    using GreenPipes;
    using MassTransit.Configuration;
    using MassTransit.Topology;
    using Pipeline;
    using Settings;
    using Topology;
    using Transport;
    using Transports;
    using Util;


    public class ServiceBusHostConfiguration :
        IServiceBusHostConfiguration
    {
        readonly IServiceBusBusConfiguration _busConfiguration;
        readonly ServiceBusHost _host;
        readonly ServiceBusHostSettings _hostSettings;
        readonly IServiceBusHostTopology _hostTopology;

        public ServiceBusHostConfiguration(IServiceBusBusConfiguration busConfiguration, ServiceBusHostSettings hostSettings,
            IServiceBusHostTopology hostTopology)
        {
            _busConfiguration = busConfiguration;
            _hostSettings = hostSettings;
            _hostTopology = hostTopology;

            _host = new ServiceBusHost(this);
        }

        IBusHostControl IHostConfiguration.Host => _host;
        Uri IHostConfiguration.HostAddress => _hostSettings.ServiceUri;
        IHostTopology IHostConfiguration.Topology => _hostTopology;

        IServiceBusBusConfiguration IServiceBusHostConfiguration.BusConfiguration => _busConfiguration;
        IServiceBusHostControl IServiceBusHostConfiguration.Host => _host;
        ServiceBusHostSettings IServiceBusHostConfiguration.Settings => _hostSettings;
        IServiceBusHostTopology IServiceBusHostConfiguration.Topology => _hostTopology;

        public bool Matches(Uri address)
        {
            return _hostSettings.ServiceUri.GetLeftPart(UriPartial.Authority)
                .Equals(address.GetLeftPart(UriPartial.Authority), StringComparison.OrdinalIgnoreCase);
        }

        public Task<ISendTransport> CreateSendTransport(Uri address)
        {
            var settings = _hostTopology.SendTopology.GetSendSettings(address);

            var endpointContextSupervisor = CreateQueueSendEndpointContextSupervisor(settings);

            var transport = new ServiceBusSendTransport(endpointContextSupervisor, address);

            _host.Add(transport);

            return Task.FromResult<ISendTransport>(transport);
        }

        public Task<ISendTransport> CreatePublishTransport<T>()
            where T : class
        {
            var publishTopology = _hostTopology.Publish<T>();

            if (!publishTopology.TryGetPublishAddress(_hostSettings.ServiceUri, out Uri publishAddress))
                throw new ArgumentException($"The type did not return a valid publish address: {TypeMetadataCache<T>.ShortName}");

            var settings = publishTopology.GetSendSettings();

            var endpointContextSupervisor = CreateTopicSendEndpointContextSupervisor(settings);

            var transport = new ServiceBusSendTransport(endpointContextSupervisor, publishAddress);

            _host.Add(transport);

            return Task.FromResult<ISendTransport>(transport);
        }

        public IServiceBusReceiveEndpointConfiguration CreateReceiveEndpointConfiguration(string queueName)
        {
            return new ServiceBusReceiveEndpointConfiguration(this, _busConfiguration.CreateEndpointConfiguration(), queueName);
        }

        public IServiceBusSubscriptionEndpointConfiguration CreateSubscriptionEndpointConfiguration(SubscriptionEndpointSettings settings)
        {
            return new ServiceBusSubscriptionEndpointConfiguration(this, _busConfiguration.CreateEndpointConfiguration(), settings);
        }

        ISendEndpointContextSupervisor CreateQueueSendEndpointContextSupervisor(SendSettings settings)
        {
            IPipe<NamespaceContext> namespacePipe = CreateConfigureTopologyPipe(settings);

            var contextFactory = new QueueSendEndpointContextFactory(_host.MessagingFactoryContextSupervisor, _host.NamespaceContextSupervisor,
                Pipe.Empty<MessagingFactoryContext>(),
                namespacePipe, settings);

            return new SendEndpointContextSupervisor(contextFactory);
        }

        ISendEndpointContextSupervisor CreateTopicSendEndpointContextSupervisor(SendSettings settings)
        {
            IPipe<NamespaceContext> namespacePipe = CreateConfigureTopologyPipe(settings);

            var contextFactory = new TopicSendEndpointContextFactory(_host.MessagingFactoryContextSupervisor, _host.NamespaceContextSupervisor,
                Pipe.Empty<MessagingFactoryContext>(),
                namespacePipe, settings);

            return new SendEndpointContextSupervisor(contextFactory);
        }

        IPipe<NamespaceContext> CreateConfigureTopologyPipe(SendSettings settings)
        {
            var configureTopologyFilter = new ConfigureTopologyFilter<SendSettings>(settings, settings.GetBrokerTopology(), false, _host.Stopping);

            IPipe<NamespaceContext> namespacePipe = configureTopologyFilter.ToPipe();
            return namespacePipe;
        }
    }
}