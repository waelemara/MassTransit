﻿// Copyright 2007-2017 Chris Patterson, Dru Sellers, Travis Smith, et. al.
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
namespace MassTransit.Azure.ServiceBus.Core.Configuration
{
    using System.Collections.Generic;
    using System.Linq;
    using GreenPipes;
    using MassTransit.Configuration;
    using MassTransit.Topology;
    using MassTransit.Topology.Observers;
    using MassTransit.Topology.Topologies;
    using Topology.Configuration;
    using Topology.Conventions.PartitionKey;
    using Topology.Conventions.SessionId;
    using Topology.Topologies;


    public class ServiceBusTopologyConfiguration :
        IServiceBusTopologyConfiguration
    {
        readonly ServiceBusConsumeTopology _consumeTopology;
        readonly IMessageTopologyConfigurator _messageTopology;
        readonly IServiceBusPublishTopologyConfigurator _publishTopology;
        readonly IServiceBusSendTopologyConfigurator _sendTopology;

        public ServiceBusTopologyConfiguration(IMessageTopologyConfigurator messageTopology)
        {
            _messageTopology = messageTopology;

            _sendTopology = new ServiceBusSendTopology();
            _sendTopology.ConnectSendTopologyConfigurationObserver(new DelegateSendTopologyConfigurationObserver(GlobalTopology.Send));
            _sendTopology.AddConvention(new SessionIdSendTopologyConvention());
            _sendTopology.AddConvention(new PartitionKeySendTopologyConvention());

            _publishTopology = new ServiceBusPublishTopology(messageTopology);
            _publishTopology.ConnectPublishTopologyConfigurationObserver(new DelegatePublishTopologyConfigurationObserver(GlobalTopology.Publish));

            var observer = new PublishToSendTopologyConfigurationObserver(_sendTopology);
            _publishTopology.ConnectPublishTopologyConfigurationObserver(observer);

            _consumeTopology = new ServiceBusConsumeTopology(messageTopology, _publishTopology);
        }

        public ServiceBusTopologyConfiguration(IServiceBusTopologyConfiguration topologyConfiguration)
        {
            _messageTopology = topologyConfiguration.Message;
            _sendTopology = topologyConfiguration.Send;
            _publishTopology = topologyConfiguration.Publish;

            _consumeTopology = new ServiceBusConsumeTopology(topologyConfiguration.Message, topologyConfiguration.Publish);
        }

        IMessageTopologyConfigurator ITopologyConfiguration.Message => _messageTopology;
        ISendTopologyConfigurator ITopologyConfiguration.Send => _sendTopology;
        IPublishTopologyConfigurator ITopologyConfiguration.Publish => _publishTopology;
        IConsumeTopologyConfigurator ITopologyConfiguration.Consume => _consumeTopology;

        IServiceBusPublishTopologyConfigurator IServiceBusTopologyConfiguration.Publish => _publishTopology;
        IServiceBusSendTopologyConfigurator IServiceBusTopologyConfiguration.Send => _sendTopology;
        IServiceBusConsumeTopologyConfigurator IServiceBusTopologyConfiguration.Consume => _consumeTopology;

        public IEnumerable<ValidationResult> Validate()
        {
            return _sendTopology.Validate()
                .Concat(_publishTopology.Validate())
                .Concat(_consumeTopology.Validate());
        }
    }
}