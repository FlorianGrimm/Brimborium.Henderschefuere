// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Frozen;

namespace Brimborium.Henderschefuere.Health;

public class PassiveHealthCheckMiddleware {
    private readonly RequestDelegate _next;
    private readonly FrozenDictionary<string, IPassiveHealthCheckPolicy> _policies;

    public PassiveHealthCheckMiddleware(RequestDelegate next, IEnumerable<IPassiveHealthCheckPolicy> policies) {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _policies = policies?.ToDictionaryByUniqueId(p => p.Name) ?? throw new ArgumentNullException(nameof(policies));
    }

    public async Task Invoke(HttpContext context) {
        await _next(context);

        var proxyFeature = context.GetReverseProxyFeature();
        var options = proxyFeature.Cluster.Config.HealthCheck?.Passive;

        // Do nothing if no target destination has been chosen for the request.
        if (options is null || !options.Enabled.GetValueOrDefault() || proxyFeature.ProxiedDestination is null) {
            return;
        }

        var policy = _policies.GetRequiredServiceById(options.Policy, HealthCheckConstants.PassivePolicy.TransportFailureRate);
        var cluster = context.GetRouteModel().Cluster!;
        policy.RequestProxied(context, cluster, proxyFeature.ProxiedDestination);
    }
}
