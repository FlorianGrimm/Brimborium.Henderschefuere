// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Brimborium.Henderschefuere.Health;

/// <summary>
/// Active health check evaluation policy.
/// </summary>
public interface IActiveHealthCheckPolicy {
    /// <summary>
    /// Policy's name.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Analyzes results of active health probes sent to destinations and calculates their new health states.
    /// </summary>
    /// <param name="cluster">Cluster.</param>
    /// <param name="probingResults">Destination probing results.</param>
    void ProbingCompleted(ClusterState cluster, IReadOnlyList<DestinationProbingResult> probingResults);
}
