// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Brimborium.Henderschefuere.Health;

internal sealed class HealthyOrPanicDestinationsPolicy : HealthyAndUnknownDestinationsPolicy
{
    public override string Name => HealthCheckConstants.AvailableDestinations.HealthyOrPanic;

    public override IReadOnlyList<DestinationState> GetAvailalableDestinations(ClusterConfig config, IReadOnlyList<DestinationState> allDestinations)
    {
        var availableDestinations = base.GetAvailalableDestinations(config, allDestinations);
        return availableDestinations.Count > 0 ? availableDestinations : allDestinations;
    }
}
