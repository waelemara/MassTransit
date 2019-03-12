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
namespace MassTransit.AmazonSqsTransport.Topology.Builders
{
    using System.Collections.Generic;
    using Entities;


    public class PublishEndpointBrokerTopologyBuilder :
        BrokerTopologyBuilder,
        IPublishEndpointBrokerTopologyBuilder
    {
        readonly PublishBrokerTopologyOptions _options;

        public PublishEndpointBrokerTopologyBuilder(PublishBrokerTopologyOptions options = PublishBrokerTopologyOptions.MaintainHierarchy)
        {
            _options = options;
        }

        /// <summary>
        /// The exchange to which the published message is sent
        /// </summary>
        public TopicHandle Topic { get; set; }

        public IPublishEndpointBrokerTopologyBuilder CreateImplementedBuilder()
        {
            if (_options.HasFlag(PublishBrokerTopologyOptions.MaintainHierarchy))
                return new ImplementedBuilder(this, _options);

            return this;
        }

        public BrokerTopology BuildBrokerTopology()
        {
            return new AmazonSqsBrokerTopology(Topics, Queues, QueueSubscriptions, TopicSubscriptions);
        }


        protected class ImplementedBuilder :
            IPublishEndpointBrokerTopologyBuilder
        {
            readonly IPublishEndpointBrokerTopologyBuilder _builder;
            readonly PublishBrokerTopologyOptions _options;

            public ImplementedBuilder(IPublishEndpointBrokerTopologyBuilder builder, PublishBrokerTopologyOptions options)
            {
                _builder = builder;
                _options = options;
            }

            public TopicHandle Topic { get; set; }

            public IPublishEndpointBrokerTopologyBuilder CreateImplementedBuilder()
            {
                if (_options.HasFlag(PublishBrokerTopologyOptions.MaintainHierarchy))
                    return new ImplementedBuilder(this, _options);

                return this;
            }

            public TopicHandle CreateTopic(string name, bool durable, bool autoDelete)
            {
                return _builder.CreateTopic(name, durable, autoDelete);
            }

            public QueueHandle CreateQueue(string name, bool durable, bool autoDelete, IDictionary<string, object> attributes = null)
            {
                return _builder.CreateQueue(name, durable, autoDelete, attributes);
            }

            public QueueSubscriptionHandle CreateQueueSubscription(TopicHandle topic, QueueHandle queue)
            {
                return _builder.CreateQueueSubscription(topic, queue);
            }

            public TopicSubscriptionHandle CreateTopicSubscription(TopicHandle source, TopicHandle destination)
            {
                return _builder.CreateTopicSubscription(source, destination);
            }
        }
    }
}