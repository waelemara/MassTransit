// Copyright 2007-2017 Chris Patterson, Dru Sellers, Travis Smith, et. al.
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
namespace MassTransit.UnityIntegration
{
    using System;
    using Registration;
    using Saga;
    using Scoping;
    using Unity;


    public class UnitySagaRepositoryFactory :
        ISagaRepositoryFactory
    {
        readonly IUnityContainer _container;

        public UnitySagaRepositoryFactory(IUnityContainer container)
        {
            _container = container;
        }

        public ISagaRepository<T> CreateSagaRepository<T>(Action<ConsumeContext> scopeAction)
            where T : class, ISaga
        {
            var repository = _container.Resolve<ISagaRepository<T>>();

            var scopeProvider = new UnitySagaScopeProvider<T>(_container);
            // if (scopeAction != null)
            //     scopeProvider.AddScopeAction(scopeAction);

            var sagaRepository = new ScopeSagaRepository<T>(repository, scopeProvider);

            return sagaRepository;
        }
    }
}
