namespace Brimborium.Henderschefuere.Transport;

internal sealed class TransportTunnelHttp2ConnectionListenerFactory
    : IConnectionListenerFactory
    , IConnectionListenerFactorySelector {
    private readonly UnShortCitcuitOnceProxyConfigManager _proxyConfigManagerOnce;
    private readonly TransportTunnelHttp2Options _options;

    public TransportTunnelHttp2ConnectionListenerFactory(
        UnShortCitcuitOnceProxyConfigManager proxyConfigManagerOnce,
        IOptions<TransportTunnelHttp2Options> options) {
        _options = options.Value;
        _proxyConfigManagerOnce = proxyConfigManagerOnce;
    }

    public bool CanBind(EndPoint endpoint) {
        return endpoint is UriEndPointHttp2;
    }

    public ValueTask<IConnectionListener> BindAsync(EndPoint endpoint, CancellationToken cancellationToken = default) {
        if (endpoint is not UriEndPointHttp2 uriEndPointHttp2) {
            throw new ArgumentException("Invalid endpoint type", nameof(endpoint));
        }
        var proxyConfigManager = _proxyConfigManagerOnce.GetService();
        var tunnelId = uriEndPointHttp2.TunnelId;
        if (!proxyConfigManager.TryGetTunnel(tunnelId, out var tunnel)) {
            throw new ArgumentException($"Tunnel: '{tunnelId} not found.'", nameof(endpoint));
        }
        return new(new TransportTunnelHttp2ConnectionListener(_options, tunnel, uriEndPointHttp2));
    }
}
