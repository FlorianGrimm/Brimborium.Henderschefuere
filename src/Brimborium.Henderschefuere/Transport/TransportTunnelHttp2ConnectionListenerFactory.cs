namespace Brimborium.Henderschefuere.Transport;

internal sealed class TransportTunnelHttp2ConnectionListenerFactory
    : IConnectionListenerFactory
    , IConnectionListenerFactorySelector {
    private readonly TransportTunnelHttp2Options _options;
    private readonly UnShortCitcuitOnceProxyConfigManager _proxyConfigManagerOnce;
    private readonly OptionalCertificateStore _optionalCertificateStore;
    private readonly ILogger _logger;

    public TransportTunnelHttp2ConnectionListenerFactory(
        IOptions<TransportTunnelHttp2Options> options,
        UnShortCitcuitOnceProxyConfigManager proxyConfigManagerOnce,
        OptionalCertificateStore optionalCertificateStore,
        ILogger<TransportTunnelHttp2ConnectionListener> logger
        ) {
        _options = options.Value;
        _proxyConfigManagerOnce = proxyConfigManagerOnce;
        _optionalCertificateStore = optionalCertificateStore;
        _logger = logger;
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

        return new(new TransportTunnelHttp2ConnectionListener(
            uriEndPointHttp2,
            tunnel,
            _options,
            _optionalCertificateStore,
            _logger));
    }
}
