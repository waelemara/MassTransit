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
namespace MassTransit.ActiveMqTransport.Configuration
{
    using System.Collections.Generic;
    using System.Linq;
    using GreenPipes;
    using MassTransit.Configuration;
    using MassTransit.Topology;
    using MassTransit.Topology.Observers;
    using MassTransit.Topology.Topologies;
    using Topology;
    using Topology.Topologies;


    public class ActiveMqTopologyConfiguration :
        IActiveMqTopologyConfiguration
    {
        readonly IActiveMqConsumeTopologyConfigurator _consumeTopology;
        readonly IMessageTopologyConfigurator _messageTopology;
        readonly IActiveMqPublishTopologyConfigurator _publishTopology;
        readonly IActiveMqSendTopologyConfigurator _sendTopology;

        public ActiveMqTopologyConfiguration(IMessageTopologyConfigurator messageTopology)
        {
            _messageTopology = messageTopology;

            _sendTopology = new ActiveMqSendTopology(ActiveMqEntityNameValidator.Validator);
            _sendTopology.ConnectSendTopologyConfigurationObserver(new DelegateSendTopologyConfigurationObserver(GlobalTopology.Send));

            _publishTopology = new ActiveMqPublishTopology(messageTopology);
            _publishTopology.ConnectPublishTopologyConfigurationObserver(new DelegatePublishTopologyConfigurationObserver(GlobalTopology.Publish));

            var observer = new PublishToSendTopologyConfigurationObserver(_sendTopology);
            _publishTopology.ConnectPublishTopologyConfigurationObserver(observer);

            _consumeTopology = new ActiveMqConsumeTopology(messageTopology, _publishTopology);
        }

        public ActiveMqTopologyConfiguration(IActiveMqTopologyConfiguration topologyConfiguration)
        {
            _messageTopology = topologyConfiguration.Message;
            _sendTopology = topologyConfiguration.Send;
            _publishTopology = topologyConfiguration.Publish;

            _consumeTopology = new ActiveMqConsumeTopology(topologyConfiguration.Message, topologyConfiguration.Publish);
        }

        IMessageTopologyConfigurator ITopologyConfiguration.Message => _messageTopology;
        ISendTopologyConfigurator ITopologyConfiguration.Send => _sendTopology;
        IPublishTopologyConfigurator ITopologyConfiguration.Publish => _publishTopology;
        IConsumeTopologyConfigurator ITopologyConfiguration.Consume => _consumeTopology;

        IActiveMqPublishTopologyConfigurator IActiveMqTopologyConfiguration.Publish => _publishTopology;
        IActiveMqSendTopologyConfigurator IActiveMqTopologyConfiguration.Send => _sendTopology;
        IActiveMqConsumeTopologyConfigurator IActiveMqTopologyConfiguration.Consume => _consumeTopology;

        public IEnumerable<ValidationResult> Validate()
        {
            return _sendTopology.Validate()
                .Concat(_publishTopology.Validate())
                .Concat(_consumeTopology.Validate());
        }
    }
}