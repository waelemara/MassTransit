﻿// Copyright 2007-2017 Chris Patterson, Dru Sellers, Travis Smith, et. al.
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
namespace MassTransit.AutomatonymousIntegration.Tests
{
    namespace Request_Specs
    {
        using System;
        using System.Threading.Tasks;
        using Automatonymous;
        using NUnit.Framework;
        using Saga;
        using Testing;


        [TestFixture]
        public class Sending_a_request_from_a_state_machine :
            StateMachineTestFixture
        {
            static Sending_a_request_from_a_state_machine()
            {
                _serviceAddress = new Uri("loopback://localhost/service_queue");
                EndpointConvention.Map<ValidateName>(_serviceAddress);
            }

            [Test]
            public async Task Should_handle_the_response()
            {
                Task<ConsumeContext<MemberRegistered>> handler = ConnectPublishHandler<MemberRegistered>();

                var memberNumber = Guid.NewGuid().ToString();

                await InputQueueSendEndpoint.Send<RegisterMember>(new
                {
                    MemberNumber = memberNumber,
                    Name = "Frank",
                    Address = "123 American Way"
                });

                ConsumeContext<MemberRegistered> registered = await handler;

                Guid? saga = await _repository.ShouldContainSaga(x => x.MemberNumber == memberNumber
                    && GetCurrentState(x) == _machine.Registered, TestTimeout);

                Assert.IsTrue(saga.HasValue);

                var sagaInstance = _repository[saga.Value].Instance;
                Assert.IsFalse(sagaInstance.ValidateAddressRequestId.HasValue);
            }

            InMemorySagaRepository<TestState> _repository;
            TestStateMachine _machine;

            State GetCurrentState(TestState state)
            {
                return _machine.GetState(state).Result;
            }

            public Sending_a_request_from_a_state_machine()
            {
                _serviceQueueAddress = _serviceAddress;
            }

            Uri _serviceQueueAddress;
            static readonly Uri _serviceAddress;

            Uri ServiceQueueAddress
            {
                get { return _serviceQueueAddress; }
                set
                {
                    if (Bus != null)
                        throw new InvalidOperationException("The LocalBus has already been created, too late to change the URI");

                    _serviceQueueAddress = value;
                }
            }

            protected override void ConfigureInMemoryBus(IInMemoryBusFactoryConfigurator configurator)
            {
                base.ConfigureInMemoryBus(configurator);

                configurator.ReceiveEndpoint("service_queue", ConfigureServiceQueueEndpoint);
            }

            protected override void ConfigureInMemoryReceiveEndpoint(IInMemoryReceiveEndpointConfigurator configurator)
            {
                base.ConfigureInMemoryReceiveEndpoint(configurator);

                _repository = new InMemorySagaRepository<TestState>();

                var settings = new RequestSettingsImpl(ServiceQueueAddress, QuartzQueueAddress, TestTimeout);

                _machine = new TestStateMachine(settings);

                configurator.StateMachineSaga(_machine, _repository);
            }

            protected virtual void ConfigureServiceQueueEndpoint(IReceiveEndpointConfigurator configurator)
            {
                configurator.Handler<ValidateAddress>(async context =>
                {
                    Console.WriteLine("Address validated: {0}", context.Message.CorrelationId);

                    await context.RespondAsync<AddressValidated>(new{});
                });

                configurator.Handler<ValidateName>(async context =>
                {
                    Console.WriteLine("Name validated: {0}", context.Message.CorrelationId);

                    await context.RespondAsync<NameValidated>(new
                    {
                        RequestName = context.Message.Name,
                    });
                });
            }
        }


        class RequestSettingsImpl :
            RequestSettings
        {
            readonly Uri _schedulingServiceAddress;
            readonly Uri _serviceAddress;
            readonly TimeSpan _timeout;

            public RequestSettingsImpl(Uri serviceAddress, Uri schedulingServiceAddress, TimeSpan timeout)
            {
                _serviceAddress = serviceAddress;
                _schedulingServiceAddress = schedulingServiceAddress;
                _timeout = timeout;
            }

            public Uri ServiceAddress
            {
                get { return _serviceAddress; }
            }

            public Uri SchedulingServiceAddress
            {
                get { return _schedulingServiceAddress; }
            }

            public TimeSpan Timeout
            {
                get { return _timeout; }
            }
        }


