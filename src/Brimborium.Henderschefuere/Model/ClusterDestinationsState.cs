// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Brimborium.Henderschefuere.Model;

public sealed class ClusterDestinationsState
{
    public ClusterDestinationsState(
        IReadOnlyList<DestinationState> allDestinations,
        IReadOnlyList<DestinationState> availableDestinations)
    {
        AllDestinations = allDestinations ?? throw new ArgumentNullException(nameof(allDestinations));
        AvailableDestinations = availableDestinations ?? throw new ArgumentNullException(nameof(availableDestinations));
    }

    public IReadOnlyList<DestinationState> AllDestinations { get; }

    public IReadOnlyList<DestinationState> AvailableDestinations { get; }
}
