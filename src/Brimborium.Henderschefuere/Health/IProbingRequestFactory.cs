// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Brimborium.Henderschefuere.Health;

/// <summary>
/// A factory for creating <see cref="HttpRequestMessage"/>s for active health probes to be sent to destinations.
/// </summary>
public interface IProbingRequestFactory {
    /// <summary>
    /// Creates a probing request.
    /// </summary>
    /// <param name="cluster">The cluster being probed.</param>
    /// <param name="destination">The destination being probed.</param>
    /// <returns>Probing <see cref="HttpRequestMessage"/>.</returns>
    HttpRequestMessage CreateRequest(ClusterModel cluster, DestinationModel destination);
}
