// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Globalization;
using System.Runtime.CompilerServices;

namespace Brimborium.Henderschefuere.Health;

internal sealed class ConsecutiveFailuresHealthPolicy : IActiveHealthCheckPolicy {
    private readonly ConsecutiveFailuresHealthPolicyOptions _options;
    private readonly ConditionalWeakTable<ClusterState, ParsedMetadataEntry<double>> _clusterThresholds = new ConditionalWeakTable<ClusterState, ParsedMetadataEntry<double>>();
    private readonly ConditionalWeakTable<DestinationState, AtomicCounter> _failureCounters = new ConditionalWeakTable<DestinationState, AtomicCounter>();
    private readonly IDestinationHealthUpdater _healthUpdater;

    public string Name => HealthCheckConstants.ActivePolicy.ConsecutiveFailures;

    public ConsecutiveFailuresHealthPolicy(IOptions<ConsecutiveFailuresHealthPolicyOptions> options, IDestinationHealthUpdater healthUpdater) {
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _healthUpdater = healthUpdater ?? throw new ArgumentNullException(nameof(healthUpdater));
    }

    public void ProbingCompleted(ClusterState cluster, IReadOnlyList<DestinationProbingResult> probingResults) {
        if (probingResults.Count == 0) {
            return;
        }

        var threshold = GetFailureThreshold(cluster);

        var newHealthStates = new NewActiveDestinationHealth[probingResults.Count];
        for (var i = 0; i < probingResults.Count; i++) {
            var destination = probingResults[i].Destination;
            var previousState = destination.Health.Active;

            var count = _failureCounters.GetOrCreateValue(destination);
            var newHealth = EvaluateHealthState(threshold, probingResults[i].Response, count, previousState);
            newHealthStates[i] = new NewActiveDestinationHealth(destination, newHealth);
        }

        _healthUpdater.SetActive(cluster, newHealthStates);
    }

    private double GetFailureThreshold(ClusterState cluster) {
        var thresholdEntry = _clusterThresholds.GetValue(cluster, c => new ParsedMetadataEntry<double>(TryParse, c, ConsecutiveFailuresHealthPolicyOptions.ThresholdMetadataName));
        return thresholdEntry.GetParsedOrDefault(_options.DefaultThreshold);
    }

    private static DestinationHealth EvaluateHealthState(double threshold, HttpResponseMessage? response, AtomicCounter count, DestinationHealth previousState) {
        DestinationHealth newHealth;
        if (response is not null && response.IsSuccessStatusCode) {
            // Success
            count.Reset();
            newHealth = DestinationHealth.Healthy;
        } else {
            // Failure
            var currentFailureCount = count.Increment();
            newHealth = currentFailureCount < threshold ? previousState : DestinationHealth.Unhealthy;
        }

        return newHealth;
    }

    private static bool TryParse(string stringValue, out double parsedValue) {
        return double.TryParse(stringValue, NumberStyles.Float, CultureInfo.InvariantCulture, out parsedValue);
    }
}
