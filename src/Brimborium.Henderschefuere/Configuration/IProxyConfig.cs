// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Brimborium.Henderschefuere.Configuration;

/// <summary>
/// Represents a snapshot of proxy configuration data. These properties may be accessed multiple times and should not be modified.
/// </summary>
public interface IProxyConfig {
    private static readonly ConditionalWeakTable<IProxyConfig, string> _revisionIdsTable = new();

    /// <summary>
    /// A unique identifier for this revision of the configuration.
    /// </summary>
    string RevisionId => _revisionIdsTable.GetValue(this, static _ => Guid.NewGuid().ToString());

    /// <summary>
    /// Tunnels that can be used to connect to remote servers.
    /// </summary>
    IReadOnlyList<TunnelConfig> Tunnels { get; }

    /// <summary>
    /// Routes matching requests to clusters.
    /// </summary>
    IReadOnlyList<RouteConfig> Routes { get; }

    /// <summary>
    /// Cluster information for where to proxy requests to.
    /// </summary>
    IReadOnlyList<ClusterConfig> Clusters { get; }

    /// <summary>
    /// A notification that triggers when this snapshot expires.
    /// </summary>
    IChangeToken ChangeToken { get; }
}
