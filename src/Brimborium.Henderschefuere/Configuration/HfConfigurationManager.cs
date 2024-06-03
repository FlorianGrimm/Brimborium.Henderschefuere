namespace Brimborium.Henderschefuere.Configuration;

public sealed class HfConfigurationManager {
    private CompositeChangeToken? _ConfigurationChanged;
    private IDisposable? _ConfigurationRegistation;
    private ImmutableArray<IHfConfigurationSource> _ListConfigurations = ImmutableArray<IHfConfigurationSource>.Empty;
    private HfRootSnapshot? _Snapshot;
    private CancellationTokenSource _SnapshotChangeSource;
    private IChangeToken _SnapshotChangeToken;
    private HfConfiguration? _SourceConfiguration;

    public HfConfigurationManager() {
        this._SnapshotChangeSource = new CancellationTokenSource();
        this._SnapshotChangeToken = new CancellationChangeToken(this._SnapshotChangeSource.Token);
    }

    public ImmutableArray<IHfConfigurationSource> ListConfigurations => this._ListConfigurations;

    public void SetHenderschefuereConfiguration(HfConfiguration hfConfiguration) {
        this._SourceConfiguration = hfConfiguration;
        this.WireSourceConfiguration(hfConfiguration);
        this.LoadConfiguration();
    }
    private void WireSourceConfiguration(HfConfiguration hfConfiguration) {

        {
            List<IHfConfigurationSource> listConfiguration = new();
            foreach (var configuration in hfConfiguration.ListHfConfigurationSource) {
                listConfiguration.Add(configuration);
            }
            this._ListConfigurations = listConfiguration.ToImmutableArray();
        }
        {
            this._ConfigurationRegistation?.Dispose();
            this._ConfigurationRegistation = ChangeToken.OnChange(
                () => {
                    List<IChangeToken> listChangeToken = new();
                    foreach (var configuration in hfConfiguration.ListHfConfigurationSource) {
                        listChangeToken.Add(configuration.ConfigurationChanged);
                    }
                    return this._ConfigurationChanged = new CompositeChangeToken(listChangeToken);
                },
                () => {
                    System.Console.WriteLine("HfConfigurationManager ConfigurationChanged");
                    this.LoadConfiguration();
                });
        }
    }
    private HfRootSnapshot LoadConfiguration() {
        var hfConfiguration = this._SourceConfiguration;
        if (hfConfiguration is null) {
            return new HfRootSnapshot(
                Tunnels: ImmutableDictionary<string, HfTunnel>.Empty,
                Routes: ImmutableDictionary<string, HfRoute>.Empty,
                Clusters: ImmutableDictionary<string, HfCluster>.Empty
            );
        }
        HfRootSnapshot rootSnapshot;
        {
            Dictionary<string, HfTunnel> dictTunnels = new();
            Dictionary<string, HfRoute> dictRoutes = new();
            Dictionary<string, HfCluster> dictClusters = new();

            {
                foreach (var configuration in this._ListConfigurations) {
                    var rootConfiguration = configuration.GetRootConfiguration();
                    foreach (var routeConfig in rootConfiguration.Routes.Values) {
                        var routeId = routeConfig.Id;
                        if (string.IsNullOrWhiteSpace(routeId)) {
                            continue;
                        }
                        if (dictRoutes.TryGetValue(routeId, out var routeSnapshot)) {
                            if (needUpdate(routeSnapshot.ClusterId, routeConfig.ClusterId)) {
                                routeSnapshot = routeSnapshot with { ClusterId = routeConfig.ClusterId! };
                            }
                            var routeMatchSnapshot = routeSnapshot.Match ?? new HfRouteMatch(Id: routeId, Path: string.Empty);
                            if (needUpdate(routeMatchSnapshot.Path, routeConfig.Match?.Path)) {
                                routeMatchSnapshot = routeMatchSnapshot with { Path = routeConfig.Match?.Path ?? string.Empty };
                                routeSnapshot = routeSnapshot with { Match = routeMatchSnapshot };
                            }
                            dictRoutes[routeId] = routeSnapshot;
                        } else {
                            var routeMatch = new HfRouteMatch(
                                Id: routeId,
                                Path: routeConfig.Match?.Path ?? string.Empty
                                );
                            routeSnapshot = new HfRoute(
                                Id: routeId,
                                ClusterId: routeConfig.ClusterId ?? string.Empty,
                                Match: routeMatch
                            );
                            dictRoutes.Add(routeId, routeSnapshot);
                        }
                    }
                    //
                    foreach (var clusterConfig in rootConfiguration.Clusters.Values) {
                        var clusterId = clusterConfig.Id;
                        if (string.IsNullOrWhiteSpace(clusterId)) {
                            continue;
                        }
                        if (dictClusters.TryGetValue(clusterId, out var clusterSnapshot)) {
                            if (clusterConfig.Transport.HasValue
                                && (HfTransport.None != clusterConfig.Transport)
                                && needUpdate(clusterSnapshot.Transport, clusterConfig.Transport)) {
                                clusterSnapshot = clusterSnapshot with { Transport = clusterConfig.Transport.Value };
                            }
                            var dictDestinations = new Dictionary<string, HfClusterDestination>(clusterSnapshot.Destinations);
                            foreach (var destinationConfig in clusterConfig.Destinations.Values) {
                                var destinationId = destinationConfig.Id;
                                if (string.IsNullOrWhiteSpace(destinationId)) {
                                    continue;
                                }
                                if (dictDestinations.TryGetValue(destinationId, out var destinationSnapshot)) {
                                    if (needUpdate(destinationSnapshot.Address, destinationConfig.Address)) {
                                        destinationSnapshot = destinationSnapshot with { Address = destinationConfig.Address! };
                                    }
                                    dictDestinations[destinationId] = destinationSnapshot;
                                } else {
                                    destinationSnapshot = new HfClusterDestination(
                                        Id: destinationId,
                                        Address: destinationConfig.Address ?? string.Empty
                                        );
                                    dictDestinations.Add(destinationId, destinationSnapshot);
                                }
                            }
                            clusterSnapshot = clusterSnapshot with { Destinations = dictDestinations.ToImmutableDictionary() };
                            dictClusters[clusterId] = clusterSnapshot;
                        } else {
                            var dictDestinations = new Dictionary<string, HfClusterDestination>();
                            foreach (var destinationConfig in clusterConfig.Destinations.Values) {
                                var destinationId = destinationConfig.Id;
                                if (string.IsNullOrWhiteSpace(destinationId)) {
                                    continue;
                                }
                                var destinationSnapshot = new HfClusterDestination(
                                    Id: destinationId,
                                    Address: destinationConfig.Address ?? string.Empty
                                    );
                                dictDestinations.Add(destinationId, destinationSnapshot);
                            }
                            clusterSnapshot = new HfCluster(
                                Id: clusterId,
                                Transport: clusterConfig.Transport ?? HfTransport.None,
                                Destinations: dictDestinations.ToImmutableDictionary()
                                );
                            dictClusters.Add(clusterId, clusterSnapshot);
                        }
                    }
                    //
                    foreach (var tunnelConfig in rootConfiguration.Tunnels.Values) {
                        var tunnelId = tunnelConfig.Id;
                        if (string.IsNullOrWhiteSpace(tunnelId)) {
                            continue;
                        }
                        if (dictTunnels.TryGetValue(tunnelId, out var tunnelSnapshot)) {
                            if (needUpdate(tunnelSnapshot.Url, tunnelConfig.Url)) {
                                tunnelSnapshot = tunnelSnapshot with { Url = tunnelConfig.Url! };
                            }
                            if (needUpdate(tunnelSnapshot.RemoteTunnelId, tunnelConfig.RemoteTunnelId)) {
                                tunnelSnapshot = tunnelSnapshot with { RemoteTunnelId = tunnelConfig.RemoteTunnelId! };
                            }
                            if (tunnelConfig.Transport.HasValue
                                && (HfTransport.None != tunnelConfig.Transport)
                                && (needUpdate(tunnelSnapshot.Transport, tunnelConfig.Transport))) {
                                tunnelSnapshot = tunnelSnapshot with { Transport = tunnelConfig.Transport.Value };
                            }
                            dictTunnels[tunnelId] = tunnelSnapshot;
                        } else {
                            tunnelSnapshot = new HfTunnel(
                                Id: tunnelId,
                                Url: tunnelConfig.Url ?? string.Empty,
                                RemoteTunnelId: tunnelConfig.RemoteTunnelId ?? string.Empty,
                                Transport: tunnelConfig.Transport ?? HfTransport.None,
                                Authentication: new HfTunnelAuthentication()
                            );
                            dictTunnels.Add(tunnelId, tunnelSnapshot);
                        }
                    }
                    //
                }
            }
            {
                foreach (var namedTunnel in dictTunnels) {
                    var tunnelId = namedTunnel.Key;
                    var tunnelSnapshot = namedTunnel.Value;
                }
                foreach (var namedCluster in dictClusters) {
                    var clusterId = namedCluster.Key;
                    var clusterSnapshot = namedCluster.Value;
                }
            }

            rootSnapshot = new HfRootSnapshot(
                Tunnels: dictTunnels.ToImmutableDictionary(),
                Routes: dictRoutes.ToImmutableDictionary(),
                Clusters: dictClusters.ToImmutableDictionary()
                );
        }

        lock (this) {
            var oldCancellationTokenSource = this._SnapshotChangeSource;
            this._Snapshot = rootSnapshot;
            this._SnapshotChangeSource = new CancellationTokenSource();
            this._SnapshotChangeToken = new CancellationChangeToken(this._SnapshotChangeSource.Token);
            System.Threading.Interlocked.MemoryBarrier();
            oldCancellationTokenSource?.Cancel();
        }

        return rootSnapshot;
    }

    public HfRootSnapshot GetSnapshot() {
        if ((this._Snapshot is null)
            || (this._ConfigurationChanged is { HasChanged: true })
            ) {
            return this.LoadConfiguration();
        }
        this._Snapshot ??= new HfRootSnapshot(
            ImmutableDictionary<string, HfTunnel>.Empty,
            ImmutableDictionary<string, HfRoute>.Empty,
            ImmutableDictionary<string, HfCluster>.Empty);
        return this._Snapshot;
    }

    private static bool needUpdate(string? snapshotValue, string? configValue) {
        if (string.IsNullOrWhiteSpace(configValue)) { return false; }
        return !string.Equals(snapshotValue, configValue, StringComparison.Ordinal);
    }
    private static bool needUpdate<T>(T? snapshotValue, T? configValue)
        where T : struct {
        if (configValue is null) { return false; }
        return !configValue.Equals(snapshotValue);
    }
}


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
