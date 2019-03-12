﻿namespace MassTransit.AmazonSqsTransport.Configuration.Configuration
{
    using System;
    using System.Collections.Generic;
    using Builders;
    using GreenPipes;
    using GreenPipes.Agents;
    using GreenPipes.Builders;
    using GreenPipes.Configurators;
    using MassTransit.Configuration;
    using MassTransit.Pipeline.Filters;
    using Pipeline;
    using Topology;
    using Topology.Settings;
    using Transport;


    public class AmazonSqsReceiveEndpointConfiguration :
        ReceiveEndpointConfiguration,
        IAmazonSqsReceiveEndpointConfiguration,
        IAmazonSqsReceiveEndpointConfigurator
    {
        readonly IBuildPipeConfigurator<ConnectionContext> _connectionConfigurator;
        readonly IAmazonSqsEndpointConfiguration _endpointConfiguration;
        readonly IAmazonSqsHostConfiguration _hostConfiguration;
        readonly Lazy<Uri> _inputAddress;
        readonly IBuildPipeConfigurator<ClientContext> _clientConfigurator;
        readonly QueueReceiveSettings _settings;

        public AmazonSqsReceiveEndpointConfiguration(IAmazonSqsHostConfiguration hostConfiguration, string queueName,
            IAmazonSqsEndpointConfiguration endpointConfiguration)
            : this(hostConfiguration, endpointConfiguration)
        {
            SubscribeMessageTopics = true;

            _settings = new QueueReceiveSettings(queueName, true, false);
        }

        public AmazonSqsReceiveEndpointConfiguration(IAmazonSqsHostConfiguration hostConfiguration, QueueReceiveSettings settings,
            IAmazonSqsEndpointConfiguration endpointConfiguration)
            : this(hostConfiguration, endpointConfiguration)
        {
            _settings = settings;
        }

        AmazonSqsReceiveEndpointConfiguration(IAmazonSqsHostConfiguration hostConfiguration, IAmazonSqsEndpointConfiguration endpointConfiguration)
            : base(hostConfiguration, endpointConfiguration)
        {
            _hostConfiguration = hostConfiguration;
            _endpointConfiguration = endpointConfiguration;

            _connectionConfigurator = new PipeConfigurator<ConnectionContext>();
            _clientConfigurator = new PipeConfigurator<ClientContext>();

            HostAddress = hostConfiguration.Host.Address;

            _inputAddress = new Lazy<Uri>(FormatInputAddress);
        }

        public IAmazonSqsReceiveEndpointConfigurator Configurator => this;

        public IAmazonSqsBusConfiguration BusConfiguration => _hostConfiguration.BusConfiguration;
        public IAmazonSqsHostConfiguration HostConfiguration => _hostConfiguration;

        public IAmazonSqsHostControl Host => _hostConfiguration.Host;

        public bool SubscribeMessageTopics { get; set; }

        public ReceiveSettings Settings => _settings;

        public override Uri HostAddress { get; }

        public override Uri InputAddress => _inputAddress.Value;

        IAmazonSqsTopologyConfiguration IAmazonSqsEndpointConfiguration.Topology => _endpointConfiguration.Topology;

        public override IReceiveEndpoint Build()
        {
            var builder = new AmazonSqsReceiveEndpointBuilder(this);

            ApplySpecifications(builder);

            var receiveEndpointContext = builder.CreateReceiveEndpointContext();

            _clientConfigurator.UseFilter(new ConfigureTopologyFilter<ReceiveSettings>(_settings, receiveEndpointContext.BrokerTopology));

            IAgent consumerAgent;
            if (_hostConfiguration.BusConfiguration.DeployTopologyOnly)
            {
                var transportReadyFilter = new TransportReadyFilter<ClientContext>(receiveEndpointContext);
                _clientConfigurator.UseFilter(transportReadyFilter);

                consumerAgent = transportReadyFilter;
            }
            else
            {
                if (_settings.PurgeOnStartup)
                    _clientConfigurator.UseFilter(new PurgeOnStartupFilter(_settings.EntityName));

                var consumerFilter = new AmazonSqsConsumerFilter(receiveEndpointContext);

                _clientConfigurator.UseFilter(consumerFilter);

                consumerAgent = consumerFilter;
            }

            IFilter<ConnectionContext> clientFilter = new ReceiveClientFilter(_clientConfigurator.Build());

            _connectionConfigurator.UseFilter(clientFilter);

            var transport = new SqsReceiveTransport(_hostConfiguration.Host, _settings, _connectionConfigurator.Build(), receiveEndpointContext);

            transport.Add(consumerAgent);

            return CreateReceiveEndpoint(_settings.EntityName ?? NewId.Next().ToString(), transport, receiveEndpointContext);
        }

        IAmazonSqsHost IAmazonSqsReceiveEndpointConfigurator.Host => Host;

        public bool Durable
        {
            set
            {
                _settings.Durable = value;

                Changed("Durable");
            }
        }

        public bool AutoDelete
        {
            set
            {
                _settings.AutoDelete = value;

                Changed("AutoDelete");
            }
        }

        public ushort PrefetchCount
        {
            set => _settings.PrefetchCount = value;
        }

        public ushort WaitTimeSeconds
        {
            set => _settings.WaitTimeSeconds = value;
        }

        public bool PurgeOnStartup
        {
            set => _settings.PurgeOnStartup = value;
        }

        public IDictionary<string, object> QueueAttributes => _settings.QueueAttributes;
        public IDictionary<string, object> QueueSubscriptionAttributes => _settings.QueueSubscriptionAttributes;

        public void Subscribe(string topicName, Action<ITopicSubscriptionConfigurator> configure = null)
        {
            if (topicName == null)
                throw new ArgumentNullException(nameof(topicName));

            _endpointConfiguration.Topology.Consume.Bind(topicName, configure);
        }

        public void Subscribe<T>(Action<ITopicSubscriptionConfigurator> configure = null)
            where T : class
        {
            _endpointConfiguration.Topology.Consume.GetMessageTopology<T>().Subscribe(configure);
        }

        public void ConfigureClient(Action<IPipeConfigurator<ClientContext>> configure)
        {
            configure?.Invoke(_clientConfigurator);
        }

        public void ConfigureConnection(Action<IPipeConfigurator<ConnectionContext>> configure)
        {
            configure?.Invoke(_connectionConfigurator);
        }

        Uri FormatInputAddress()
        {
            return _settings.GetInputAddress(_hostConfiguration.Host.Settings.HostAddress);
        }

        protected override bool IsAlreadyConfigured()
        {
            return _inputAddress.IsValueCreated || base.IsAlreadyConfigured();
        }

        public override IEnumerable<ValidationResult> Validate()
        {
            var queueName = $"{_settings.EntityName}";

            if (!AmazonSqsEntityNameValidator.Validator.IsValidEntityName(_settings.EntityName))
                yield return this.Failure(queueName, "must be a valid queue name");

            if (_settings.PurgeOnStartup)
                yield return this.Warning(queueName, "Existing messages in the queue will be purged on service start");

            foreach (var result in base.Validate())
                yield return result.WithParentKey(queueName);
        }
    }
}
