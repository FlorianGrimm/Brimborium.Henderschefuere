// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Brimborium.Henderschefuere.Configuration.ConfigProvider;

public sealed class ConfigurationSnapshot : IProxyConfig {
    public List<TunnelConfig> Tunnels { get; internal set; } = new List<TunnelConfig>();

    public List<RouteConfig> Routes { get; internal set; } = new List<RouteConfig>();

    public List<ClusterConfig> Clusters { get; internal set; } = new List<ClusterConfig>();

    IReadOnlyList<TunnelConfig> IProxyConfig.Tunnels => Tunnels;

    IReadOnlyList<RouteConfig> IProxyConfig.Routes => Routes;

    IReadOnlyList<ClusterConfig> IProxyConfig.Clusters => Clusters;

    // This field is required.
    public IChangeToken ChangeToken { get; internal set; } = default!;
}
