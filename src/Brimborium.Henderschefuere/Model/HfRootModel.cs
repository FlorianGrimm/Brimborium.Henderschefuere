namespace Brimborium.Henderschefuere.Model;

public sealed record class HfRootModel(
    ImmutableDictionary<string, HfTunnelPair> Tunnels,
    ImmutableDictionary<string, HfRoutePair> Routes,
    ImmutableDictionary<string, HfClusterPair> Clusters
    ) {
    public HfRootModel() : this(
        ImmutableDictionary<string, HfTunnelPair>.Empty,
        ImmutableDictionary<string, HfRoutePair>.Empty,
        ImmutableDictionary<string, HfClusterPair>.Empty
        ) { }
};

public sealed record class HfTunnelPair(
    HfTunnelModel TunnelModel,
    HfTunnelState TunnelState
    ) {
    public string Id => this.TunnelModel.Id;
}

public sealed class HfTunnelState { }

public sealed record class HfRoutePair(
    HfRouteModel RouteModel,
    HfRouteState RouteState) {
    public string Id => this.RouteModel.Id;

    public HfClusterPair Cluster { get; internal set; } = null!;
}

public sealed class HfRouteState { }

public sealed record class HfClusterPair(
    HfClusterModel ClusterModel,
    ImmutableDictionary<string, HfClusterDestinationPair> Destinations,
    HfClusterState ClusterState) {
    public string Id => this.ClusterModel.Id;

    //public ImmutableDictionary<string, HfClusterDestinationPair> Destinations { get; set; } = ImmutableDictionary<string, HfClusterDestinationPair>.Empty;
}

public sealed record class HfClusterDestinationPair(
    HfClusterDestinationModel DestinationModel) {
    public string Id => this.DestinationModel.Id;
}

public sealed class HfClusterState { }