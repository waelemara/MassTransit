namespace MassTransit.AspNetCoreIntegration.HealthChecks
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Diagnostics.HealthChecks;
    using Util;


    public class ReceiveEndpointHealthCheck :
        IReceiveEndpointObserver,
        IHealthCheck
    {
        readonly ConcurrentDictionary<Uri, EndpointStatus> _endpoints;

        public ReceiveEndpointHealthCheck()
        {
            _endpoints = new ConcurrentDictionary<Uri, EndpointStatus>();
        }

        Task<HealthCheckResult> IHealthCheck.CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken)
        {
            if (_endpoints.All(x => x.Value.Ready))
                return Task.FromResult(HealthCheckResult.Healthy("All endpoints ready",
                    new Dictionary<string, object> {{"Endpoints", _endpoints.Keys.ToArray()}}));

            var faulted = _endpoints.Where(x => !x.Value.Ready).ToArray();

            var unhealthyMessage = string.Join(",", faulted.Select(x => x.Key));

            return Task.FromResult(HealthCheckResult.Unhealthy($"Failed endpoints: {unhealthyMessage}",
                faulted.Select(x => x.Value.LastException).FirstOrDefault(e => e != null),
                new Dictionary<string, object> {{"Endpoints", faulted.Select(x => x.Key).ToArray()}}));
        }

        public Task Ready(ReceiveEndpointReady ready)
        {
            GetEndpoint(ready.InputAddress).Ready = true;

            return TaskUtil.Completed;
        }

        public Task Completed(ReceiveEndpointCompleted completed)
        {
            return TaskUtil.Completed;
        }

        public Task Faulted(ReceiveEndpointFaulted faulted)
        {
            var endpoint = GetEndpoint(faulted.InputAddress);

            endpoint.Ready = false;
            endpoint.LastException = faulted.Exception;

            return TaskUtil.Completed;
        }

        EndpointStatus GetEndpoint(Uri inputAddress)
        {
            if (!_endpoints.ContainsKey(inputAddress))
                _endpoints.TryAdd(inputAddress, new EndpointStatus());

            return _endpoints[inputAddress];
        }


        class EndpointStatus
        {
            public bool Ready { get; set; }
            public Exception LastException { get; set; }
        }
    }
}
