namespace Brimborium.Henderschefuere.Model;

public sealed record class HfRootSnapshot(
    ImmutableDictionary<string, HfTunnelModel> Tunnels,
    ImmutableDictionary<string, HfRouteModel> Routes,
    ImmutableDictionary<string, HfClusterModel> Clusters
    );

public sealed record class HfTunnelModel(
    string Id,
    string Url,
    string RemoteTunnelId,
    HfTransport Transport,
    HfTunnelAuthentication? Authentication
    );

public sealed record class HfTunnelAuthentication {
}

public sealed record class HfRouteModel(
    string Id,
    string ClusterId,
    HfRouteMatch Match
    );

public sealed record class HfRouteMatch(
    string Id,
    string Path
    );


public sealed record class HfClusterModel(
    string Id,
    HfTransport Transport,
    ImmutableDictionary<string, HfClusterDestinationModel> Destinations
    );

public sealed record class HfClusterDestinationModel(
    string Id,
    string Address
    );
