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
namespace MassTransit.ExtensionsDependencyInjectionIntegration.Configuration.Registration
{
    using System;
    using MassTransit.Registration;
    using Microsoft.Extensions.DependencyInjection;
    using ScopeProviders;
    using Scoping;


    public class ServiceCollectionConfigurator :
        RegistrationConfigurator,
        IServiceCollectionConfigurator
    {
        public ServiceCollectionConfigurator(IServiceCollection collection)
            : base(new DependencyInjectionContainerRegistrar(collection))
        {
            Collection = collection;

            AddMassTransitComponents(collection);

            collection.AddSingleton<IRegistrationConfigurator>(this);
            collection.AddSingleton(provider => CreateRegistration(provider.GetRequiredService<IConfigurationServiceProvider>()));
        }

        public IServiceCollection Collection { get; }

        public void AddBus(Func<IServiceProvider, IBusControl> busFactory)
        {
            Collection.AddSingleton(busFactory);
            Collection.AddSingleton<IBus>(provider => provider.GetRequiredService<IBusControl>());
            Collection.AddSingleton<ISendEndpointProvider>(provider => provider.GetRequiredService<IBusControl>());
            Collection.AddSingleton<IPublishEndpoint>(provider => provider.GetRequiredService<IBusControl>());

            Collection.AddSingleton(context => context.GetRequiredService<IBus>().CreateClientFactory());
        }

        public void AddRequestClient<T>(RequestTimeout timeout = default)
            where T : class
        {
            Collection.AddScoped(context =>
            {
                var clientFactory = context.GetRequiredService<IClientFactory>();

                var consumeContext = context.GetService<ConsumeContext>();
                return (consumeContext != null)
                    ? clientFactory.CreateRequestClient<T>(consumeContext, timeout)
                    : clientFactory.CreateRequestClient<T>(timeout);
            });
        }

        public void AddRequestClient<T>(Uri destinationAddress, RequestTimeout timeout = default)
            where T : class
        {
            Collection.AddScoped(context =>
            {
                var clientFactory = context.GetRequiredService<IClientFactory>();

                var consumeContext = context.GetService<ConsumeContext>();
                return (consumeContext != null)
                    ? clientFactory.CreateRequestClient<T>(consumeContext, destinationAddress, timeout)
                    : clientFactory.CreateRequestClient<T>(destinationAddress, timeout);
            });
        }

        static void AddMassTransitComponents(IServiceCollection collection)
        {
            collection.AddScoped<ScopedConsumeContextProvider>();
            collection.AddScoped(provider => provider.GetRequiredService<ScopedConsumeContextProvider>().GetContext());

            collection.AddScoped(provider => (ISendEndpointProvider)provider.GetService<ScopedConsumeContextProvider>()?.GetContext() ??
                provider.GetRequiredService<IBus>());

            collection.AddScoped(provider => (IPublishEndpoint)provider.GetService<ScopedConsumeContextProvider>()?.GetContext() ??
                provider.GetRequiredService<IBus>());

            collection.AddSingleton<IConsumerScopeProvider>(provider => new DependencyInjectionConsumerScopeProvider(provider));
            collection.AddSingleton<ISagaRepositoryFactory>(provider => new DependencyInjectionSagaRepositoryFactory(provider));
            collection.AddSingleton<IConfigurationServiceProvider>(provider => new DependencyInjectionConfigurationServiceProvider(provider));
        }
    }
}
