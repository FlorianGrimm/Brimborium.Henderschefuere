// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Brimborium.Henderschefuere.Configuration;

internal sealed class ConfigValidator : IConfigValidator {
    private readonly ITransformBuilder _transformBuilder;
    private readonly ITunnelValidator[] _tunnelValidators;
    private readonly IRouteValidator[] _routeValidators;
    private readonly IClusterValidator[] _clusterValidators;

    public ConfigValidator(ITransformBuilder transformBuilder,
        IEnumerable<ITunnelValidator> tunnelValidators,
        IEnumerable<IRouteValidator> routeValidators,
        IEnumerable<IClusterValidator> clusterValidators) {
        _transformBuilder = transformBuilder ?? throw new ArgumentNullException(nameof(transformBuilder));
        _tunnelValidators = tunnelValidators?.ToArray() ?? throw new ArgumentNullException(nameof(tunnelValidators));
        _routeValidators = routeValidators?.ToArray() ?? throw new ArgumentNullException(nameof(routeValidators));
        _clusterValidators = clusterValidators?.ToArray() ?? throw new ArgumentNullException(nameof(clusterValidators));
    }

    public async ValueTask<IList<Exception>> ValidateTunnelAsync(TunnelConfig tunnel) {
        _ = tunnel ?? throw new ArgumentNullException(nameof(tunnel));
        var errors = new List<Exception>();
        foreach (var tunnelValidator in _tunnelValidators) {
            await tunnelValidator.ValidateAsync(tunnel, errors);
        }
        return errors;
    }

    // Note this performs all validation steps without short circuiting in order to report all possible errors.
    public async ValueTask<IList<Exception>> ValidateRouteAsync(RouteConfig route) {
        _ = route ?? throw new ArgumentNullException(nameof(route));
        var errors = new List<Exception>();

        if (string.IsNullOrEmpty(route.RouteId)) {
            errors.Add(new ArgumentException("Missing Route Id."));
        }

        errors.AddRange(_transformBuilder.ValidateRoute(route));

        if (route.Match is null) {
            errors.Add(new ArgumentException($"Route '{route.RouteId}' did not set any match criteria, it requires Hosts or Path specified. Set the Path to '/{{**catchall}}' to match all requests."));
            return errors;
        }

        if ((route.Match.Hosts is null || route.Match.Hosts.All(string.IsNullOrEmpty)) && string.IsNullOrEmpty(route.Match.Path)) {
            errors.Add(new ArgumentException($"Route '{route.RouteId}' requires Hosts or Path specified. Set the Path to '/{{**catchall}}' to match all requests."));
        }

        foreach (var routeValidator in _routeValidators) {
            await routeValidator.ValidateAsync(route, errors);
        }

        return errors;
    }

    // Note this performs all validation steps without short circuiting in order to report all possible errors.
    public async ValueTask<IList<Exception>> ValidateClusterAsync(ClusterConfig cluster) {
        _ = cluster ?? throw new ArgumentNullException(nameof(cluster));
        var errors = new List<Exception>();

        if (string.IsNullOrEmpty(cluster.ClusterId)) {
            errors.Add(new ArgumentException("Missing Cluster Id."));
        }

        errors.AddRange(_transformBuilder.ValidateCluster(cluster));

        foreach (var clusterValidator in _clusterValidators) {
            await clusterValidator.ValidateAsync(cluster, errors);
        }

        return errors;
    }
}
