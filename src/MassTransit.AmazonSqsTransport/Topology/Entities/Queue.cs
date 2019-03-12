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
namespace MassTransit.AmazonSqsTransport.Topology.Entities
{
    using System.Collections.Generic;


    /// <summary>
    /// The queue details used to declare the queue to AmazonSQS
    /// </summary>
    public interface Queue
    {
        /// <summary>
        /// The queue name
        /// </summary>
        string EntityName { get; }

        /// <summary>
        /// True if the queue should be durable, and survive a broker restart
        /// </summary>
        bool Durable { get; }

        /// <summary>
        /// True if the queue should be deleted when the connection is closed
        /// </summary>
        bool AutoDelete { get; }

        /// <summary>
        /// Additional <see href="https://docs.aws.amazon.com/AWSSimpleQueueService/latest/APIReference/API_SetQueueAttributes.html">attributes</see> for the queue.
        /// </summary>
        IDictionary<string, object> Attributes { get; }
    }
}