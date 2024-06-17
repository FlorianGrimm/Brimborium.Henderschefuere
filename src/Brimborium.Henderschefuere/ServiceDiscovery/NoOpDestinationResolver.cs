// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Brimborium.Henderschefuere.ServiceDiscovery;

/// <summary>
/// An <see cref="IDestinationResolver"/> which performs no action.
/// </summary>
internal sealed class NoOpDestinationResolver : IDestinationResolver
{
    public ValueTask<ResolvedDestinationCollection> ResolveDestinationsAsync(IReadOnlyDictionary<string, DestinationConfig> destinations, CancellationToken cancellationToken)
        => new(new ResolvedDestinationCollection(destinations, changeToken: null));
}
