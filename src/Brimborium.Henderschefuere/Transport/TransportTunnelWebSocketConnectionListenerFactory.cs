namespace Brimborium.Henderschefuere.Transport;

internal sealed class TransportTunnelWebSocketConnectionListenerFactory : IConnectionListenerFactory, IConnectionListenerFactorySelector {
    private readonly TransportTunnelWebSocketOptions _options;
    private readonly UnShortCitcuitOnceProxyConfigManager _proxyConfigManagerOnce;
    private readonly OptionalCertificateStore _optionalCertificateStore;
    private readonly ILogger<TransportTunnelWebSocketConnectionListener> _logger;

    public TransportTunnelWebSocketConnectionListenerFactory(
        IOptions<TransportTunnelWebSocketOptions> options,
        UnShortCitcuitOnceProxyConfigManager proxyConfigManagerOnce,
        OptionalCertificateStore optionalCertificateStore,
        ILogger<TransportTunnelWebSocketConnectionListener> logger
        ) {
        _options = options.Value;
        _proxyConfigManagerOnce = proxyConfigManagerOnce;
        _optionalCertificateStore = optionalCertificateStore;
        _logger = logger;
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

        return new(new TransportTunnelWebSocketConnectionListener(
            uriEndpointWebSocket,
            tunnel,
            _options,
            _optionalCertificateStore,
            _logger
            ));
    }
}
