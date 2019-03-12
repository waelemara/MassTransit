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
namespace MassTransit.Transports
{
    using System;
    using System.Threading.Tasks;
    using Configuration;
    using GreenPipes;
    using GreenPipes.Agents;
    using Pipeline;
    using Topology;


    public abstract class BaseHost :
        Supervisor,
        IBusHostControl
    {
        readonly IHostConfiguration _hostConfiguration;
        readonly IReceiveEndpointCollection _receiveEndpoints;
        HostHandle _handle;

        protected BaseHost(IHostConfiguration hostConfiguration)
        {
            _hostConfiguration = hostConfiguration;

            _receiveEndpoints = new ReceiveEndpointCollection();
            Add(_receiveEndpoints);
        }

        protected IReceiveEndpointCollection ReceiveEndpoints => _receiveEndpoints;

        Uri IHost.Address => _hostConfiguration.HostAddress;
        IHostTopology IHost.Topology => _hostConfiguration.Topology;

        public abstract HostReceiveEndpointHandle ConnectReceiveEndpoint(IEndpointDefinition definition, IEndpointNameFormatter endpointNameFormatter,
            Action<IReceiveEndpointConfigurator> configureEndpoint = null);

        public abstract HostReceiveEndpointHandle ConnectReceiveEndpoint(string queueName, Action<IReceiveEndpointConfigurator> configureEndpoint = null);

        ConnectHandle IConsumeMessageObserverConnector.ConnectConsumeMessageObserver<T>(IConsumeMessageObserver<T> observer)
        {
            return _receiveEndpoints.ConnectConsumeMessageObserver(observer);
        }

        ConnectHandle IConsumeObserverConnector.ConnectConsumeObserver(IConsumeObserver observer)
        {
            return _receiveEndpoints.ConnectConsumeObserver(observer);
        }

        ConnectHandle IReceiveObserverConnector.ConnectReceiveObserver(IReceiveObserver observer)
        {
            return _receiveEndpoints.ConnectReceiveObserver(observer);
        }

        ConnectHandle IReceiveEndpointObserverConnector.ConnectReceiveEndpointObserver(IReceiveEndpointObserver observer)
        {
            return _receiveEndpoints.ConnectReceiveEndpointObserver(observer);
        }

        ConnectHandle IPublishObserverConnector.ConnectPublishObserver(IPublishObserver observer)
        {
            return _receiveEndpoints.ConnectPublishObserver(observer);
        }

        ConnectHandle ISendObserverConnector.ConnectSendObserver(ISendObserver observer)
        {
            return _receiveEndpoints.ConnectSendObserver(observer);
        }

        public virtual Task<HostHandle> Start()
        {
            if (_handle != null)
                throw new MassTransitException($"The host was already started: {_hostConfiguration.HostAddress}");

            HostReceiveEndpointHandle[] handles = _receiveEndpoints.StartEndpoints();

            _handle = new StartHostHandle(this, handles, GetAgentHandles());

            return Task.FromResult(_handle);
        }

        void IBusHostControl.AddReceiveEndpoint(string endpointName, IReceiveEndpointControl receiveEndpoint)
        {
            _receiveEndpoints.Add(endpointName, receiveEndpoint);
        }

        void IProbeSite.Probe(ProbeContext context)
        {
            var scope = context.CreateScope("host");

            Probe(scope);

            _receiveEndpoints.Probe(scope);
        }

        protected override async Task StopSupervisor(StopSupervisorContext context)
        {
            await base.StopSupervisor(context).ConfigureAwait(false);

            foreach (var agent in GetAgentHandles())
            {
                await agent.Stop(context).ConfigureAwait(false);
            }
        }

        protected abstract void Probe(ProbeContext context);

        protected virtual IAgent[] GetAgentHandles()
        {
            return new IAgent[0];
        }
    }
}
