namespace Brimborium.Henderschefuere.Transport;

internal sealed class TransportTunnelWebSocketConnectionListenerFactory : IConnectionListenerFactory, IConnectionListenerFactorySelector {
    private readonly UnShortCitcuitOnceProxyConfigManager _proxyConfigManagerOnce;
    private readonly TransportTunnelWebSocketOptions _options;

    public TransportTunnelWebSocketConnectionListenerFactory(
        UnShortCitcuitOnceProxyConfigManager proxyConfigManagerOnce,
        IOptions<TransportTunnelWebSocketOptions> options) {
        _options = options.Value;
        _proxyConfigManagerOnce = proxyConfigManagerOnce;
    }

    public bool CanBind(EndPoint endpoint) {
        return endpoint is UriWebSocketEndPoint;
    }

    public ValueTask<IConnectionListener> BindAsync(EndPoint endpoint, CancellationToken cancellationToken = default) {
        if (endpoint is not UriWebSocketEndPoint uriEndpointWebSocket) {
            throw new ArgumentException("Invalid endpoint type", nameof(endpoint));
        }

        var proxyConfigManager = _proxyConfigManagerOnce.GetService();
        var tunnelId = uriEndpointWebSocket.TunnelId;
        if (!proxyConfigManager.TryGetTunnel(tunnelId, out var tunnel)) {
            throw new ArgumentException($"Tunnel: '{tunnelId} not found.'", nameof(endpoint));
        }

        return new(new TransportTunnelWebSocketConnectionListener(_options, tunnel, uriEndpointWebSocket));
    }
}
