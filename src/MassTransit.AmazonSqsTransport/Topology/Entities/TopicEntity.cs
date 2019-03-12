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
    using System.Linq;


    public class TopicEntity :
        Topic,
        TopicHandle
    {
        public TopicEntity(long id, string name, bool durable, bool autoDelete, IDictionary<string, object> topicAttributes = null, IDictionary<string, object> topicSubscriptionAttributes = null)
        {
            Id = id;
            EntityName = name;
            Durable = durable;
            AutoDelete = autoDelete;
            TopicAttributes = topicAttributes ?? new Dictionary<string, object>();
            TopicSubscriptionAttributes = topicSubscriptionAttributes ?? new Dictionary<string, object>();

            EnsureRawDeliveryIsSet();
        }

        public static IEqualityComparer<TopicEntity> NameComparer { get; } = new NameEqualityComparer();

        public static IEqualityComparer<TopicEntity> EntityComparer { get; } = new TopicEntityEqualityComparer();

        public string EntityName { get; }
        public bool Durable { get; }
        public bool AutoDelete { get; }
        public long Id { get; }
        public IDictionary<string, object> TopicAttributes { get; }
        public IDictionary<string, object> TopicSubscriptionAttributes { get; }
        public Topic Topic => this;

        public override string ToString()
        {
            return string.Join(", ", new[]
            {
                $"name: {EntityName}",
                Durable ? "durable" : "",
                AutoDelete ? "auto-delete" : "",
                TopicAttributes.Any() ? $"attributes: {string.Join(";", TopicAttributes.Select(a => $"{a.Key}={a.Value}"))}" : "",
                TopicSubscriptionAttributes.Any() ? $"subscription-attributes: {string.Join(";", TopicSubscriptionAttributes.Select(a => $"{a.Key}={a.Value}"))}" : ""
            }.Where(x => !string.IsNullOrWhiteSpace(x)));
        }

        private void EnsureRawDeliveryIsSet()
        {
            TopicSubscriptionAttributes["RawMessageDelivery"] = true;
        }


        sealed class NameEqualityComparer : IEqualityComparer<TopicEntity>
        {
            public bool Equals(TopicEntity x, TopicEntity y)
            {
                if (ReferenceEquals(x, y))
                    return true;

                if (ReferenceEquals(x, null))
                    return false;

                if (ReferenceEquals(y, null))
                    return false;

                if (x.GetType() != y.GetType())
                    return false;

                return string.Equals(x.EntityName, y.EntityName);
            }

            public int GetHashCode(TopicEntity obj)
            {
                return obj.EntityName.GetHashCode();
            }
        }


        sealed class TopicEntityEqualityComparer :
            IEqualityComparer<TopicEntity>
        {
            public bool Equals(TopicEntity x, TopicEntity y)
            {
                if (ReferenceEquals(x, y))
                    return true;

                if (ReferenceEquals(x, null))
                    return false;

                if (ReferenceEquals(y, null))
                    return false;

                if (x.GetType() != y.GetType())
                    return false;

                return string.Equals(x.EntityName, y.EntityName) && x.Durable == y.Durable && x.AutoDelete == y.AutoDelete;
            }

            public int GetHashCode(TopicEntity obj)
            {
                unchecked
                {
                    var hashCode = obj.EntityName.GetHashCode();
                    hashCode = (hashCode * 397) ^ obj.Durable.GetHashCode();
                    hashCode = (hashCode * 397) ^ obj.AutoDelete.GetHashCode();
                    return hashCode;
                }
            }
        }
    }
}
