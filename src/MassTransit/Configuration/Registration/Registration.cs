// Copyright 2007-2019 Chris Patterson, Dru Sellers, Travis Smith, et. al.
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
namespace MassTransit.Registration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using ConsumeConfigurators;
    using Definition;
    using Logging;
    using Util;


    public class Registration :
        IRegistration
    {
        static readonly ILog _log = Logger.Get<Registration>();

        readonly IConfigurationServiceProvider _configurationServiceProvider;
        readonly IDictionary<Type, IConsumerRegistration> _consumers;
        readonly IDictionary<Type, ISagaRegistration> _sagas;
        readonly IDictionary<Type, IExecuteActivityRegistration> _executeActivities;
        readonly IDictionary<Type, IActivityRegistration> _activities;
        readonly IDictionary<Type, IEndpointRegistration> _endpoints;

        public Registration(IConfigurationServiceProvider configurationServiceProvider, IDictionary<Type, IConsumerRegistration> consumers,
            IDictionary<Type, ISagaRegistration> sagas, IDictionary<Type, IExecuteActivityRegistration> executeActivities,
            IDictionary<Type, IActivityRegistration> activities, IDictionary<Type, IEndpointRegistration> endpoints)
        {
            _configurationServiceProvider = configurationServiceProvider;
            _consumers = consumers;
            _sagas = sagas;
            _executeActivities = executeActivities;
            _activities = activities;
            _endpoints = endpoints;
        }

        public void ConfigureConsumer(Type consumerType, IReceiveEndpointConfigurator configurator)
        {
            if (!_consumers.TryGetValue(consumerType, out var consumer))
                throw new ArgumentException($"The consumer type was not found: {TypeMetadataCache.GetShortName(consumerType)}", nameof(consumerType));

            consumer.Configure(configurator, _configurationServiceProvider);
        }

        void IRegistration.ConfigureConsumer<T>(IReceiveEndpointConfigurator configurator, Action<IConsumerConfigurator<T>> configure)
        {
            if (!_consumers.TryGetValue(typeof(T), out var consumer))
                throw new ArgumentException($"The consumer type was not found: {TypeMetadataCache.GetShortName(typeof(T))}", nameof(T));

            consumer.AddConfigureAction(configure);
            consumer.Configure(configurator, _configurationServiceProvider);
        }

        public void ConfigureConsumers(IReceiveEndpointConfigurator configurator)
        {
            foreach (var consumer in _consumers.Values)
                consumer.Configure(configurator, _configurationServiceProvider);
        }

        public void ConfigureSaga(Type sagaType, IReceiveEndpointConfigurator configurator)
        {
            if (!_sagas.TryGetValue(sagaType, out var saga))
                throw new ArgumentException($"The saga type was not found: {TypeMetadataCache.GetShortName(sagaType)}", nameof(sagaType));

            saga.Configure(configurator, _configurationServiceProvider);
        }

        void IRegistration.ConfigureSaga<T>(IReceiveEndpointConfigurator configurator, Action<ISagaConfigurator<T>> configure)
        {
            if (!_sagas.TryGetValue(typeof(T), out var saga))
                throw new ArgumentException($"The saga type was not found: {TypeMetadataCache.GetShortName(typeof(T))}", nameof(T));

            saga.AddConfigureAction(configure);
            saga.Configure(configurator, _configurationServiceProvider);
        }

        void IRegistration.ConfigureSagas(IReceiveEndpointConfigurator configurator)
        {
            foreach (var saga in _sagas.Values)
                saga.Configure(configurator, _configurationServiceProvider);
        }

        public void ConfigureExecuteActivity(Type activityType, IReceiveEndpointConfigurator configurator)
        {
            if (!_executeActivities.TryGetValue(activityType, out var activity))
                throw new ArgumentException($"The activity type was not found: {TypeMetadataCache.GetShortName(activityType)}", nameof(activityType));

            activity.Configure(configurator, _configurationServiceProvider);
        }

        public void ConfigureActivity(Type activityType, IReceiveEndpointConfigurator executeEndpointConfigurator,
            IReceiveEndpointConfigurator compensateEndpointConfigurator)
        {
            if (!_activities.TryGetValue(activityType, out var activity))
                throw new ArgumentException($"The activity type was not found: {TypeMetadataCache.GetShortName(activityType)}", nameof(activityType));

            activity.Configure(executeEndpointConfigurator, compensateEndpointConfigurator, _configurationServiceProvider);
        }

        public void ConfigureEndpoints<T>(T configurator, IEndpointNameFormatter endpointNameFormatter)
            where T : IBusFactoryConfigurator
        {
            if (configurator == null)
                throw new ArgumentNullException(nameof(configurator));

            if (endpointNameFormatter == null)
            {
                endpointNameFormatter = _configurationServiceProvider.GetService<IEndpointNameFormatter>()
                    ?? DefaultEndpointNameFormatter.Instance;
            }

            var consumersByEndpoint = _consumers.Values
                .Select(x => x.GetDefinition(_configurationServiceProvider))
                .GroupBy(x => x.GetEndpointName(endpointNameFormatter));

            var sagasByEndpoint = _sagas.Values
                .Select(x => x.GetDefinition(_configurationServiceProvider))
                .GroupBy(x => x.GetEndpointName(endpointNameFormatter));

            var activitiesByExecuteEndpoint = _activities.Values
                .Select(x => x.GetDefinition(_configurationServiceProvider))
                .GroupBy(x => x.GetExecuteEndpointName(endpointNameFormatter));

            var executeActivitiesByEndpoint = _executeActivities.Values
                .Select(x => x.GetDefinition(_configurationServiceProvider))
                .GroupBy(x => x.GetExecuteEndpointName(endpointNameFormatter));

            var endpointsWithName = _endpoints.Values
                .Select(x => x.GetDefinition(_configurationServiceProvider))
                .Select(x => new
                {
                    Definition = x,
                    Name = x.GetEndpointName(endpointNameFormatter)
                });

            var endpointNames = consumersByEndpoint.Select(x => x.Key)
                .Union(sagasByEndpoint.Select(x => x.Key))
                .Union(activitiesByExecuteEndpoint.Select(x => x.Key))
                .Union(executeActivitiesByEndpoint.Select(x => x.Key))
                .Union(endpointsWithName.Select(x => x.Name));

            var endpoints =
                from e in endpointNames
                join c in consumersByEndpoint on e equals c.Key into cs
                from c in cs.DefaultIfEmpty()
                join s in sagasByEndpoint on e equals s.Key into ss
                from s in ss.DefaultIfEmpty()
                join a in activitiesByExecuteEndpoint on e equals a.Key into aes
                from a in aes.DefaultIfEmpty()
                join ea in executeActivitiesByEndpoint on e equals ea.Key into eas
                from ea in eas.DefaultIfEmpty()
                join ep in endpointsWithName on e equals ep.Name into eps
                from ep in eps.Select(x => x.Definition).DefaultIfEmpty(new NamedEndpointDefinition(e))
                select new {Name = e, Definition = ep, Consumers = c, Sagas = s, Activities = a, ExecuteActivities = ea};

            foreach (var endpoint in endpoints)
            {
                if (_log.IsDebugEnabled)
                    _log.DebugFormat("Configuring receive endpoint ({0})",
                        string.Join(", ", new[]
                        {
                            $"name: {endpoint.Name}",
                            endpoint.Definition.IsTemporary ? "temporary" : "",
                            endpoint.Definition.ConcurrentMessageLimit.HasValue
                                ? $"concurrent-message-limit: {endpoint.Definition.ConcurrentMessageLimit}"
                                : "",
                            endpoint.Definition.PrefetchCount.HasValue
                                ? $"prefect-count: {endpoint.Definition.PrefetchCount}"
                                : "",
                        }.Where(x => !string.IsNullOrWhiteSpace(x))));

                configurator.ReceiveEndpoint(endpoint.Definition, endpointNameFormatter, cfg =>
                {
                    if (endpoint.Consumers != null)
                        foreach (var consumer in endpoint.Consumers)
                        {
                            if (_log.IsDebugEnabled)
                                _log.DebugFormat("Configuring consumer {0} on {1}", TypeMetadataCache.GetShortName(consumer.ConsumerType), endpoint.Name);

                            ConfigureConsumer(consumer.ConsumerType, cfg);
                        }

                    if (endpoint.Sagas != null)
                        foreach (var saga in endpoint.Sagas)
                        {
                            if (_log.IsDebugEnabled)
                                _log.DebugFormat("Configuring saga {0} on {1}", TypeMetadataCache.GetShortName(saga.SagaType), endpoint.Name);

                            ConfigureSaga(saga.SagaType, cfg);
                        }

                    if (endpoint.Activities != null)
                        foreach (var activity in endpoint.Activities)
                        {
                            var compensateEndpointName = activity.GetCompensateEndpointName(endpointNameFormatter);

                            configurator.ReceiveEndpoint(compensateEndpointName, compensateEndpointConfigurator =>
                            {
                                if (_log.IsDebugEnabled)
                                    _log.DebugFormat("Configuring activity {0} on {1}/{2}", TypeMetadataCache.GetShortName(activity.ActivityType),
                                        endpoint.Name, compensateEndpointName);

                                ConfigureActivity(activity.ActivityType, cfg, compensateEndpointConfigurator);
                            });
                        }

                    if (endpoint.ExecuteActivities != null)
                        foreach (var activity in endpoint.ExecuteActivities)
                        {
                            if (_log.IsDebugEnabled)
                                _log.DebugFormat("Configuring activity {0} on {1}", TypeMetadataCache.GetShortName(activity.ActivityType), endpoint.Name);

                            ConfigureExecuteActivity(activity.ActivityType, cfg);
                        }
                });
            }
        }
    }
}
