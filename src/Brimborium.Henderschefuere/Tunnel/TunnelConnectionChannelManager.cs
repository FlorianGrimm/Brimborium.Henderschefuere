namespace Brimborium.Henderschefuere.Tunnel;

public sealed class TunnelConnectionChannelManager {
    private readonly ConcurrentDictionary<string, TunnelConnectionChannels> _clusterConnections = new();

    public TunnelConnectionChannelManager() {
    }

    public bool TryGetConnectionChannel(string clusterId, [MaybeNullWhen(false)] out TunnelConnectionChannels channels) {
        return _clusterConnections.TryGetValue(clusterId, out channels);
    }

    internal TunnelConnectionChannels? RegisterConnectionChannel(string clusterId) {
        var result = new TunnelConnectionChannels(Channel.CreateUnbounded<int>(), Channel.CreateUnbounded<Stream>());
        if (_clusterConnections.TryAdd(clusterId.ToLowerInvariant(), result)) {
            return result;
        } else {
            return default;
        }
    }
}

public sealed record TunnelConnectionChannels(
    Channel<int> Trigger,
    Channel<Stream> Streams
    );