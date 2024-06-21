namespace Brimborium.Henderschefuere.Tunnel;

public sealed class TunnelConnectionChannelManager
    : IClusterChangeListener {
    private readonly ConcurrentDictionary<string, TunnelConnectionChannels> _clusterConnections = new(StringComparer.OrdinalIgnoreCase);

    public bool TryGetConnectionChannel(string clusterId, [MaybeNullWhen(false)] out TunnelConnectionChannels channels) {
        return _clusterConnections.TryGetValue(clusterId, out channels);
    }

    internal void RegisterConnectionChannel(string clusterId) {
        if (_clusterConnections.ContainsKey(clusterId)) { return; }

        var result = new TunnelConnectionChannels(Channel.CreateUnbounded<int>(), Channel.CreateUnbounded<Stream>());
        _clusterConnections.TryAdd(clusterId, result);
    }

    void IClusterChangeListener.OnClusterAdded(ClusterState cluster) {
        RegisterConnectionChannel(cluster.ClusterId);
    }

    void IClusterChangeListener.OnClusterChanged(ClusterState cluster) {
    }

    void IClusterChangeListener.OnClusterRemoved(ClusterState cluster) {
        _clusterConnections.TryRemove(cluster.ClusterId, out _);
    }
}

public sealed record TunnelConnectionChannels(
    Channel<int> Trigger,
    Channel<Stream> Streams
    ) {
    public int CountSource;
    public int CountSink;
}