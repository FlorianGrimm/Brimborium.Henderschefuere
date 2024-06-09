namespace Brimborium.Henderschefuere.Model;
public sealed record class HfRootSnapshot(
    ImmutableDictionary<string, HfTunnel> Tunnels,
    ImmutableDictionary<string, HfRoute> Routes,
    ImmutableDictionary<string, HfCluster> Clusters
    );

public sealed record class HfTunnel(
    string Id,
    string Url,
    string RemoteTunnelId,
    HfTransport Transport,
    HfTunnelAuthentication? Authentication
    );

public sealed record class HfTunnelAuthentication {
}

public sealed record class HfRoute(
    string Id,
    string ClusterId,
    HfRouteMatch Match
    );

public sealed record class HfRouteMatch(
    string Id,
    string Path
    );


public sealed record class HfCluster(
    string Id,
    HfTransport Transport,
    ImmutableDictionary<string, HfClusterDestination> Destinations
    );

public sealed record class HfClusterDestination(
    string Id,
    string Address
    );
