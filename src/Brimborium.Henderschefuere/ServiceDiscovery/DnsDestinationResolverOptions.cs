// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net.Sockets;

namespace Brimborium.Henderschefuere.ServiceDiscovery;

/// <summary>
/// Options for <see cref="DnsDestinationResolver"/>.
/// </summary>
public sealed class DnsDestinationResolverOptions {
    /// <summary>
    /// The period between requesting a refresh of a resolved name.
    /// </summary>
    /// <remarks>
    /// Defaults to 5 minutes.
    /// </remarks>
    public TimeSpan? RefreshPeriod { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// The optional address family to query for.
    /// Use <see cref="AddressFamily.InterNetwork"/> for IPv4 addresses and <see cref="AddressFamily.InterNetworkV6"/> for IPv6 addresses.
    /// </summary>
    /// <remarks>
    /// Defaults to <see langword="null"/> (any address).
    /// </remarks>
    public AddressFamily? AddressFamily { get; set; }
}