        class TestState :
            SagaStateMachineInstance
        {
            public State CurrentState { get; set; }

            public string MemberNumber { get; set; }
            public string Name { get; set; }
            public string Address { get; set; }

            public Guid? ValidateAddressRequestId { get; set; }
            public Guid? ValidateNameRequestId { get; set; }

            public Guid CorrelationId { get; set; }
        }


        public interface RegisterMember
        {
            string MemberNumber { get; }
            string Name { get; }
            string Address { get; }
        }


        public interface MemberRegistered
        {
            string Name { get; }
            string Address { get; }
        }


        public interface ValidateAddress :
            CorrelatedBy<Guid>
        {
            string Address { get; }
        }


        public interface AddressValidated :
            CorrelatedBy<Guid>
        {
            string Address { get; }

            string RequestAddress { get; }
        }


        public interface ValidateName :
            CorrelatedBy<Guid>
        {
            string Name { get; }
        }


        public interface NameValidated :
            CorrelatedBy<Guid>
        {
            string Name { get; }

            string RequestName { get; }
        }


        class TestStateMachine :
            MassTransitStateMachine<TestState>
        {
            public TestStateMachine(RequestSettings settings)
            {
                Event(() => Register, x =>
                {
                    x.CorrelateBy(p => p.MemberNumber, p => p.Message.MemberNumber);
                    x.SelectId(context => NewId.NextGuid());
                });

                Request(() => ValidateAddress, x => x.ValidateAddressRequestId, settings);
                Request(() => ValidateName, x => x.ValidateNameRequestId, cfg =>
                {
                    cfg.SchedulingServiceAddress = settings.SchedulingServiceAddress;
                    cfg.Timeout = settings.Timeout;
                });

                Initially(When(Register)
                    .Then(context =>
                    {
                        Console.WriteLine("Registration received: {0}", context.Data.MemberNumber);

                        Console.WriteLine("TestState ID: {0}", context.Instance.CorrelationId);

                        context.Instance.Name = context.Data.Name;
                        context.Instance.Address = context.Data.Address;
                        context.Instance.MemberNumber = context.Data.MemberNumber;
                    })
                    .Request(ValidateAddress, x => ValidateAddress.Settings.ServiceAddress, x => x.Init<ValidateAddress>(x.Instance))
                    .TransitionTo(ValidateAddress.Pending));

                During(ValidateAddress.Pending,
                    When(ValidateAddress.Completed)
                        .ThenAsync(async context =>
                        {
                            await Console.Out.WriteLineAsync("Request Completed!");

                            context.Instance.Address = context.Data.Address;
                        })
                        .Request(ValidateName, context => context.Init<ValidateName>(context.Instance))
                        .TransitionTo(ValidateName.Pending),
                    When(ValidateAddress.Faulted)
                        .ThenAsync(async context => await Console.Out.WriteLineAsync("Request Faulted"))
                        .TransitionTo(AddressValidationFaulted),
                    When(ValidateAddress.TimeoutExpired)
                        .ThenAsync(async context => await Console.Out.WriteLineAsync("Request timed out"))
                        .TransitionTo(AddressValidationTimeout));

                During(ValidateName.Pending,
                    When(ValidateName.Completed)
                        .ThenAsync(async context =>
                        {
                            await Console.Out.WriteLineAsync("Request Completed!");

                            context.Instance.Name = context.Data.Name;
                        })
                        .PublishAsync(context => context.Init<MemberRegistered>(context.Instance))
                        .TransitionTo(Registered),
                    When(ValidateName.Faulted)
                        .ThenAsync(async context => await Console.Out.WriteLineAsync("Request Faulted"))
                        .TransitionTo(NameValidationFaulted),
                    When(ValidateName.TimeoutExpired)
                        .ThenAsync(async context => await Console.Out.WriteLineAsync("Request timed out"))
                        .TransitionTo(NameValidationTimeout));
            }

            public Request<TestState, ValidateAddress, AddressValidated> ValidateAddress { get; private set; }
            public Request<TestState, ValidateName, NameValidated> ValidateName { get; private set; }

            public Event<RegisterMember> Register { get; private set; }

            public State Registered { get; private set; }
            public State AddressValidationFaulted { get; private set; }
            public State AddressValidationTimeout { get; private set; }

            public State NameValidationFaulted { get; private set; }
            public State NameValidationTimeout { get; private set; }
        }
    }
}
