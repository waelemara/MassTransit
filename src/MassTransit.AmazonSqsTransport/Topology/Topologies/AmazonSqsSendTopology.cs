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
namespace MassTransit.AmazonSqsTransport.Topology.Topologies
{
    using System;
    using System.Globalization;
    using Configuration;
    using MassTransit.Topology;
    using MassTransit.Topology.Topologies;
    using Settings;
    using Util;


    public class AmazonSqsSendTopology :
        SendTopology,
        IAmazonSqsSendTopologyConfigurator
    {
        const string FifoSuffix = ".fifo";

        public AmazonSqsSendTopology(IEntityNameValidator validator)
        {
            EntityNameValidator = validator;
        }

        public IEntityNameValidator EntityNameValidator { get; }

        IAmazonSqsMessageSendTopologyConfigurator<T> IAmazonSqsSendTopology.GetMessageTopology<T>()
        {
            IMessageSendTopologyConfigurator<T> configurator = base.GetMessageTopology<T>();

            return configurator as IAmazonSqsMessageSendTopologyConfigurator<T>;
        }

        public SendSettings GetSendSettings(Uri address)
        {
            var name = address.AbsolutePath.Substring(1);
            string[] pathSegments = name.Split('/');
            if (pathSegments.Length == 2)
                name = pathSegments[1];

            if (name == "*")
                throw new ArgumentException("Cannot send to a dynamic address");

            EntityNameValidator.ThrowIfInvalidEntityName(name);

            var isTemporary = address.Query.GetValueFromQueryString("temporary", false);
            var durable = address.Query.GetValueFromQueryString("durable", !isTemporary);
            var autoDelete = address.Query.GetValueFromQueryString("autodelete", isTemporary);

            return new QueueSendSettings(name, durable, autoDelete);
        }

        public ErrorSettings GetErrorSettings(EntitySettings settings)
        {
            string entityName;

            if (settings.EntityName.EndsWith(FifoSuffix, true, CultureInfo.InvariantCulture))
            {
                entityName = settings.EntityName.Substring(0, settings.EntityName.Length - FifoSuffix.Length) + "_error" + FifoSuffix;
            }
            else
            {
                entityName = settings.EntityName + "_error";
            }

            return new QueueErrorSettings(settings, entityName);
        }

        public DeadLetterSettings GetDeadLetterSettings(EntitySettings settings)
        {
            string entityName;

            if (settings.EntityName.EndsWith(FifoSuffix, true, CultureInfo.InvariantCulture))
            {
                entityName = settings.EntityName.Substring(0, settings.EntityName.Length - FifoSuffix.Length) + "_skipped" + FifoSuffix;
            }
            else
            {
                entityName = settings.EntityName + "_skipped";
            }

            return new QueueDeadLetterSettings(settings, entityName);
        }

        protected override IMessageSendTopologyConfigurator CreateMessageTopology<T>(Type type)
        {
            var messageTopology = new AmazonSqsMessageSendTopology<T>();

            OnMessageTopologyCreated(messageTopology);

            return messageTopology;
        }
    }
}
