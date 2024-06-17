// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Brimborium.Henderschefuere.SessionAffinity;

/// <summary>
/// Affinity resolution result.
/// </summary>
public readonly struct AffinityResult {
    public IReadOnlyList<DestinationState>? Destinations { get; }

    public AffinityStatus Status { get; }

    public AffinityResult(IReadOnlyList<DestinationState>? destinations, AffinityStatus status) {
        Destinations = destinations;
        Status = status;
    }
}
