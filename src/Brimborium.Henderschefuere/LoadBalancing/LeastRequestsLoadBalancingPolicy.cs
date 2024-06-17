// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Brimborium.Henderschefuere.LoadBalancing;

internal sealed class LeastRequestsLoadBalancingPolicy : ILoadBalancingPolicy {
    public string Name => LoadBalancingPolicies.LeastRequests;

    public DestinationState? PickDestination(HttpContext context, ClusterState cluster, IReadOnlyList<DestinationState> availableDestinations) {
        if (availableDestinations.Count == 0) {
            return null;
        }

        var destinationCount = availableDestinations.Count;
        var leastRequestsDestination = availableDestinations[0];
        var leastRequestsCount = leastRequestsDestination.ConcurrentRequestCount;
        for (var i = 1; i < destinationCount; i++) {
            var destination = availableDestinations[i];
            var endpointRequestCount = destination.ConcurrentRequestCount;
            if (endpointRequestCount < leastRequestsCount) {
                leastRequestsDestination = destination;
                leastRequestsCount = endpointRequestCount;
            }
        }
        return leastRequestsDestination;
    }
}
