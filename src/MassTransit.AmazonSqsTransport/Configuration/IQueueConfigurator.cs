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
namespace MassTransit.AmazonSqsTransport.Configuration
{
    using System.Collections.Generic;


    /// <summary>
    /// Configures a queue/exchange pair in AmazonSQS
    /// </summary>
    public interface IQueueConfigurator
    {
        /// <summary>
        /// Specify the queue should be durable (survives broker restart) or in-memory
        /// </summary>
        /// <value>True for a durable queue, False for an in-memory queue</value>
        bool Durable { set; }

        /// <summary>
        /// Specify that the queue (and the exchange of the same name) should be created as auto-delete
        /// </summary>
        bool AutoDelete { set; }

        /// <summary>
        /// Specify optional <see href="https://docs.aws.amazon.com/AWSSimpleQueueService/latest/APIReference/API_SetQueueAttributes.html">attributes</see> for the queue.  
        /// </summary>
        IDictionary<string, object> Attributes { get; }
    }
}