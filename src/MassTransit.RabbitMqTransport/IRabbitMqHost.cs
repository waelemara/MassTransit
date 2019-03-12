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
namespace MassTransit.RabbitMqTransport
{
    using System;
    using GreenPipes;
    using Integration;
    using Topology;


    public interface IRabbitMqHost :
        IHost
    {
        IConnectionContextSupervisor ConnectionContextSupervisor { get; }

        IRetryPolicy ConnectionRetryPolicy { get; }

        RabbitMqHostSettings Settings { get; }

        new IRabbitMqHostTopology Topology { get; }

        /// <summary>
        /// Connect a receive endpoint for the host, using an endpoint definition
        /// </summary>
        /// <param name="definition">An endpoint definition, which abstracts specific endpoint behaviors from the transport</param>
        /// <param name="endpointNameFormatter"></param>
        /// <param name="configureEndpoint">The configuration callback</param>
        HostReceiveEndpointHandle ConnectReceiveEndpoint(IEndpointDefinition definition, IEndpointNameFormatter endpointNameFormatter = null,
            Action<IRabbitMqReceiveEndpointConfigurator> configureEndpoint = null);

        /// <summary>
        /// Connect a receive endpoint to the host, with the specified queue name
        /// </summary>
        /// <param name="queueName"></param>
        /// <param name="configure"></param>
        /// <returns></returns>
        HostReceiveEndpointHandle ConnectReceiveEndpoint(string queueName, Action<IRabbitMqReceiveEndpointConfigurator> configure = null);
    }
}
