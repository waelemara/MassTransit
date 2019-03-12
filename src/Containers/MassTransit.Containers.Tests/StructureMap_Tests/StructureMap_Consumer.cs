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
namespace MassTransit.Containers.Tests.StructureMap_Tests
{
    using Common_Tests;
    using NUnit.Framework;
    using Scenarios;
    using StructureMap;


    [TestFixture]
    public class StructureMap_Consumer :
        Common_Consumer
    {
        readonly IContainer _container;

        public StructureMap_Consumer()
        {
            _container = new Container(expression =>
            {
                expression.AddMassTransit(cfg =>
                {
                    cfg.AddConsumer<SimpleConsumer>();
                    cfg.AddBus(context => BusControl);
                });

                expression.For<ISimpleConsumerDependency>()
                    .Use<SimpleConsumerDependency>();

                expression.For<AnotherMessageConsumer>()
                    .Use<AnotherMessageConsumerImpl>();
            });
        }

        protected override void ConfigureConsumer(IInMemoryReceiveEndpointConfigurator configurator)
        {
            configurator.ConfigureConsumer<SimpleConsumer>(_container);
        }
    }


    [TestFixture]
    public class StructureMap_Consumer_Endpoint :
        Common_Consumer_Endpoint
    {
        readonly IContainer _container;

        public StructureMap_Consumer_Endpoint()
        {
            _container = new Container(expression =>
            {
                expression.AddMassTransit(cfg =>
                {
                    cfg.AddConsumer<SimplerConsumer>()
                        .Endpoint(e => e.Name = "custom-endpoint-name");

                    cfg.AddBus(context => BusControl);
                });

                expression.For<ISimpleConsumerDependency>()
                    .Use<SimpleConsumerDependency>();

                expression.For<AnotherMessageConsumer>()
                    .Use<AnotherMessageConsumerImpl>();
            });
        }

        protected override void ConfigureEndpoints(IInMemoryBusFactoryConfigurator configurator)
        {
            configurator.ConfigureEndpoints(_container);
        }
    }
}
